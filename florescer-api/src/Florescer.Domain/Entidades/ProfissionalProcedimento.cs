namespace Florescer.Domain.Entidades;

public class ProfissionalProcedimento
{
    public int ProfissionalId { get; set; }
    public Profissional Profissional { get; set; } = null!;

    public int ProcedimentoId { get; set; }
    public Procedimento Procedimento { get; set; } = null!;
}
