namespace Florescer.Domain.Entidades;

public class Procedimento
{
    public int Id { get; set; }

    public int CategoriaId { get; set; }
    public Categoria Categoria { get; set; } = null!;

    public string Nome { get; set; } = string.Empty;

    public string? Descricao { get; set; }

    public int DuracaoMinutos { get; set; }

    public decimal Preco { get; set; }

    public bool Ativo { get; set; } = true;

    public ICollection<ProfissionalProcedimento> ProfissionaisHabilitadas { get; set; }
        = new List<ProfissionalProcedimento>();

    public ICollection<Agendamento> Agendamentos { get; set; } = new List<Agendamento>();
}
