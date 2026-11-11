using Florescer.Domain.Regras;

namespace Florescer.Tests;

public class RegrasDeConfirmacaoTestes
{
    [Fact]
    public void Agendamento_com_muita_antecedencia_deve_ser_confirmado_ate_24h_antes()
    {
        var agora = new DateTime(2026, 5, 10, 9, 0, 0);
        var atendimento = new DateTime(2026, 5, 20, 14, 0, 0);

        var prazo = RegrasDeConfirmacao.CalcularPrazo(agora, atendimento);

        Assert.Equal(new DateTime(2026, 5, 19, 14, 0, 0), prazo);
    }

    [Fact]
    public void Agendamento_feito_dentro_das_24h_deve_ser_confirmado_ate_2h_antes()
    {
        var agora = new DateTime(2026, 5, 20, 6, 0, 0);
        var atendimento = new DateTime(2026, 5, 20, 14, 0, 0);

        var prazo = RegrasDeConfirmacao.CalcularPrazo(agora, atendimento);

        Assert.Equal(new DateTime(2026, 5, 20, 12, 0, 0), prazo);
    }

    [Fact]
    public void Agendamento_exatamente_no_limite_de_24h_usa_o_prazo_normal()
    {
        var atendimento = new DateTime(2026, 5, 20, 14, 0, 0);
        var agora = atendimento.AddHours(-24).AddMinutes(-1);

        var prazo = RegrasDeConfirmacao.CalcularPrazo(agora, atendimento);

        Assert.Equal(atendimento.AddHours(-24), prazo);
    }

    [Fact]
    public void Agendamento_com_menos_de_2h_nasce_confirmado()
    {
        var agora = new DateTime(2026, 5, 20, 13, 0, 0);
        var atendimento = new DateTime(2026, 5, 20, 14, 0, 0);

        Assert.True(RegrasDeConfirmacao.NasceConfirmado(agora, atendimento));
    }

    [Fact]
    public void Agendamento_com_mais_de_2h_nao_nasce_confirmado()
    {
        var agora = new DateTime(2026, 5, 20, 10, 0, 0);
        var atendimento = new DateTime(2026, 5, 20, 14, 0, 0);

        Assert.False(RegrasDeConfirmacao.NasceConfirmado(agora, atendimento));
    }

    [Fact]
    public void Prazo_vencido_e_detectado_corretamente()
    {
        var prazo = new DateTime(2026, 5, 19, 14, 0, 0);

        Assert.True(RegrasDeConfirmacao.PrazoVencido(prazo.AddMinutes(1), prazo));
        Assert.False(RegrasDeConfirmacao.PrazoVencido(prazo.AddMinutes(-1), prazo));
    }

    [Fact]
    public void Lembrete_e_enviado_na_janela_de_6h_antes_do_prazo()
    {
        var prazo = new DateTime(2026, 5, 19, 14, 0, 0);

        Assert.False(RegrasDeConfirmacao.DeveLembrarDaConfirmacao(prazo.AddHours(-7), prazo));

        Assert.True(RegrasDeConfirmacao.DeveLembrarDaConfirmacao(prazo.AddHours(-5), prazo));

        Assert.False(RegrasDeConfirmacao.DeveLembrarDaConfirmacao(prazo.AddMinutes(1), prazo));
    }

    [Fact]
    public void Mensagem_do_prazo_informa_a_data_limite_ao_cliente()
    {
        var prazo = new DateTime(2026, 5, 19, 14, 30, 0);

        var mensagem = RegrasDeConfirmacao.MensagemDoPrazo(prazo);

        Assert.Contains("19/05/2026", mensagem);
        Assert.Contains("14:30", mensagem);
        Assert.Contains("cancelado automaticamente", mensagem);
    }
}
