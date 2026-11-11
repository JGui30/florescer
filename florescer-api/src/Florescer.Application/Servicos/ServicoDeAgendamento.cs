using Florescer.Application.Contratos;
using Florescer.Application.Dtos;
using Florescer.Application.Persistencia;
using Florescer.Domain.Entidades;
using Florescer.Domain.Enums;
using Florescer.Domain.Regras;
using Microsoft.EntityFrameworkCore;

namespace Florescer.Application.Servicos;

public class ServicoDeAgendamento
{
    private readonly IBancoDeDados _banco;
    private readonly IRelogio _relogio;
    private readonly ServicoDeDisponibilidade _disponibilidade;
    private readonly ServicoDeNotificacao _notificacoes;

    public ServicoDeAgendamento(
        IBancoDeDados banco,
        IRelogio relogio,
        ServicoDeDisponibilidade disponibilidade,
        ServicoDeNotificacao notificacoes)
    {
        _banco = banco;
        _relogio = relogio;
        _disponibilidade = disponibilidade;
        _notificacoes = notificacoes;
    }

    public async Task<PreviaDeAgendamentoResposta> CalcularPreviaAsync(
        int procedimentoId, DateTime inicio, CancellationToken ct = default)
    {
        var procedimento = await _banco.Procedimentos.FirstOrDefaultAsync(p => p.Id == procedimentoId, ct)
            ?? throw new RegraDeNegocioException("Procedimento não encontrado.");

        var agora = _relogio.Agora;
        var fim = inicio.AddMinutes(procedimento.DuracaoMinutos);
        var nasceConfirmado = RegrasDeConfirmacao.NasceConfirmado(agora, inicio);
        var prazo = RegrasDeConfirmacao.CalcularPrazo(agora, inicio);

        var aviso = nasceConfirmado
            ? "Como faltam menos de 2 horas para o atendimento, seu agendamento já será criado confirmado."
            : RegrasDeConfirmacao.MensagemDoPrazo(prazo);

        return new PreviaDeAgendamentoResposta(
            inicio, fim, procedimento.Preco, prazo, nasceConfirmado, aviso);
    }

    public async Task<AgendamentoResposta> CriarAsync(
        int clienteId, CriarAgendamentoRequisicao dados, CancellationToken ct = default)
    {
        var agora = _relogio.Agora;

        if (dados.Inicio <= agora)
            throw new RegraDeNegocioException("Escolha um horário futuro.");

        if (dados.Inicio > agora.AddDays(RegrasDeAgenda.DiasDeAgendaAberta))
        {
            throw new RegraDeNegocioException(
                $"A agenda está aberta para os próximos {RegrasDeAgenda.DiasDeAgendaAberta} dias.");
        }

        var procedimento = await _banco.Procedimentos
            .Include(p => p.Categoria)
            .FirstOrDefaultAsync(p => p.Id == dados.ProcedimentoId && p.Ativo, ct)
            ?? throw new RegraDeNegocioException("Procedimento não encontrado ou indisponível.");

        var profissional = await _banco.Profissionais
            .Include(p => p.ProcedimentosHabilitados)
            .FirstOrDefaultAsync(p => p.Id == dados.ProfissionalId && p.Ativa, ct)
            ?? throw new RegraDeNegocioException("Profissional não encontrada ou indisponível.");

        if (!profissional.ProcedimentosHabilitados.Any(pp => pp.ProcedimentoId == procedimento.Id))
            throw new RegraDeNegocioException($"{profissional.Nome} não realiza este procedimento.");

        var fim = dados.Inicio.AddMinutes(procedimento.DuracaoMinutos);

        await _disponibilidade.GarantirQueEstaLivreAsync(
            profissional.Id, dados.Inicio, fim, null, ct);

        var cliente = await _banco.Usuarios.FirstOrDefaultAsync(u => u.Id == clienteId, ct)
            ?? throw new RegraDeNegocioException("Cliente não encontrado.");

        var nasceConfirmado = RegrasDeConfirmacao.NasceConfirmado(agora, dados.Inicio);

        var agendamento = new Agendamento
        {
            ClienteId = cliente.Id,
            Cliente = cliente,
            ProfissionalId = profissional.Id,
            Profissional = profissional,
            ProcedimentoId = procedimento.Id,
            Procedimento = procedimento,
            Inicio = dados.Inicio,
            Fim = fim,
            ValorPrevisto = procedimento.Preco,
            PrazoConfirmacao = RegrasDeConfirmacao.CalcularPrazo(agora, dados.Inicio),
            Status = nasceConfirmado ? StatusAgendamento.Confirmado : StatusAgendamento.Agendado,
            ConfirmadoEm = nasceConfirmado ? agora : null,
            Observacoes = dados.Observacoes?.Trim(),
            CriadoEm = agora
        };

        _banco.Agendamentos.Add(agendamento);
        await _banco.SalvarAsync(ct);

        await _notificacoes.EnviarAsync(
            cliente,
            TipoNotificacao.AgendamentoCriado,
            "Agendamento recebido — Florescer",
            ServicoDeNotificacao.TextoAgendamentoCriado(agendamento, procedimento.Nome, profissional.Nome),
            agendamento,
            ct);

        return Converter(agendamento);
    }

