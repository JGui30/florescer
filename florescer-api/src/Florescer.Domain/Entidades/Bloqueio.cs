using Florescer.Domain.Enums;

namespace Florescer.Domain.Entidades;

public class Bloqueio
{
    public int Id { get; set; }

    public int? ProfissionalId { get; set; }
    public Profissional? Profissional { get; set; }

    public DateTime Inicio { get; set; }

    public DateTime Fim { get; set; }

    public TipoBloqueio Tipo { get; set; }

    public string? Motivo { get; set; }

    public DateTime CriadoEm { get; set; }
}
