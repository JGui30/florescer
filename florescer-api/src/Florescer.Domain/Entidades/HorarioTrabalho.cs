namespace Florescer.Domain.Entidades;

public class HorarioTrabalho
{
    public int Id { get; set; }

    public int ProfissionalId { get; set; }
    public Profissional Profissional { get; set; } = null!;

    public DayOfWeek DiaSemana { get; set; }

    public TimeSpan HoraInicio { get; set; }

    public TimeSpan HoraFim { get; set; }
}
