using Florescer.Application.Servicos;

namespace Florescer.Tests;

public class ServicoFinanceiroTestes
{
    private static readonly DateTime Referencia = new(2026, 5, 20);

    [Fact]
    public void Periodo_dia_cobre_apenas_a_data_informada()
    {
        var (inicio, fim) = ServicoFinanceiro.CalcularIntervalo("dia", Referencia);

        Assert.Equal(new DateTime(2026, 5, 20), inicio);
        Assert.Equal(new DateTime(2026, 5, 21), fim);
    }

    [Fact]
    public void Periodo_semana_comeca_na_segunda_feira()
    {
        var (inicio, fim) = ServicoFinanceiro.CalcularIntervalo("semana", Referencia);

        Assert.Equal(new DateTime(2026, 5, 18), inicio);
        Assert.Equal(DayOfWeek.Monday, inicio.DayOfWeek);
        Assert.Equal(new DateTime(2026, 5, 25), fim);
    }

    [Fact]
    public void Periodo_semana_funciona_quando_a_referencia_e_domingo()
    {
        var domingo = new DateTime(2026, 5, 24);

        var (inicio, _) = ServicoFinanceiro.CalcularIntervalo("semana", domingo);

        Assert.Equal(new DateTime(2026, 5, 18), inicio);
    }

    [Fact]
    public void Periodo_mes_cobre_do_primeiro_ao_ultimo_dia()
    {
        var (inicio, fim) = ServicoFinanceiro.CalcularIntervalo("mes", Referencia);

        Assert.Equal(new DateTime(2026, 5, 1), inicio);
        Assert.Equal(new DateTime(2026, 6, 1), fim);
    }

    [Fact]
    public void Periodo_ano_cobre_o_ano_inteiro()
    {
        var (inicio, fim) = ServicoFinanceiro.CalcularIntervalo("ano", Referencia);

        Assert.Equal(new DateTime(2026, 1, 1), inicio);
        Assert.Equal(new DateTime(2027, 1, 1), fim);
    }

    [Fact]
    public void Periodo_desconhecido_cai_no_mes_como_padrao()
    {
        var (inicio, fim) = ServicoFinanceiro.CalcularIntervalo("qualquer-coisa", Referencia);

        Assert.Equal(new DateTime(2026, 5, 1), inicio);
        Assert.Equal(new DateTime(2026, 6, 1), fim);
    }
}
