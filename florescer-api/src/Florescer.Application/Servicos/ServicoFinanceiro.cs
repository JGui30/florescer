using Florescer.Application.Contratos;
using Florescer.Application.Dtos;
using Florescer.Application.Persistencia;
using Florescer.Domain.Enums;
using Florescer.Domain.Regras;
using Microsoft.EntityFrameworkCore;

namespace Florescer.Application.Servicos;

public class ServicoFinanceiro
{
    private readonly IBancoDeDados _banco;
    private readonly IRelogio _relogio;

    public ServicoFinanceiro(IBancoDeDados banco, IRelogio relogio)
    {
        _banco = banco;
        _relogio = relogio;
    }

    public async Task<ResumoFinanceiroResposta> ObterResumoAsync(
        string periodo = "mes",
        DateTime? referencia = null,
        DateTime? de = null,
        DateTime? ate = null,
        CancellationToken ct = default)
    {
        var data = (referencia ?? _relogio.Hoje).Date;

        periodo = string.IsNullOrWhiteSpace(periodo) ? "mes" : periodo.Trim().ToLowerInvariant();

        DateTime inicio, fim;

        if (periodo == "personalizado")
        {
            inicio = (de ?? _relogio.Hoje).Date;
            fim = (ate ?? inicio).Date.AddDays(1);

            if (fim <= inicio)
                throw new RegraDeNegocioException("A data final deve ser igual ou posterior à inicial.");
        }
        else
        {
            (inicio, fim) = CalcularIntervalo(periodo, data);
        }

        var duracao = fim - inicio;
        var (inicioAnterior, fimAnterior) = periodo == "personalizado"
            ? (inicio - duracao, inicio)
            : CalcularIntervaloAnterior(periodo, inicio);

        var receita = await SomarReceitaAsync(inicio, fim, ct);
        var receitaAnterior = await SomarReceitaAsync(inicioAnterior, fimAnterior, ct);

        var concluidos = await _banco.Agendamentos.CountAsync(a =>
            a.Status == StatusAgendamento.Concluido &&
            a.Inicio >= inicio && a.Inicio < fim, ct);

        var ticketMedio = concluidos == 0 ? 0 : Math.Round(receita / concluidos, 2);

        var agora = _relogio.Agora;
        var receitaPrevista = await _banco.Agendamentos
            .Where(a => a.Status == StatusAgendamento.Confirmado &&
                        a.Inicio >= agora && a.Inicio < fim)
            .SumAsync(a => (decimal?)a.ValorPrevisto, ct) ?? 0;

        var aFechar = await _banco.Agendamentos.CountAsync(a =>
            a.Fim < agora &&
            (a.Status == StatusAgendamento.Confirmado || a.Status == StatusAgendamento.Agendado), ct);

        var totalNoPeriodo = await _banco.Agendamentos.CountAsync(a =>
            a.Inicio >= inicio && a.Inicio < fim, ct);

        var canceladosNoPeriodo = await _banco.Agendamentos.CountAsync(a =>
            a.Inicio >= inicio && a.Inicio < fim && a.Status == StatusAgendamento.Cancelado, ct);

        var taxaDeCancelamento = totalNoPeriodo == 0
            ? 0
            : Math.Round(canceladosNoPeriodo * 100m / totalNoPeriodo, 1);

        var perdaPorFaltaDeConfirmacao = await _banco.Agendamentos
            .Where(a => a.Inicio >= inicio && a.Inicio < fim &&
                        a.Status == StatusAgendamento.Cancelado &&
                        a.MotivoCancelamento == MotivoCancelamento.FaltaDeConfirmacao)
            .SumAsync(a => (decimal?)a.ValorPrevisto, ct) ?? 0;

        return new ResumoFinanceiroResposta(
            periodo,
            inicio,
            fim.AddDays(-1),
            receita,
            receitaAnterior,
            CalcularVariacao(receita, receitaAnterior),
            concluidos,
            ticketMedio,
            receitaPrevista,
            aFechar,
            taxaDeCancelamento,
            perdaPorFaltaDeConfirmacao,
            await MontarSerieAsync(periodo, inicio, fim, ct),
            await ReceitaPorCategoriaAsync(inicio, fim, ct),
            await ReceitaPorProfissionalAsync(inicio, fim, ct),
            await TopProcedimentosAsync(inicio, fim, ct));
    }

