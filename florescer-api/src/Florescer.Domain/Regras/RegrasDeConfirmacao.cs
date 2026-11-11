namespace Florescer.Domain.Regras;

public static class RegrasDeConfirmacao
{
    public const int HorasAntesDoAtendimento = 24;

    public const int HorasAntesQuandoEmCimaDaHora = 2;

    public const int HorasDeAntecedenciaDoLembrete = 6;

    public static DateTime CalcularPrazo(DateTime agora, DateTime inicioAtendimento)
    {
        var prazoPadrao = inicioAtendimento.AddHours(-HorasAntesDoAtendimento);

        if (agora < prazoPadrao)
            return prazoPadrao;

        return inicioAtendimento.AddHours(-HorasAntesQuandoEmCimaDaHora);
    }

    public static bool NasceConfirmado(DateTime agora, DateTime inicioAtendimento)
    {
        return agora >= inicioAtendimento.AddHours(-HorasAntesQuandoEmCimaDaHora);
    }

    public static bool PrazoVencido(DateTime agora, DateTime prazoConfirmacao)
    {
        return agora > prazoConfirmacao;
    }

    public static bool DeveLembrarDaConfirmacao(DateTime agora, DateTime prazoConfirmacao)
    {
        var momentoDoLembrete = prazoConfirmacao.AddHours(-HorasDeAntecedenciaDoLembrete);
        return agora >= momentoDoLembrete && agora <= prazoConfirmacao;
    }

    public static string MensagemDoPrazo(DateTime prazoConfirmacao)
    {
        return $"Confirme este agendamento até {prazoConfirmacao:dd/MM/yyyy 'às' HH:mm}. " +
               "Sem a confirmação, ele será cancelado automaticamente e o horário " +
               "ficará disponível para outro cliente.";
    }
}
