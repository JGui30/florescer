using Florescer.Application.Contratos;
using Florescer.Application.Dtos;
using Florescer.Application.Servicos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Florescer.API.Controllers;

[Route("api/auth")]
public class AutenticacaoController : ControllerDeApi
{
    private readonly ServicoDeAutenticacao _servico;

    public AutenticacaoController(ServicoDeAutenticacao servico, IUsuarioLogado usuarioLogado)
        : base(usuarioLogado)
    {
        _servico = servico;
    }

    [HttpPost("registrar")]
    [AllowAnonymous]
    [EnableRateLimiting("login")]
    public async Task<ActionResult<LoginResposta>> Registrar(
        RegistroRequisicao dados, CancellationToken ct)
    {
        return Ok(await _servico.RegistrarAsync(dados, ct));
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("login")]
    public async Task<ActionResult<LoginResposta>> Entrar(LoginRequisicao dados, CancellationToken ct)
    {
        return Ok(await _servico.EntrarAsync(dados, ct));
    }

    [HttpPost("login-admin")]
    [AllowAnonymous]
    [EnableRateLimiting("login")]
    public async Task<ActionResult<LoginResposta>> EntrarComoAdministrador(
        LoginRequisicao dados, CancellationToken ct)
    {
        return Ok(await _servico.EntrarComoAdministradorAsync(dados, ct));
    }

    [HttpGet("eu")]
    [Authorize]
    public async Task<ActionResult<UsuarioResposta>> ObterPerfil(CancellationToken ct)
    {
        return Ok(await _servico.ObterPerfilAsync(UsuarioAtualId, ct));
    }

    [HttpPut("eu")]
    [Authorize]
    public async Task<ActionResult<UsuarioResposta>> AtualizarPerfil(
        AtualizarPerfilRequisicao dados, CancellationToken ct)
    {
        return Ok(await _servico.AtualizarPerfilAsync(UsuarioAtualId, dados, ct));
    }

    [HttpPost("alterar-senha")]
    [Authorize]
    public async Task<IActionResult> AlterarSenha(AlterarSenhaRequisicao dados, CancellationToken ct)
    {
        await _servico.AlterarSenhaAsync(UsuarioAtualId, dados, ct);
        return NoContent();
    }

    [HttpPost("recuperar")]
    [AllowAnonymous]
    [EnableRateLimiting("recuperacao")]
    public async Task<ActionResult<SolicitarRecuperacaoResposta>> SolicitarRecuperacao(
        SolicitarRecuperacaoRequisicao dados, CancellationToken ct)
    {
        return Ok(await _servico.SolicitarRecuperacaoAsync(dados, ct));
    }

    [HttpPost("recuperar/validar")]
    [AllowAnonymous]
    [EnableRateLimiting("login")]
    public async Task<IActionResult> ValidarCodigo(
        [FromBody] ValidarCodigoRequisicao dados, CancellationToken ct)
    {
        await _servico.ValidarCodigoAsync(dados.Email, dados.Codigo, ct);
        return NoContent();
    }

    [HttpPost("recuperar/redefinir")]
    [AllowAnonymous]
    [EnableRateLimiting("login")]
    public async Task<IActionResult> RedefinirSenha(
        RedefinirSenhaRequisicao dados, CancellationToken ct)
    {
        await _servico.RedefinirSenhaAsync(dados, ct);
        return NoContent();
    }
}

public record ValidarCodigoRequisicao(string Email, string Codigo);