    public async Task<AgendamentoResposta> ConfirmarAsync(
        int agendamentoId, int? clienteId, CancellationToken ct = default)
    {
        var agendamento = await BuscarCompletoAsync(agendamentoId, ct);

        if (clienteId.HasValue && agendamento.ClienteId != clienteId.Value)
            throw new RegraDeNegocioException("Este agendamento pertence a outro cliente.");

        if (agendamento.Status == StatusAgendamento.Confirmado)
            return Converter(agendamento);

        if (agendamento.Status != StatusAgendamento.Agendado)
        {
            throw new RegraDeNegocioException(
                $"Não é possível confirmar um agendamento com status {Textos.Status(agendamento.Status)}.");
        }

        if (RegrasDeConfirmacao.PrazoVencido(_relogio.Agora, agendamento.PrazoConfirmacao))
        {
            throw new RegraDeNegocioException(
                "O prazo de confirmação venceu. Por favor, faça um novo agendamento.");
        }

        agendamento.Status = StatusAgendamento.Confirmado;
        agendamento.ConfirmadoEm = _relogio.Agora;

        await _banco.SalvarAsync(ct);

        await _notificacoes.EnviarAsync(
            agendamento.Cliente,
            TipoNotificacao.AgendamentoConfirmado,
            "Agendamento confirmado — Florescer",
            ServicoDeNotificacao.TextoAgendamentoConfirmado(
                agendamento, agendamento.Procedimento.Nome, agendamento.Profissional.Nome),
            agendamento,
            ct);

        return Converter(agendamento);
    }

    public async Task<AgendamentoResposta> CancelarAsync(
        int agendamentoId,
        int? clienteId,
        bool porAdministrador,
        CancellationToken ct = default)
    {
        var agendamento = await BuscarCompletoAsync(agendamentoId, ct);

        if (clienteId.HasValue && agendamento.ClienteId != clienteId.Value)
            throw new RegraDeNegocioException("Este agendamento pertence a outro cliente.");

        if (agendamento.Status == StatusAgendamento.Cancelado)
            return Converter(agendamento);

        if (agendamento.Status == StatusAgendamento.Concluido)
            throw new RegraDeNegocioException("Um atendimento já concluído não pode ser cancelado.");

        if (!porAdministrador && !RegrasDeAgenda.ClientePodeAlterar(_relogio.Agora, agendamento.Inicio))
        {
            throw new RegraDeNegocioException(
                $"Cancelamentos pelo aplicativo são permitidos até {RegrasDeAgenda.HorasMinimasParaCancelarOuReagendar} " +
                "horas antes do atendimento. Entre em contato com a recepção.");
        }

        agendamento.Status = StatusAgendamento.Cancelado;
        agendamento.CanceladoEm = _relogio.Agora;
        agendamento.MotivoCancelamento = porAdministrador
            ? MotivoCancelamento.CanceladoPeloAdministrador
            : MotivoCancelamento.CanceladoPeloCliente;

        await _banco.SalvarAsync(ct);

        await _notificacoes.EnviarAsync(
            agendamento.Cliente,
            TipoNotificacao.AgendamentoCancelado,
            "Agendamento cancelado — Florescer",
            ServicoDeNotificacao.TextoCancelado(agendamento, agendamento.Procedimento.Nome),
            agendamento,
            ct);

        return Converter(agendamento);
    }

