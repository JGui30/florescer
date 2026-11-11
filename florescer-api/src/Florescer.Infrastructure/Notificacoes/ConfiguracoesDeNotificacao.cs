namespace Florescer.Infrastructure.Notificacoes;

public class ConfiguracoesDeNotificacao
{
    public const string Secao = "Notificacoes";

    public string? ServidorSmtp { get; set; }

    public int PortaSmtp { get; set; } = 587;

    public string? UsuarioSmtp { get; set; }

    public string? SenhaSmtp { get; set; }

    public string RemetenteNome { get; set; } = "Florescer";

    public string RemetenteEmail { get; set; } = "nao-responda@florescer.com.br";

    public string? WhatsAppDoEstabelecimento { get; set; }
}
