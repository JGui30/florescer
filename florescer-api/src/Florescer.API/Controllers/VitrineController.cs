using Florescer.Application.Contratos;
using Florescer.Application.Dtos;
using Florescer.Application.Servicos;
using Florescer.Domain.Enums;
using Florescer.Domain.Regras;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Florescer.API.Controllers;

[Route("api/landing")]
[ApiExplorerSettings(IgnoreApi = true)]
public class VitrineController : ControllerDeApi
{
    private static readonly string[] ExtensoesPermitidas = { ".jpg", ".jpeg", ".png", ".webp", ".gif" };

    private const long TamanhoMaximoEmBytes = 5 * 1024 * 1024;

    private readonly ServicoDeLanding _servico;
    private readonly IWebHostEnvironment _ambiente;

    public VitrineController(
        ServicoDeLanding servico, IWebHostEnvironment ambiente, IUsuarioLogado usuarioLogado)
        : base(usuarioLogado)
    {
        _servico = servico;
        _ambiente = ambiente;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<LandingResposta>> Obter(
        [FromQuery] bool incluirInativos = false, CancellationToken ct = default)
    {
        var incluir = incluirInativos && EhAdministrador;
        return Ok(await _servico.ObterAsync(incluir, ct));
    }

    [HttpPut("estabelecimento")]
    [Authorize(Roles = nameof(PerfilUsuario.Administrador))]
    public async Task<ActionResult<EstabelecimentoResposta>> AtualizarEstabelecimento(
        EstabelecimentoRequisicao dados, CancellationToken ct)
    {
        return Ok(await _servico.AtualizarEstabelecimentoAsync(dados, ct));
    }

    [HttpPut("secoes/{id:int}")]
    [Authorize(Roles = nameof(PerfilUsuario.Administrador))]
    public async Task<ActionResult<SecaoLandingResposta>> AtualizarSecao(
        int id, SecaoLandingRequisicao dados, CancellationToken ct)
    {
        return Ok(await _servico.AtualizarSecaoAsync(id, dados, ct));
    }

    [HttpPost("midias")]
    [Authorize(Roles = nameof(PerfilUsuario.Administrador))]
    [RequestSizeLimit(TamanhoMaximoEmBytes)]
    public async Task<ActionResult<MidiaResposta>> EnviarImagem(
        IFormFile arquivo,
        [FromQuery] int? secaoId = null,
        [FromQuery] string? legenda = null,
        CancellationToken ct = default)
    {
        if (arquivo is null || arquivo.Length == 0)
            throw new RegraDeNegocioException("Selecione uma imagem para enviar.");

        if (arquivo.Length > TamanhoMaximoEmBytes)
            throw new RegraDeNegocioException("A imagem deve ter no máximo 5 MB.");

        var extensao = Path.GetExtension(arquivo.FileName).ToLowerInvariant();

        if (!ExtensoesPermitidas.Contains(extensao))
            throw new RegraDeNegocioException("Formato não aceito. Envie JPG, PNG, WEBP ou GIF.");

        var pasta = Path.Combine(_ambiente.WebRootPath ?? "wwwroot", "uploads");
        Directory.CreateDirectory(pasta);

        var nomeGerado = $"{Guid.NewGuid():N}{extensao}";
        var caminho = Path.Combine(pasta, nomeGerado);

        await using (var destino = System.IO.File.Create(caminho))
        {
            await arquivo.CopyToAsync(destino, ct);
        }

        return Ok(await _servico.AdicionarMidiaAsync(secaoId, $"/uploads/{nomeGerado}", legenda, ct));
    }

    [HttpDelete("midias/{id:int}")]
    [Authorize(Roles = nameof(PerfilUsuario.Administrador))]
    public async Task<IActionResult> RemoverImagem(int id, CancellationToken ct)
    {
        var url = await _servico.RemoverMidiaAsync(id, ct);
        ApagarArquivo(url);
        return NoContent();
    }

    private void ApagarArquivo(string url)
    {
        if (string.IsNullOrWhiteSpace(url)) return;

        var nome = Path.GetFileName(url);
        if (string.IsNullOrWhiteSpace(nome)) return;

        var pasta = Path.Combine(_ambiente.WebRootPath ?? "wwwroot", "uploads");
        var caminho = Path.Combine(pasta, nome);

        if (!Path.GetFullPath(caminho).StartsWith(Path.GetFullPath(pasta), StringComparison.OrdinalIgnoreCase))
            return;

        if (System.IO.File.Exists(caminho))
            System.IO.File.Delete(caminho);
    }

    [HttpPost("faqs")]
    [Authorize(Roles = nameof(PerfilUsuario.Administrador))]
    public async Task<ActionResult<FaqResposta>> CriarFaq(FaqRequisicao dados, CancellationToken ct)
    {
        return Ok(await _servico.CriarFaqAsync(dados, ct));
    }

    [HttpPut("faqs/{id:int}")]
    [Authorize(Roles = nameof(PerfilUsuario.Administrador))]
    public async Task<ActionResult<FaqResposta>> AtualizarFaq(
        int id, FaqRequisicao dados, CancellationToken ct)
    {
        return Ok(await _servico.AtualizarFaqAsync(id, dados, ct));
    }

    [HttpDelete("faqs/{id:int}")]
    [Authorize(Roles = nameof(PerfilUsuario.Administrador))]
    public async Task<IActionResult> RemoverFaq(int id, CancellationToken ct)
    {
        await _servico.RemoverFaqAsync(id, ct);
        return NoContent();
    }
}
