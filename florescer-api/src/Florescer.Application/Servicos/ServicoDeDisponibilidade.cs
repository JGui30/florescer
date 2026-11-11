using Florescer.Application.Contratos;
using Florescer.Application.Dtos;
using Florescer.Application.Persistencia;
using Florescer.Domain.Entidades;
using Florescer.Domain.Enums;
using Florescer.Domain.Regras;
using Microsoft.EntityFrameworkCore;

namespace Florescer.Application.Servicos;

public class ServicoDeDisponibilidade
{
    private readonly IBancoDeDados _banco;
    private readonly IRelogio _relogio;

    public ServicoDeDisponibilidade(IBancoDeDados banco, IRelogio relogio)
    {
        _banco = banco;
        _relogio = relogio;
    }

    public async Task<DisponibilidadeDoDiaResposta> ObterDoDiaAsync(
        int profissionalId, int procedimentoId, DateTime data, CancellationToken ct = default)
    {
        var dias = await ObterDeVariosDiasAsync(profissionalId, procedimentoId, data.Date, 1, ct);
        return dias[0];
    }

    public async Task<IReadOnlyList<DisponibilidadeDoDiaResposta>> ObterDeVariosDiasAsync(
        int profissionalId,
        int procedimentoId,
        DateTime dataInicial,
        int quantidadeDeDias,
        CancellationToken ct = default)
    {
        var procedimento = await _banco.Procedimentos
            .FirstOrDefaultAsync(p => p.Id == procedimentoId, ct)
            ?? throw new RegraDeNegocioException("Procedimento não encontrado.");

        var profissional = await _banco.Profissionais
            .Include(p => p.HorariosTrabalho)
            .Include(p => p.ProcedimentosHabilitados)
            .FirstOrDefaultAsync(p => p.Id == profissionalId, ct)
            ?? throw new RegraDeNegocioException("Profissional não encontrada.");

        if (!profissional.Ativa)
            throw new RegraDeNegocioException("Esta profissional não está disponível para agendamentos.");

        if (!profissional.ProcedimentosHabilitados.Any(pp => pp.ProcedimentoId == procedimentoId))
            throw new RegraDeNegocioException($"{profissional.Nome} não realiza este procedimento.");

        var inicioDoPeriodo = dataInicial.Date;
        var fimDoPeriodo = inicioDoPeriodo.AddDays(quantidadeDeDias);

        var ocupados = await _banco.Agendamentos
            .Where(a => a.ProfissionalId == profissionalId &&
                        a.Inicio < fimDoPeriodo && a.Fim >= inicioDoPeriodo &&
                        (a.Status == StatusAgendamento.Agendado ||
                         a.Status == StatusAgendamento.Confirmado ||
                         a.Status == StatusAgendamento.Concluido))
            .Select(a => new { a.Inicio, a.Fim })
            .ToListAsync(ct);

        var bloqueios = await _banco.Bloqueios
            .Where(b => (b.ProfissionalId == null || b.ProfissionalId == profissionalId) &&
                        b.Inicio < fimDoPeriodo && b.Fim >= inicioDoPeriodo)
            .Select(b => new { b.Inicio, b.Fim })
            .ToListAsync(ct);

        var resultado = new List<DisponibilidadeDoDiaResposta>();

        for (var i = 0; i < quantidadeDeDias; i++)
        {
            var dia = inicioDoPeriodo.AddDays(i);
            var jornada = profissional.HorariosTrabalho.FirstOrDefault(h => h.DiaSemana == dia.DayOfWeek);

            if (jornada is null)
            {
                resultado.Add(new DisponibilidadeDoDiaResposta(
                    dia, Textos.DataCurta(dia), false, Array.Empty<HorarioDisponivelResposta>()));
                continue;
            }

            var horarios = new List<HorarioDisponivelResposta>();
            var momento = dia.Add(jornada.HoraInicio);
            var fimDoExpediente = dia.Add(jornada.HoraFim);

            while (momento < fimDoExpediente)
            {
                var terminoDoAtendimento = momento.AddMinutes(procedimento.DuracaoMinutos);

                if (terminoDoAtendimento > fimDoExpediente)
                    break;

                var jaPassou = momento <= _relogio.Agora;

                var conflitaComAtendimento = ocupados.Any(o =>
                    RegrasDeAgenda.HaConflito(momento, terminoDoAtendimento, o.Inicio, o.Fim));

                var dentroDeBloqueio = bloqueios.Any(b =>
                    momento < b.Fim && b.Inicio < terminoDoAtendimento);

                horarios.Add(new HorarioDisponivelResposta(
                    Textos.Hora(momento),
                    momento,
                    !jaPassou && !conflitaComAtendimento && !dentroDeBloqueio));

                momento = momento.AddMinutes(RegrasDeAgenda.MinutosEntreHorariosOferecidos);
            }

            resultado.Add(new DisponibilidadeDoDiaResposta(
                dia, Textos.DataCurta(dia), true, horarios));
        }

        return resultado;
    }

    public async Task GarantirQueEstaLivreAsync(
        int profissionalId,
        DateTime inicio,
        DateTime fim,
        int? ignorarAgendamentoId = null,
        CancellationToken ct = default)
    {
        var profissional = await _banco.Profissionais
            .Include(p => p.HorariosTrabalho)
            .FirstOrDefaultAsync(p => p.Id == profissionalId, ct)
            ?? throw new RegraDeNegocioException("Profissional não encontrada.");

        var jornada = profissional.HorariosTrabalho.FirstOrDefault(h => h.DiaSemana == inicio.DayOfWeek)
            ?? throw new RegraDeNegocioException($"{profissional.Nome} não atende neste dia da semana.");

        if (inicio.TimeOfDay < jornada.HoraInicio || fim.TimeOfDay > jornada.HoraFim)
            throw new RegraDeNegocioException($"O horário escolhido está fora do expediente de {profissional.Nome}.");

        var dentroDeBloqueio = await _banco.Bloqueios.AnyAsync(b =>
            (b.ProfissionalId == null || b.ProfissionalId == profissionalId) &&
            b.Inicio < fim && inicio < b.Fim, ct);

        if (dentroDeBloqueio)
            throw new RegraDeNegocioException("Este horário está bloqueado na agenda.");

        var margem = RegrasDeAgenda.MinutosDeIntervaloEntreAtendimentos;

        var vizinhos = await _banco.Agendamentos
            .Where(a => a.ProfissionalId == profissionalId &&
                        a.Id != (ignorarAgendamentoId ?? 0) &&
                        a.Inicio < fim.AddMinutes(margem) &&
                        inicio.AddMinutes(-margem) < a.Fim &&
                        (a.Status == StatusAgendamento.Agendado ||
                         a.Status == StatusAgendamento.Confirmado ||
                         a.Status == StatusAgendamento.Concluido))
            .Select(a => new { a.Inicio, a.Fim })
            .ToListAsync(ct);

        var haConflito = vizinhos.Any(v => RegrasDeAgenda.HaConflito(inicio, fim, v.Inicio, v.Fim));

        if (haConflito)
        {
            throw new RegraDeNegocioException(
                "Este horário acabou de ser reservado por outra cliente. Por favor, escolha outro.");
        }
    }
}
