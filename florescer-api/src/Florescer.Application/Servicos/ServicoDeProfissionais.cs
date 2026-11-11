using Florescer.Application.Contratos;
using Florescer.Application.Dtos;
using Florescer.Application.Persistencia;
using Florescer.Domain.Entidades;
using Florescer.Domain.Enums;
using Florescer.Domain.Regras;
using Microsoft.EntityFrameworkCore;

namespace Florescer.Application.Servicos;

public class ServicoDeProfissionais
{
    private readonly IBancoDeDados _banco;
    private readonly IRelogio _relogio;

    public ServicoDeProfissionais(IBancoDeDados banco, IRelogio relogio)
    {
        _banco = banco;
        _relogio = relogio;
    }

    public async Task<IReadOnlyList<ProfissionalResposta>> ListarAsync(
        bool apenasAtivas = false,
        int? procedimentoId = null,
        int? clienteId = null,
        CancellationToken ct = default)
    {
        var consulta = _banco.Profissionais
            .Include(p => p.HorariosTrabalho)
            .Include(p => p.ProcedimentosHabilitados)
            .AsQueryable();

        if (apenasAtivas)
            consulta = consulta.Where(p => p.Ativa);

        if (procedimentoId.HasValue)
        {
            consulta = consulta.Where(p =>
                p.ProcedimentosHabilitados.Any(pp => pp.ProcedimentoId == procedimentoId.Value));
        }

        var profissionais = await consulta.OrderBy(p => p.Nome).ToListAsync(ct);

        var historico = await ObterHistoricoDaClienteAsync(clienteId, ct);

        return profissionais
            .Select(p =>
            {
                historico.TryGetValue(p.Id, out var dados);
                return Converter(p, dados.Quantidade, dados.Ultimo);
            })
            .OrderByDescending(p => p.JaAtendeu)
            .ThenByDescending(p => p.UltimoAtendimento)
            .ThenBy(p => p.Nome)
            .ToList();
    }

    private async Task<Dictionary<int, (int Quantidade, DateTime? Ultimo)>> ObterHistoricoDaClienteAsync(
        int? clienteId, CancellationToken ct)
    {
        if (!clienteId.HasValue)
            return new Dictionary<int, (int, DateTime?)>();

        var dados = await _banco.Agendamentos
            .Where(a => a.ClienteId == clienteId.Value &&
                        (a.Status == StatusAgendamento.Concluido ||
                         a.Status == StatusAgendamento.Confirmado))
            .GroupBy(a => a.ProfissionalId)
            .Select(g => new
            {
                ProfissionalId = g.Key,
                Quantidade = g.Count(),
                Ultimo = g.Max(a => a.Inicio)
            })
            .ToListAsync(ct);

        return dados.ToDictionary(
            d => d.ProfissionalId,
            d => (d.Quantidade, (DateTime?)d.Ultimo));
    }

    public async Task<ProfissionalResposta> ObterAsync(int id, CancellationToken ct = default)
    {
        var profissional = await BuscarAsync(id, ct);
        return Converter(profissional);
    }

    public async Task<ProfissionalResposta> CriarAsync(
        ProfissionalRequisicao dados, CancellationToken ct = default)
    {
        Validar(dados);

        var profissional = new Profissional
        {
            Nome = dados.Nome.Trim(),
            Especialidade = dados.Especialidade.Trim(),
            Email = dados.Email?.Trim(),
            Telefone = dados.Telefone?.Trim(),
            Ativa = dados.Ativa,
            CriadoEm = _relogio.Agora
        };

        AplicarJornada(profissional, dados);
        AplicarProcedimentos(profissional, dados.ProcedimentoIds);

        _banco.Profissionais.Add(profissional);
        await _banco.SalvarAsync(ct);

        return Converter(profissional);
    }