    private async Task<IReadOnlyList<PontoDaSerieResposta>> MontarSerieAsync(
        string periodo, DateTime inicio, DateTime fim, CancellationToken ct)
    {
        var lancamentos = await _banco.Lancamentos
            .Where(l => l.Tipo == TipoLancamento.Receita && l.Data >= inicio && l.Data < fim)
            .Select(l => new { l.Data, l.Valor })
            .ToListAsync(ct);

        var pontos = new List<PontoDaSerieResposta>();

        if (periodo == "personalizado")
        {
            var dias = (fim - inicio).TotalDays;
            periodo = dias > 62 ? "ano-livre" : "mes";
        }

        if (periodo == "ano-livre")
        {
            var mes = new DateTime(inicio.Year, inicio.Month, 1);
            while (mes < fim)
            {
                var doMes = lancamentos.Where(l => l.Data.Year == mes.Year && l.Data.Month == mes.Month).ToList();
                pontos.Add(new PontoDaSerieResposta(
                    $"{Textos.MesAbreviado(mes)}/{mes:yy}", mes, doMes.Sum(l => l.Valor), doMes.Count));
                mes = mes.AddMonths(1);
            }
            return pontos;
        }

        if (periodo == "ano")
        {
            for (var mes = 1; mes <= 12; mes++)
            {
                var primeiroDia = new DateTime(inicio.Year, mes, 1);
                var doMes = lancamentos.Where(l => l.Data.Month == mes).ToList();

                pontos.Add(new PontoDaSerieResposta(
                    Textos.MesAbreviado(primeiroDia),
                    primeiroDia,
                    doMes.Sum(l => l.Valor),
                    doMes.Count));
            }

            return pontos;
        }

        for (var dia = inicio; dia < fim; dia = dia.AddDays(1))
        {
            var doDia = lancamentos.Where(l => l.Data.Date == dia.Date).ToList();

            var rotulo = periodo switch
            {
                "dia" => Textos.Hora(dia),
                "semana" => Textos.DataCurta(dia),
                _ => dia.Day.ToString("D2")
            };

            pontos.Add(new PontoDaSerieResposta(
                rotulo, dia, doDia.Sum(l => l.Valor), doDia.Count));
        }

        return pontos;
    }

    private async Task<IReadOnlyList<EstatisticaCategoriaResposta>> ReceitaPorCategoriaAsync(
        DateTime inicio, DateTime fim, CancellationToken ct)
    {
        var dados = await _banco.Lancamentos
            .Where(l => l.Tipo == TipoLancamento.Receita &&
                        l.Data >= inicio && l.Data < fim &&
                        l.Agendamento != null)
            .Select(l => new
            {
                l.Valor,
                Categoria = l.Agendamento!.Procedimento.Categoria
            })
            .ToListAsync(ct);

        var agrupado = dados
            .GroupBy(d => new { d.Categoria.Slug, d.Categoria.Nome, d.Categoria.Cor })
            .Select(g => new
            {
                g.Key.Slug,
                g.Key.Nome,
                g.Key.Cor,
                Total = g.Sum(x => x.Valor),
                Quantidade = g.Count()
            })
            .OrderByDescending(x => x.Total)
            .ToList();

        var maior = agrupado.Select(x => x.Total).DefaultIfEmpty(0).Max();

        return agrupado.Select(x => new EstatisticaCategoriaResposta(
            x.Slug, x.Nome, x.Cor, x.Quantidade,
            maior == 0 ? 0 : Math.Round(x.Total * 100m / maior, 1),
            x.Total)).ToList();
    }

    private async Task<IReadOnlyList<ReceitaPorProfissionalResposta>> ReceitaPorProfissionalAsync(
        DateTime inicio, DateTime fim, CancellationToken ct)
    {
        var dados = await _banco.Lancamentos
            .Where(l => l.Tipo == TipoLancamento.Receita &&
                        l.Data >= inicio && l.Data < fim &&
                        l.Agendamento != null)
            .Select(l => new
            {
                l.Valor,
                Profissional = l.Agendamento!.Profissional
            })
            .ToListAsync(ct);

        return dados
            .GroupBy(d => new { d.Profissional.Id, d.Profissional.Nome, d.Profissional.Especialidade })
            .Select(g =>
            {
                var total = g.Sum(x => x.Valor);
                var quantidade = g.Count();

                return new ReceitaPorProfissionalResposta(
                    g.Key.Id, g.Key.Nome, g.Key.Especialidade,
                    total, quantidade,
                    quantidade == 0 ? 0 : Math.Round(total / quantidade, 2));
            })
            .OrderByDescending(x => x.Total)
            .ToList();
    }