    public async Task<AgendamentoResposta> ReagendarAsync(
        int agendamentoId,
        int? clienteId,
        ReagendarRequisicao dados,
        bool porAdministrador,
        CancellationToken ct = default)
    {
        var agendamento = await BuscarCompletoAsync(agendamentoId, ct);

        if (clienteId.HasValue && agendamento.ClienteId != clienteId.Value)
            throw new RegraDeNegocioException("Este agendamento pertence a outro cliente.");

        if (agendamento.Status == StatusAgendamento.Concluido)
            throw new RegraDeNegocioException("Um atendimento já concluído não pode ser remarcado.");

        if (!porAdministrador && !RegrasDeAgenda.ClientePodeAlterar(_relogio.Agora, agendamento.Inicio))
        {
            throw new RegraDeNegocioException(
                $"Remarcações pelo aplicativo são permitidas até {RegrasDeAgenda.HorasMinimasParaCancelarOuReagendar} " +
                "horas antes do atendimento. Entre em contato com a recepção.");
        }

        var agora = _relogio.Agora;

        if (dados.NovoInicio <= agora)
            throw new RegraDeNegocioException("Escolha um horário futuro.");

        var profissionalId = dados.NovoProfissionalId ?? agendamento.ProfissionalId;

        if (profissionalId != agendamento.ProfissionalId)
        {
            var novaProfissional = await _banco.Profissionais
                .Include(p => p.ProcedimentosHabilitados)
                .FirstOrDefaultAsync(p => p.Id == profissionalId && p.Ativa, ct)
                ?? throw new RegraDeNegocioException("Profissional não encontrada.");

            if (!novaProfissional.ProcedimentosHabilitados.Any(pp => pp.ProcedimentoId == agendamento.ProcedimentoId))
                throw new RegraDeNegocioException($"{novaProfissional.Nome} não realiza este procedimento.");
        }

        var novoFim = dados.NovoInicio.AddMinutes(agendamento.Procedimento.DuracaoMinutos);

        await _disponibilidade.GarantirQueEstaLivreAsync(
            profissionalId, dados.NovoInicio, novoFim, agendamento.Id, ct);

        agendamento.ProfissionalId = profissionalId;
        agendamento.Inicio = dados.NovoInicio;
        agendamento.Fim = novoFim;

        var nasceConfirmado = RegrasDeConfirmacao.NasceConfirmado(agora, dados.NovoInicio);
        agendamento.PrazoConfirmacao = RegrasDeConfirmacao.CalcularPrazo(agora, dados.NovoInicio);
        agendamento.Status = nasceConfirmado ? StatusAgendamento.Confirmado : StatusAgendamento.Agendado;
        agendamento.ConfirmadoEm = nasceConfirmado ? agora : null;
        agendamento.CanceladoEm = null;
        agendamento.MotivoCancelamento = null;

        await _banco.SalvarAsync(ct);

        var atualizado = await BuscarCompletoAsync(agendamentoId, ct);

        await _notificacoes.EnviarAsync(
            atualizado.Cliente,
            TipoNotificacao.AgendamentoReagendado,
            "Agendamento remarcado — Florescer",
            ServicoDeNotificacao.TextoReagendado(atualizado, atualizado.Procedimento.Nome),
            atualizado,
            ct);

        return Converter(atualizado);
    }

