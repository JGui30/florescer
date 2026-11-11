using Florescer.Application.Contratos;
using Florescer.Domain.Enums;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Florescer.Infrastructure.Notificacoes;

public class CanalDeEmail : ICanalDeNotificacao
{
    private readonly ConfiguracoesDeNotificacao _config;
    private readonly ILogger<CanalDeEmail> _log;

    public CanalDeEmail(IOptions<ConfiguracoesDeNotificacao> config, ILogger<CanalDeEmail> log)
    {
        _config = config.Value;
        _log = log;
    }

    public CanalNotificacao Canal => CanalNotificacao.Email;

    public bool EstaConfigurado => !string.IsNullOrWhiteSpace(_config.ServidorSmtp);

    public async Task<ResultadoDoEnvio> EnviarAsync(
        MensagemParaEnvio mensagem, CancellationToken cancelamento = default)
    {
        if (!EstaConfigurado)
        {
            _log.LogInformation(
                "E-mail simulado para {Destino}: {Assunto}", mensagem.Destino, mensagem.Assunto);

            return new ResultadoDoEnvio(true, StatusNotificacao.Simulada);
        }

        try
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(_config.RemetenteNome, _config.RemetenteEmail));
            email.To.Add(MailboxAddress.Parse(mensagem.Destino));
            email.Subject = mensagem.Assunto;
            email.Body = new TextPart("plain") { Text = mensagem.Texto };

            using var cliente = new SmtpClient();
            await cliente.ConnectAsync(
                _config.ServidorSmtp, _config.PortaSmtp, SecureSocketOptions.StartTls, cancelamento);

            if (!string.IsNullOrWhiteSpace(_config.UsuarioSmtp))
                await cliente.AuthenticateAsync(_config.UsuarioSmtp, _config.SenhaSmtp, cancelamento);

            await cliente.SendAsync(email, cancelamento);
            await cliente.DisconnectAsync(true, cancelamento);

            return new ResultadoDoEnvio(true, StatusNotificacao.Enviada);
        }
        catch (Exception erro)
        {
            _log.LogError(erro, "Falha ao enviar e-mail para {Destino}", mensagem.Destino);
            return new ResultadoDoEnvio(false, StatusNotificacao.Falhou, erro.Message);
        }
    }
}