    private async Task<IReadOnlyList<ReceitaPorProcedimentoResposta>> TopProcedimentosAsync(
        DateTime inicio, DateTime fim, CancellationToken ct)
    {
        var dados = await _banco.Lancamentos
            .Where(l => l.Tipo == TipoLancamento.Receita &&
                        l.Data >= inicio && l.Data < fim &&
                        l.Agendamento != null)
            .Select(l => new
            {
                l.Valor,
                Procedimento = l.Agendamento!.Procedimento
            })
            .ToListAsync(ct);

        return dados
            .GroupBy(d => new { d.Procedimento.Id, d.Procedimento.Nome, Slug = d.Procedimento.Categoria.Slug })
            .Select(g => new ReceitaPorProcedimentoResposta(
                g.Key.Id, g.Key.Nome, g.Key.Slug, g.Sum(x => x.Valor), g.Count()))
            .OrderByDescending(x => x.Total)
            .Take(8)
            .ToList();
    }

    public async Task<IReadOnlyList<AtendimentoAFecharResposta>> ListarAtendimentosAFecharAsync(
        CancellationToken ct = default)
    {
        var agora = _relogio.Agora;

        var pendentes = await _banco.Agendamentos
            .Include(a => a.Cliente)
            .Include(a => a.Profissional)
            .Include(a => a.Procedimento)
            .Where(a => a.Fim < agora &&
                        (a.Status == StatusAgendamento.Confirmado || a.Status == StatusAgendamento.Agendado))
            .OrderBy(a => a.Inicio)
            .Take(50)
            .ToListAsync(ct);

        return pendentes.Select(a => new AtendimentoAFecharResposta(
            a.Id, a.Inicio, a.Cliente.Nome, a.Profissional.Nome,
            a.Procedimento.Nome, a.ValorPrevisto)).ToList();
    }

    public async Task<IReadOnlyList<LancamentoResposta>> ListarLancamentosAsync(
        DateTime? de = null, DateTime? ate = null, CancellationToken ct = default)
    {
        var inicio = (de ?? _relogio.Hoje.AddDays(-30)).Date;
        var fim = (ate ?? _relogio.Hoje).Date.AddDays(1);

        var lancamentos = await _banco.Lancamentos
            .Include(l => l.RegistradoPor)
            .Where(l => l.Data >= inicio && l.Data < fim)
            .OrderByDescending(l => l.Data).ThenByDescending(l => l.Id)
            .Take(200)
            .ToListAsync(ct);

        return lancamentos.Select(l => new LancamentoResposta(
            l.Id, l.Tipo.ToString(), l.Valor, l.Data, l.Descricao,
            l.AgendamentoId, l.RegistradoPor?.Nome)).ToList();
    }

    public static (DateTime Inicio, DateTime Fim) CalcularIntervalo(string periodo, DateTime referencia)
    {
        switch (periodo?.ToLowerInvariant())
        {
            case "personalizado":
                return (referencia.Date, referencia.Date.AddDays(1));

            case "dia":
                return (referencia.Date, referencia.Date.AddDays(1));

            case "semana":
                var diasDesdeSegunda = ((int)referencia.DayOfWeek + 6) % 7;
                var segunda = referencia.Date.AddDays(-diasDesdeSegunda);
                return (segunda, segunda.AddDays(7));

            case "ano":
                var primeiroDiaDoAno = new DateTime(referencia.Year, 1, 1);
                return (primeiroDiaDoAno, primeiroDiaDoAno.AddYears(1));

            case "mes":
            default:
                var primeiroDiaDoMes = new DateTime(referencia.Year, referencia.Month, 1);
                return (primeiroDiaDoMes, primeiroDiaDoMes.AddMonths(1));
        }
    }

    private static (DateTime Inicio, DateTime Fim) CalcularIntervaloAnterior(string periodo, DateTime inicio)
    {
        return periodo?.ToLowerInvariant() switch
        {
            "dia" => (inicio.AddDays(-1), inicio),
            "semana" => (inicio.AddDays(-7), inicio),
            "ano" => (inicio.AddYears(-1), inicio),
            _ => (inicio.AddMonths(-1), inicio)
        };
    }

    private static decimal CalcularVariacao(decimal atual, decimal anterior)
    {
        if (anterior == 0)
            return atual > 0 ? 100 : 0;

        return Math.Round((atual - anterior) / anterior * 100m, 1);
    }

    private async Task<decimal> SomarReceitaAsync(DateTime inicio, DateTime fim, CancellationToken ct)
    {
        return await _banco.Lancamentos
            .Where(l => l.Tipo == TipoLancamento.Receita && l.Data >= inicio && l.Data < fim)
            .SumAsync(l => (decimal?)l.Valor, ct) ?? 0;
    }
}
