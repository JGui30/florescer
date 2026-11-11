namespace Florescer.Domain.Entidades;

public class Profissional
{
    public int Id { get; set; }

    public string Nome { get; set; } = string.Empty;

    public string Especialidade { get; set; } = string.Empty;

    public string? Email { get; set; }

    public string? Telefone { get; set; }

    public string? FotoUrl { get; set; }

    public bool Ativa { get; set; } = true;

    public DateTime CriadoEm { get; set; }

    public ICollection<HorarioTrabalho> HorariosTrabalho { get; set; } = new List<HorarioTrabalho>();
    public ICollection<Bloqueio> Bloqueios { get; set; } = new List<Bloqueio>();
    public ICollection<ProfissionalProcedimento> ProcedimentosHabilitados { get; set; }
        = new List<ProfissionalProcedimento>();
    public ICollection<Agendamento> Agendamentos { get; set; } = new List<Agendamento>();
}
