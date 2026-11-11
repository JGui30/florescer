namespace Florescer.Domain.Regras;

public static class RegrasDeAgenda
{
    public const int MinutosDeIntervaloEntreAtendimentos = 10;

    public const int MinutosEntreHorariosOferecidos = 30;

    public const int HorasMinimasParaCancelarOuReagendar = 2;

    public const int DiasDeAgendaAberta = 60;

    public static bool HaConflito(DateTime inicioA, DateTime fimA, DateTime inicioB, DateTime fimB)
    {
        var fimAComIntervalo = fimA.AddMinutes(MinutosDeIntervaloEntreAtendimentos);
        var fimBComIntervalo = fimB.AddMinutes(MinutosDeIntervaloEntreAtendimentos);

        return inicioA < fimBComIntervalo && inicioB < fimAComIntervalo;
    }

    public static bool ClientePodeAlterar(DateTime agora, DateTime inicioAtendimento)
    {
        return agora < inicioAtendimento.AddHours(-HorasMinimasParaCancelarOuReagendar);
    }
}
