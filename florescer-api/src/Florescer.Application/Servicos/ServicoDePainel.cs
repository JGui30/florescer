using Florescer.Application.Contratos;
using Florescer.Application.Dtos;
using Florescer.Application.Persistencia;
using Florescer.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Florescer.Application.Servicos;

public class ServicoDePainel
{
    private readonly IBancoDeDados _banco;
    private readonly IRelogio _relogio;
    private readonly ServicoDeAgendamento _agendamentos;

    public ServicoDePainel(
        IBancoDeDados banco, IRelogio relogio, ServicoDeAgendamento agendamentos)
    {
        _banco = banco;
        _relogio = relogio;
        _agendamentos = agendamentos;
    }

    public async Task<VisaoGeralResposta> ObterVisaoGeralAsync(
        DateTime? dataConsultada = null, CancellationToken ct = default)
    {
        var hoje = (dataConsultada ?? _relogio.Hoje).Date;
        var ontem = hoje.AddDays(-1);

        var agendamentosDeHoje = await BuscarDoDiaAsync(hoje, ct);
        var agendamentosDeOntem = await BuscarDoDiaAsync(ontem, ct);

        var faturamentoHoje = await SomarReceitaAsync(hoje, hoje, ct);
        var faturamentoOntem = await SomarReceitaAsync(ontem, ontem, ct);

        var ocupacaoHoje = await CalcularOcupacaoAsync(hoje, ct);
        var ocupacaoOntem = await CalcularOcupacaoAsync(ontem, ct);

        var validosDeHoje = agendamentosDeHoje
            .Where(a => a.Status != StatusAgendamento.Cancelado)
            .ToList();

        var categorias = await _banco.Categorias.OrderBy(c => c.Ordem).ToListAsync(ct);
        var maiorQuantidade = categorias
            .Select(c => validosDeHoje.Count(a => a.Procedimento.CategoriaId == c.Id))
            .DefaultIfEmpty(0)
            .Max();

        var porCategoria = categorias.Select(c =>
        {
            var quantidade = validosDeHoje.Count(a => a.Procedimento.CategoriaId == c.Id);
            var percentual = maiorQuantidade == 0
                ? 0
                : Math.Round(quantidade * 100m / maiorQuantidade, 1);

            return new EstatisticaCategoriaResposta(c.Slug, c.Nome, c.Cor, quantidade, percentual, 0);
        }).ToList();

        var agora = _relogio.Agora;
        var proximos = await _banco.Agendamentos
            .Include(a => a.Cliente)
            .Include(a => a.Profissional)
            .Include(a => a.Procedimento).ThenInclude(p => p.Categoria)
            .Include(a => a.Lancamento)
            .Where(a => a.Inicio >= agora &&
                        (a.Status == StatusAgendamento.Agendado || a.Status == StatusAgendamento.Confirmado))
            .OrderBy(a => a.Inicio)
            .Take(6)
            .ToListAsync(ct);

        var aguardandoConfirmacao = await _banco.Agendamentos.CountAsync(a =>
            a.Status == StatusAgendamento.Agendado && a.Inicio >= agora, ct);

        var aFechar = await ContarAtendimentosAFecharAsync(ct);

        return new VisaoGeralResposta(
            hoje,
            validosDeHoje.Count,
            agendamentosDeOntem.Count(a => a.Status != StatusAgendamento.Cancelado),
            ocupacaoHoje,
            ocupacaoOntem,
            faturamentoHoje,
            faturamentoOntem,
            agendamentosDeHoje.Count(a => a.Status == StatusAgendamento.Cancelado),
            agendamentosDeOntem.Count(a => a.Status == StatusAgendamento.Cancelado),
            aguardandoConfirmacao,
            aFechar,
            porCategoria,
            proximos.Select(_agendamentos.Converter).ToList());
    }

    private async Task<decimal> CalcularOcupacaoAsync(DateTime dia, CancellationToken ct)
    {
        var jornadas = await _banco.HorariosTrabalho
            .Where(h => h.DiaSemana == dia.DayOfWeek && h.Profissional.Ativa)
            .Select(h => new { h.HoraInicio, h.HoraFim })
            .ToListAsync(ct);

        var minutosDisponiveis = jornadas.Sum(j => (j.HoraFim - j.HoraInicio).TotalMinutes);

        if (minutosDisponiveis <= 0) return 0;

        var inicioDoDia = dia.Date;
        var fimDoDia = inicioDoDia.AddDays(1);

        var atendimentos = await _banco.Agendamentos
            .Where(a => a.Inicio >= inicioDoDia && a.Inicio < fimDoDia &&
                        a.Status != StatusAgendamento.Cancelado)
            .Select(a => new { a.Inicio, a.Fim })
            .ToListAsync(ct);

        var minutosOcupados = atendimentos.Sum(a => (a.Fim - a.Inicio).TotalMinutes);

        return Math.Round((decimal)(minutosOcupados / minutosDisponiveis) * 100m, 1);
    }

    private async Task<List<Domain.Entidades.Agendamento>> BuscarDoDiaAsync(
        DateTime dia, CancellationToken ct)
    {
        var inicio = dia.Date;
        var fim = inicio.AddDays(1);

        return await _banco.Agendamentos
            .Include(a => a.Procedimento)
            .Where(a => a.Inicio >= inicio && a.Inicio < fim)
            .ToListAsync(ct);
    }

    private async Task<decimal> SomarReceitaAsync(DateTime de, DateTime ate, CancellationToken ct)
    {
        var inicio = de.Date;
        var fim = ate.Date.AddDays(1);

        return await _banco.Lancamentos
            .Where(l => l.Tipo == TipoLancamento.Receita && l.Data >= inicio && l.Data < fim)
            .SumAsync(l => (decimal?)l.Valor, ct) ?? 0;
    }

    private async Task<int> ContarAtendimentosAFecharAsync(CancellationToken ct)
    {
        var agora = _relogio.Agora;

        return await _banco.Agendamentos.CountAsync(a =>
            a.Fim < agora &&
            (a.Status == StatusAgendamento.Confirmado || a.Status == StatusAgendamento.Agendado), ct);
    }
}
