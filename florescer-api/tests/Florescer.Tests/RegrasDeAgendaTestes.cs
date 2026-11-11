using Florescer.Domain.Regras;

namespace Florescer.Tests;

public class RegrasDeAgendaTestes
{
    private static readonly DateTime Inicio = new(2026, 5, 20, 10, 0, 0);
    private static readonly DateTime Fim = new(2026, 5, 20, 11, 0, 0);

    [Fact]
    public void Horarios_identicos_conflitam()
    {
        Assert.True(RegrasDeAgenda.HaConflito(Inicio, Fim, Inicio, Fim));
    }

    [Fact]
    public void Horario_que_comeca_no_meio_do_outro_conflita()
    {
        var outroInicio = new DateTime(2026, 5, 20, 10, 30, 0);
        var outroFim = new DateTime(2026, 5, 20, 11, 30, 0);

        Assert.True(RegrasDeAgenda.HaConflito(Inicio, Fim, outroInicio, outroFim));
    }

    [Fact]
    public void Horario_que_engloba_o_outro_conflita()
    {
        var outroInicio = new DateTime(2026, 5, 20, 9, 0, 0);
        var outroFim = new DateTime(2026, 5, 20, 12, 0, 0);

        Assert.True(RegrasDeAgenda.HaConflito(Inicio, Fim, outroInicio, outroFim));
    }

    [Fact]
    public void Horario_colado_no_anterior_conflita_por_causa_do_intervalo()
    {
        var outroInicio = new DateTime(2026, 5, 20, 11, 0, 0);
        var outroFim = new DateTime(2026, 5, 20, 12, 0, 0);

        Assert.True(RegrasDeAgenda.HaConflito(Inicio, Fim, outroInicio, outroFim));
    }

    [Fact]
    public void Horario_com_intervalo_suficiente_nao_conflita()
    {
        var outroInicio = new DateTime(2026, 5, 20, 11, 15, 0);
        var outroFim = new DateTime(2026, 5, 20, 12, 15, 0);

        Assert.False(RegrasDeAgenda.HaConflito(Inicio, Fim, outroInicio, outroFim));
    }

    [Fact]
    public void Horarios_em_dias_diferentes_nao_conflitam()
    {
        var outroInicio = new DateTime(2026, 5, 21, 10, 0, 0);
        var outroFim = new DateTime(2026, 5, 21, 11, 0, 0);

        Assert.False(RegrasDeAgenda.HaConflito(Inicio, Fim, outroInicio, outroFim));
    }

    [Fact]
    public void Cliente_pode_alterar_com_mais_de_2h_de_antecedencia()
    {
        var agora = new DateTime(2026, 5, 20, 7, 0, 0);

        Assert.True(RegrasDeAgenda.ClientePodeAlterar(agora, Inicio));
    }

    [Fact]
    public void Cliente_nao_pode_alterar_perto_do_atendimento()
    {
        var agora = new DateTime(2026, 5, 20, 9, 0, 0);

        Assert.False(RegrasDeAgenda.ClientePodeAlterar(agora, Inicio));
    }
}
