using Florescer.Domain.Enums;

namespace Florescer.Application.Contratos;

public record MensagemParaEnvio(
    string Destino,
    string Assunto,
    string Texto);

public record ResultadoDoEnvio(bool Sucesso, StatusNotificacao Status, string? Erro = null);

public interface ICanalDeNotificacao
{
    CanalNotificacao Canal { get; }

    bool EstaConfigurado { get; }

    Task<ResultadoDoEnvio> EnviarAsync(MensagemParaEnvio mensagem, CancellationToken cancelamento = default);
}
