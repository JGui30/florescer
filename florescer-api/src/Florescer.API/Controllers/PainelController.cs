using Florescer.Application.Contratos;
using Florescer.Application.Dtos;
using Florescer.Application.Persistencia;
using Florescer.Application.Servicos;
using Florescer.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Florescer.API.Controllers;

[Route("api/painel")]
[Authorize(Roles = nameof(Domain.Enums.PerfilUsuario.Administrador))]
public class PainelController : ControllerDeApi
{
    private readonly ServicoDePainel _painel;
    private readonly IBancoDeDados _banco;
    private readonly ServicoDeNotificacao _notificacoes;
    private readonly ICanalDeNotificacao _canal;

    public PainelController(
        ServicoDePainel painel,
        IBancoDeDados banco,
        ServicoDeNotificacao notificacoes,
        ICanalDeNotificacao canal,
        IUsuarioLogado usuarioLogado)
        : base(usuarioLogado)
    {
        _painel = painel;
        _banco = banco;
        _notificacoes = notificacoes;
        _canal = canal;
    }

    [HttpPost("testar-email")]
    public async Task<ActionResult<object>> TestarEmail(CancellationToken ct)
    {
        var admin = await _banco.Usuarios.FirstOrDefaultAsync(u => u.Id == UsuarioAtualId, ct);
        if (admin is null)
            return BadRequest(new { erro = "Usuário não encontrado." });

        var texto = string.Join(Environment.NewLine, new[]
        {
            $"Olá, {admin.Nome}!",
            "",
            "Este é um e-mail de teste do sistema Florescer.",
            "",
            "Se você recebeu esta mensagem, o envio está configurado corretamente e as",
            "clientes passarão a receber os avisos de agendamento, os lembretes de",
            "confirmação e os códigos de recuperação de senha.",
            "",
            $"Enviado em {DateTime.Now:dd/MM/yyyy 'às' HH:mm}.",
            "",
            "Equipe Florescer"
        });

        var registro = await _notificacoes.EnviarAsync(
            admin,
            TipoNotificacao.CodigoDeRecuperacao,
            "Teste de envio — Florescer",
            texto,
            null,
            ct);

        return Ok(new
        {
            canalConfigurado = _canal.EstaConfigurado,
            destino = admin.Email,
            status = registro.Status.ToString(),
            erro = registro.Erro,
            mensagem = _canal.EstaConfigurado
                ? "E-mail enviado. Confira a caixa de entrada e o spam."
                : "Servidor SMTP não configurado: a mensagem foi apenas registrada no sistema, sem sair pela internet."
        });
    }

    [HttpGet("visao-geral")]
    public async Task<ActionResult<VisaoGeralResposta>> VisaoGeral(
        [FromQuery] DateTime? data = null, CancellationToken ct = default)
    {
        return Ok(await _painel.ObterVisaoGeralAsync(data, ct));
    }

    [HttpGet("notificacoes")]
    public async Task<ActionResult<IReadOnlyList<NotificacaoResposta>>> Notificacoes(
        [FromQuery] int limite = 50, CancellationToken ct = default)
    {
        var notificacoes = await _banco.Notificacoes
            .Include(n => n.Usuario)
            .OrderByDescending(n => n.CriadaEm)
            .Take(Math.Clamp(limite, 1, 200))
            .Select(n => new NotificacaoResposta(
                n.Id,
                n.Usuario.Nome,
                n.Destino,
                n.Canal.ToString(),
                n.Tipo.ToString(),
                n.Assunto,
                n.Mensagem,
                n.Status.ToString(),
                n.CriadaEm))
            .ToListAsync(ct);

        return Ok(notificacoes);
    }
}

public record NotificacaoResposta(
    int Id,
    string Destinatario,
    string Destino,
    string Canal,
    string Tipo,
    string Assunto,
    string Mensagem,
    string Status,
    DateTime CriadaEm);
