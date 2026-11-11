namespace Florescer.Infrastructure.Seguranca;

public class ConfiguracoesDeToken
{
    public const string Secao = "Jwt";

    public string Chave { get; set; } = string.Empty;

    public string Emissor { get; set; } = "Florescer";

    public string Audiencia { get; set; } = "FlorescerApp";

    public int HorasDeValidade { get; set; } = 8;
}
