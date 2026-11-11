using System.Globalization;
using Florescer.Domain.Enums;

namespace Florescer.Application.Servicos;

public static class Textos
{
    private static readonly CultureInfo Brasil = new("pt-BR");

    private static readonly string[] DiasAbreviados =
        { "dom", "seg", "ter", "qua", "qui", "sex", "sáb" };

    public static string Status(StatusAgendamento status) => status switch
    {
        StatusAgendamento.Agendado => "Agendado",
        StatusAgendamento.Confirmado => "Confirmado",
        StatusAgendamento.Concluido => "Concluído",
        StatusAgendamento.Cancelado => "Cancelado",
        _ => status.ToString()
    };

    public static string Motivo(MotivoCancelamento motivo) => motivo switch
    {
        MotivoCancelamento.CanceladoPeloCliente => "Cancelado pela cliente",
        MotivoCancelamento.CanceladoPeloAdministrador => "Cancelado pela recepção",
        MotivoCancelamento.FaltaDeConfirmacao => "Cancelado por falta de confirmação",
        MotivoCancelamento.ClienteNaoCompareceu => "Cliente não compareceu",
        _ => motivo.ToString()
    };

    public static string TipoDeBloqueio(TipoBloqueio tipo) => tipo switch
    {
        TipoBloqueio.Folga => "Folga",
        TipoBloqueio.Ferias => "Férias",
        TipoBloqueio.Feriado => "Feriado",
        TipoBloqueio.BloqueioPontual => "Bloqueio",
        _ => tipo.ToString()
    };

    public static string DataCurta(DateTime data)
        => $"{DiasAbreviados[(int)data.DayOfWeek]}, {data:dd/MM}";

    public static string DataPorExtenso(DateTime data)
        => data.ToString("dddd, dd 'de' MMMM", Brasil);

    public static string Hora(DateTime data) => data.ToString("HH:mm");

    public static string MesAbreviado(DateTime data)
        => Brasil.DateTimeFormat.GetAbbreviatedMonthName(data.Month);

    public static string Mascarar(string destino)
    {
        if (string.IsNullOrWhiteSpace(destino))
            return string.Empty;

        if (destino.Contains('@'))
        {
            var partes = destino.Split('@');
            var inicio = partes[0];
            var visivel = inicio.Length <= 2 ? inicio[..1] : inicio[..2];
            return $"{visivel}{new string('*', Math.Max(3, inicio.Length - 2))}@{partes[1]}";
        }

        var digitos = new string(destino.Where(char.IsDigit).ToArray());
        if (digitos.Length < 4) return "****";

        return $"({digitos[..2]}) ****-{digitos[^4..]}";
    }
}
