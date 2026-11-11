namespace Florescer.Domain.Entidades;

public class Categoria
{
    public int Id { get; set; }

    public string Nome { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    public string Cor { get; set; } = "#C22656";

    public string? ImagemUrl { get; set; }

    public int Ordem { get; set; }

    public bool Ativa { get; set; } = true;

    public ICollection<Procedimento> Procedimentos { get; set; } = new List<Procedimento>();
}
