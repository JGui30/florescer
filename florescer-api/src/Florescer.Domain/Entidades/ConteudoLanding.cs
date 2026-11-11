namespace Florescer.Domain.Entidades;

public class SecaoLanding
{
    public int Id { get; set; }

    public string Chave { get; set; } = string.Empty;

    public string Titulo { get; set; } = string.Empty;

    public string? Subtitulo { get; set; }

    public string? Texto { get; set; }

    public string? TextoBotao { get; set; }

    public string? ImagemUrl { get; set; }

    public int Ordem { get; set; }

    public bool Ativa { get; set; } = true;

    public ICollection<MidiaLanding> Midias { get; set; } = new List<MidiaLanding>();
}

public class MidiaLanding
{
    public int Id { get; set; }

    public int? SecaoLandingId { get; set; }
    public SecaoLanding? SecaoLanding { get; set; }

    public string Url { get; set; } = string.Empty;

    public string? Legenda { get; set; }

    public int Ordem { get; set; }

    public bool Ativa { get; set; } = true;

    public DateTime CriadaEm { get; set; }
}

public class Faq
{
    public int Id { get; set; }

    public string Pergunta { get; set; } = string.Empty;

    public string Resposta { get; set; } = string.Empty;

    public int Ordem { get; set; }

    public bool Ativa { get; set; } = true;
}

public class ConfiguracaoEstabelecimento
{
    public int Id { get; set; }

    public string Nome { get; set; } = "Florescer";

    public string? Slogan { get; set; }

    public string? Telefone { get; set; }

    public string? WhatsApp { get; set; }

    public string? Email { get; set; }

    public string? Endereco { get; set; }

    public string? HorarioFuncionamento { get; set; }

    public string? InstagramUrl { get; set; }

    public string? FacebookUrl { get; set; }

    public string? MapaUrl { get; set; }
}