    public async Task<AgendamentoResposta> ConcluirAsync(
        int agendamentoId,
        int administradorId,
        ConcluirAtendimentoRequisicao dados,
        CancellationToken ct = default)
    {
        var agendamento = await BuscarCompletoAsync(agendamentoId, ct);

        if (agendamento.Status == StatusAgendamento.Cancelado)
            throw new RegraDeNegocioException("Um agendamento cancelado não pode ser concluído.");

        if (agendamento.Status == StatusAgendamento.Concluido)
            throw new RegraDeNegocioException("Este atendimento já foi concluído.");

        var valor = dados.ValorCobrado ?? agendamento.ValorPrevisto;

        if (valor < 0)
            throw new RegraDeNegocioException("O valor cobrado não pode ser negativo.");

        agendamento.Status = StatusAgendamento.Concluido;
        agendamento.ConcluidoEm = _relogio.Agora;
        agendamento.ConfirmadoEm ??= _relogio.Agora;

        _banco.Lancamentos.Add(new LancamentoFinanceiro
        {
            AgendamentoId = agendamento.Id,
            Tipo = TipoLancamento.Receita,
            Valor = valor,
            Data = agendamento.Inicio.Date,
            Descricao = $"{agendamento.Procedimento.Nome} — {agendamento.Cliente.Nome}",
            RegistradoPorUsuarioId = administradorId,
            CriadoEm = _relogio.Agora
        });

        await _banco.SalvarAsync(ct);

        return Converter(await BuscarCompletoAsync(agendamentoId, ct));
    }

    public async Task<AgendamentoResposta> RegistrarFaltaAsync(
        int agendamentoId, CancellationToken ct = default)
    {
        var agendamento = await BuscarCompletoAsync(agendamentoId, ct);

        if (agendamento.Status == StatusAgendamento.Concluido)
            throw new RegraDeNegocioException("Este atendimento já foi concluído.");

        agendamento.Status = StatusAgendamento.Cancelado;
        agendamento.CanceladoEm = _relogio.Agora;
        agendamento.MotivoCancelamento = MotivoCancelamento.ClienteNaoCompareceu;

        await _banco.SalvarAsync(ct);
        return Converter(agendamento);
    }

    public async Task<IReadOnlyList<AgendamentoResposta>> ListarDoClienteAsync(
        int clienteId, CancellationToken ct = default)
    {
        var agendamentos = await ConsultaCompleta()
            .Where(a => a.ClienteId == clienteId)
            .OrderByDescending(a => a.Inicio)
            .ToListAsync(ct);

        return agendamentos.Select(Converter).ToList();
    }

    public async Task<IReadOnlyList<AgendamentoResposta>> ListarAsync(
        DateTime? data = null,
        DateTime? de = null,
        DateTime? ate = null,
        int? profissionalId = null,
        StatusAgendamento? status = null,
        CancellationToken ct = default)
    {
        var consulta = ConsultaCompleta();

        if (data.HasValue)
        {
            var inicioDoDia = data.Value.Date;
            var fimDoDia = inicioDoDia.AddDays(1);
            consulta = consulta.Where(a => a.Inicio >= inicioDoDia && a.Inicio < fimDoDia);
        }
        else if (de.HasValue || ate.HasValue)
        {
            var inicio = (de ?? _relogio.Hoje).Date;
            var fim = (ate ?? inicio.AddMonths(1)).Date.AddDays(1);
            consulta = consulta.Where(a => a.Inicio >= inicio && a.Inicio < fim);
        }

        if (profissionalId.HasValue)
            consulta = consulta.Where(a => a.ProfissionalId == profissionalId.Value);

        if (status.HasValue)
            consulta = consulta.Where(a => a.Status == status.Value);

        var agendamentos = await consulta.OrderBy(a => a.Inicio).ToListAsync(ct);
        return agendamentos.Select(Converter).ToList();
    }