    public async Task<ProfissionalResposta> AtualizarAsync(
        int id, ProfissionalRequisicao dados, CancellationToken ct = default)
    {
        Validar(dados);

        var profissional = await BuscarAsync(id, ct);

        profissional.Nome = dados.Nome.Trim();
        profissional.Especialidade = dados.Especialidade.Trim();
        profissional.Email = dados.Email?.Trim();
        profissional.Telefone = dados.Telefone?.Trim();
        profissional.Ativa = dados.Ativa;

        _banco.HorariosTrabalho.RemoveRange(profissional.HorariosTrabalho);
        profissional.HorariosTrabalho.Clear();
        AplicarJornada(profissional, dados);

        _banco.ProfissionaisProcedimentos.RemoveRange(profissional.ProcedimentosHabilitados);
        profissional.ProcedimentosHabilitados.Clear();
        AplicarProcedimentos(profissional, dados.ProcedimentoIds);

        await _banco.SalvarAsync(ct);

        return Converter(profissional);
    }

    public async Task DesativarAsync(int id, CancellationToken ct = default)
    {
        var profissional = await BuscarAsync(id, ct);

        var temAgendamentosFuturos = await _banco.Agendamentos.AnyAsync(a =>
            a.ProfissionalId == id &&
            a.Inicio > _relogio.Agora &&
            (a.Status == StatusAgendamento.Agendado || a.Status == StatusAgendamento.Confirmado), ct);

        if (temAgendamentosFuturos)
        {
            throw new RegraDeNegocioException(
                "Esta profissional possui atendimentos futuros. Cancele ou transfira os agendamentos antes de desativá-la.");
        }

        profissional.Ativa = false;
        await _banco.SalvarAsync(ct);
    }

    public async Task<IReadOnlyList<BloqueioResposta>> ListarBloqueiosAsync(
        DateTime? de = null, DateTime? ate = null, CancellationToken ct = default)
    {
        var inicio = de ?? _relogio.Hoje;
        var fim = ate ?? inicio.AddMonths(3);

        var bloqueios = await _banco.Bloqueios
            .Include(b => b.Profissional)
            .Where(b => b.Fim >= inicio && b.Inicio <= fim)
            .OrderBy(b => b.Inicio)
            .ToListAsync(ct);

        return bloqueios.Select(b => new BloqueioResposta(
            b.Id, b.ProfissionalId, b.Profissional?.Nome,
            b.Inicio, b.Fim, Textos.TipoDeBloqueio(b.Tipo), b.Motivo)).ToList();
    }

    public async Task<BloqueioResposta> CriarBloqueioAsync(
        BloqueioRequisicao dados, CancellationToken ct = default)
    {
        if (dados.Fim <= dados.Inicio)
            throw new RegraDeNegocioException("O fim do bloqueio deve ser posterior ao início.");

        var conflitantes = await _banco.Agendamentos
            .Include(a => a.Cliente)
            .Where(a =>
                (a.Status == StatusAgendamento.Agendado || a.Status == StatusAgendamento.Confirmado) &&
                a.Inicio < dados.Fim && dados.Inicio < a.Fim &&
                (dados.ProfissionalId == null || a.ProfissionalId == dados.ProfissionalId))
            .CountAsync(ct);

        if (conflitantes > 0)
        {
            throw new RegraDeNegocioException(
                $"Existem {conflitantes} atendimento(s) marcados neste período. Cancele ou remarque antes de bloquear.");
        }

        var bloqueio = new Bloqueio
        {
            ProfissionalId = dados.ProfissionalId,
            Inicio = dados.Inicio,
            Fim = dados.Fim,
            Tipo = (TipoBloqueio)dados.Tipo,
            Motivo = dados.Motivo?.Trim(),
            CriadoEm = _relogio.Agora
        };

        _banco.Bloqueios.Add(bloqueio);
        await _banco.SalvarAsync(ct);

        var profissional = dados.ProfissionalId.HasValue
            ? await _banco.Profissionais.FirstOrDefaultAsync(p => p.Id == dados.ProfissionalId, ct)
            : null;

        return new BloqueioResposta(
            bloqueio.Id, bloqueio.ProfissionalId, profissional?.Nome,
            bloqueio.Inicio, bloqueio.Fim, Textos.TipoDeBloqueio(bloqueio.Tipo), bloqueio.Motivo);
    }