    public async Task<IReadOnlyList<DiaComMovimentoResposta>> ListarDiasComMovimentoAsync(
        DateTime de, DateTime ate, int? profissionalId = null, CancellationToken ct = default)
    {
        var inicio = de.Date;
        var fim = ate.Date.AddDays(1);

        var consulta = _banco.Agendamentos.Where(a => a.Inicio >= inicio && a.Inicio < fim);

        if (profissionalId.HasValue)
            consulta = consulta.Where(a => a.ProfissionalId == profissionalId.Value);

        var porDia = await consulta
            .GroupBy(a => a.Inicio.Date)
            .Select(g => new
            {
                Data = g.Key,
                Total = g.Count(),
                Confirmados = g.Count(a => a.Status == StatusAgendamento.Confirmado),
                Pendentes = g.Count(a => a.Status == StatusAgendamento.Agendado),
                Cancelados = g.Count(a => a.Status == StatusAgendamento.Cancelado),
                Concluidos = g.Count(a => a.Status == StatusAgendamento.Concluido)
            })
            .OrderBy(x => x.Data)
            .ToListAsync(ct);

        return porDia.Select(x => new DiaComMovimentoResposta(
            x.Data, x.Total, x.Confirmados, x.Pendentes, x.Cancelados, x.Concluidos)).ToList();
    }

    public async Task<PeriodoDaAgendaResposta> ObterPeriodoDaAgendaAsync(CancellationToken ct = default)
    {
        if (!await _banco.Agendamentos.AnyAsync(ct))
            return new PeriodoDaAgendaResposta(null, null);

        var primeiro = await _banco.Agendamentos.MinAsync(a => a.Inicio, ct);
        var ultimo = await _banco.Agendamentos.MaxAsync(a => a.Inicio, ct);

        return new PeriodoDaAgendaResposta(primeiro, ultimo);
    }

    private IQueryable<Agendamento> ConsultaCompleta()
        => _banco.Agendamentos
            .Include(a => a.Cliente)
            .Include(a => a.Profissional)
            .Include(a => a.Procedimento).ThenInclude(p => p.Categoria)
            .Include(a => a.Lancamento);

    private async Task<Agendamento> BuscarCompletoAsync(int id, CancellationToken ct)
    {
        var agendamento = await ConsultaCompleta().FirstOrDefaultAsync(a => a.Id == id, ct);
        return agendamento ?? throw new RegraDeNegocioException("Agendamento não encontrado.");
    }

    public AgendamentoResposta Converter(Agendamento a)
    {
        var agora = _relogio.Agora;
        var precisaConfirmar = a.Status == StatusAgendamento.Agendado;
        var prazoVencido = precisaConfirmar && RegrasDeConfirmacao.PrazoVencido(agora, a.PrazoConfirmacao);
        var podeAlterar = RegrasDeAgenda.ClientePodeAlterar(agora, a.Inicio);
        var emAberto = a.Status == StatusAgendamento.Agendado || a.Status == StatusAgendamento.Confirmado;

        string? aviso = null;
        if (precisaConfirmar && !prazoVencido)
            aviso = RegrasDeConfirmacao.MensagemDoPrazo(a.PrazoConfirmacao);

        return new AgendamentoResposta(
            a.Id,
            a.ClienteId,
            a.Cliente?.Nome ?? string.Empty,
            a.Cliente?.Telefone,
            a.ProfissionalId,
            a.Profissional?.Nome ?? string.Empty,
            a.ProcedimentoId,
            a.Procedimento?.Nome ?? string.Empty,
            a.Procedimento?.Categoria?.Slug ?? string.Empty,
            a.Procedimento?.Categoria?.Nome ?? string.Empty,
            a.Inicio,
            a.Fim,
            Textos.DataCurta(a.Inicio),
            Textos.Hora(a.Inicio),
            Textos.Status(a.Status),
            a.ValorPrevisto,
            a.Lancamento?.Valor,
            a.PrazoConfirmacao,
            precisaConfirmar,
            prazoVencido,
            emAberto && podeAlterar,
            emAberto && podeAlterar,
            a.MotivoCancelamento.HasValue ? Textos.Motivo(a.MotivoCancelamento.Value) : null,
            aviso,
            a.Observacoes);
    }
}