    public async Task RemoverBloqueioAsync(int id, CancellationToken ct = default)
    {
        var bloqueio = await _banco.Bloqueios.FirstOrDefaultAsync(b => b.Id == id, ct)
            ?? throw new RegraDeNegocioException("Bloqueio não encontrado.");

        _banco.Bloqueios.Remove(bloqueio);
        await _banco.SalvarAsync(ct);
    }

    private static void Validar(ProfissionalRequisicao dados)
    {
        if (string.IsNullOrWhiteSpace(dados.Nome))
            throw new RegraDeNegocioException("Informe o nome da profissional.");

        if (string.IsNullOrWhiteSpace(dados.Especialidade))
            throw new RegraDeNegocioException("Informe a especialidade.");

        if (dados.Dias is null || dados.Dias.Count != 7)
            throw new RegraDeNegocioException("Informe os sete dias da semana de trabalho.");

        if (dados.Dias.All(d => d == 0))
            throw new RegraDeNegocioException("Selecione ao menos um dia de trabalho.");
    }

    private static void AplicarJornada(Profissional profissional, ProfissionalRequisicao dados)
    {
        var inicio = TimeSpan.Parse(dados.HoraInicio);
        var fim = TimeSpan.Parse(dados.HoraFim);

        if (fim <= inicio)
            throw new RegraDeNegocioException("O horário final deve ser posterior ao inicial.");

        for (var posicao = 0; posicao < dados.Dias.Count; posicao++)
        {
            if (dados.Dias[posicao] == 0) continue;

            profissional.HorariosTrabalho.Add(new HorarioTrabalho
            {
                DiaSemana = ConverterParaDiaDaSemana(posicao),
                HoraInicio = inicio,
                HoraFim = fim
            });
        }
    }

    private static void AplicarProcedimentos(Profissional profissional, IReadOnlyList<int> procedimentoIds)
    {
        foreach (var procedimentoId in procedimentoIds.Distinct())
        {
            profissional.ProcedimentosHabilitados.Add(new ProfissionalProcedimento
            {
                ProcedimentoId = procedimentoId
            });
        }
    }

    private static DayOfWeek ConverterParaDiaDaSemana(int posicao) => posicao switch
    {
        0 => DayOfWeek.Monday,
        1 => DayOfWeek.Tuesday,
        2 => DayOfWeek.Wednesday,
        3 => DayOfWeek.Thursday,
        4 => DayOfWeek.Friday,
        5 => DayOfWeek.Saturday,
        _ => DayOfWeek.Sunday
    };

    public static int ConverterParaPosicaoDoFront(DayOfWeek dia) => dia switch
    {
        DayOfWeek.Monday => 0,
        DayOfWeek.Tuesday => 1,
        DayOfWeek.Wednesday => 2,
        DayOfWeek.Thursday => 3,
        DayOfWeek.Friday => 4,
        DayOfWeek.Saturday => 5,
        _ => 6
    };

    private async Task<Profissional> BuscarAsync(int id, CancellationToken ct)
    {
        var profissional = await _banco.Profissionais
            .Include(p => p.HorariosTrabalho)
            .Include(p => p.ProcedimentosHabilitados)
            .FirstOrDefaultAsync(p => p.Id == id, ct);

        return profissional ?? throw new RegraDeNegocioException("Profissional não encontrada.");
    }

    private static ProfissionalResposta Converter(
        Profissional p, int atendimentosAnteriores = 0, DateTime? ultimoAtendimento = null)
    {
        var dias = new int[7];
        foreach (var horario in p.HorariosTrabalho)
            dias[ConverterParaPosicaoDoFront(horario.DiaSemana)] = 1;

        return new ProfissionalResposta(
            p.Id, p.Nome, p.Especialidade, p.Email, p.Telefone, p.FotoUrl, p.Ativa,
            dias,
            p.ProcedimentosHabilitados.Select(pp => pp.ProcedimentoId).ToList(),
            atendimentosAnteriores > 0,
            atendimentosAnteriores,
            ultimoAtendimento);
    }
}
