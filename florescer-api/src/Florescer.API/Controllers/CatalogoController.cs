using Florescer.API.Armazenamento;
using Florescer.Application.Contratos;
using Florescer.Application.Dtos;
using Florescer.Application.Servicos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Florescer.API.Controllers;

[Route("api/catalogo")]
public class CatalogoController : ControllerDeApi
{
    private readonly ServicoDeCatalogo _servico;
    private readonly ArmazenamentoDeImagens _imagens;

    public CatalogoController(
        ServicoDeCatalogo servico,
        ArmazenamentoDeImagens imagens,
        IUsuarioLogado usuarioLogado)
        : base(usuarioLogado)
    {
        _servico = servico;
        _imagens = imagens;
    }

    [HttpGet("categorias")]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyList<CategoriaResposta>>> ListarCategorias(
        [FromQuery] bool incluirInativos = false, CancellationToken ct = default)
    {
        return Ok(await _servico.ListarCategoriasAsync(!incluirInativos, ct));
    }

    [HttpGet("procedimentos")]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyList<ProcedimentoResposta>>> ListarProcedimentos(
        [FromQuery] bool incluirInativos = false, CancellationToken ct = default)
    {
        return Ok(await _servico.ListarProcedimentosAsync(!incluirInativos, ct));
    }

    [HttpPost("procedimentos")]
    [Authorize(Roles = PerfilAdministrador)]
    public async Task<ActionResult<ProcedimentoResposta>> CriarProcedimento(
        ProcedimentoRequisicao dados, CancellationToken ct)
    {
        return Ok(await _servico.CriarProcedimentoAsync(dados, ct));
    }

    [HttpPut("procedimentos/{id:int}")]
    [Authorize(Roles = PerfilAdministrador)]
    public async Task<ActionResult<ProcedimentoResposta>> AtualizarProcedimento(
        int id, ProcedimentoRequisicao dados, CancellationToken ct)
    {
        return Ok(await _servico.AtualizarProcedimentoAsync(id, dados, ct));
    }

    [HttpDelete("procedimentos/{id:int}")]
    [Authorize(Roles = PerfilAdministrador)]
    public async Task<IActionResult> DesativarProcedimento(int id, CancellationToken ct)
    {
        await _servico.DesativarProcedimentoAsync(id, ct);
        return NoContent();
    }

    [HttpPost("categorias")]
    [Authorize(Roles = PerfilAdministrador)]
    public async Task<ActionResult<CategoriaResposta>> CriarCategoria(
        CategoriaRequisicao dados, CancellationToken ct)
    {
        return Ok(await _servico.CriarCategoriaAsync(dados, ct));
    }

    [HttpPut("categorias/{id:int}")]
    [Authorize(Roles = PerfilAdministrador)]
    public async Task<ActionResult<CategoriaResposta>> AtualizarCategoria(
        int id, CategoriaRequisicao dados, CancellationToken ct)
    {
        return Ok(await _servico.AtualizarCategoriaAsync(id, dados, ct));
    }

    [HttpDelete("categorias/{id:int}")]
    [Authorize(Roles = PerfilAdministrador)]
    public async Task<IActionResult> DesativarCategoria(int id, CancellationToken ct)
    {
        await _servico.DesativarCategoriaAsync(id, ct);
        return NoContent();
    }

    [HttpPost("categorias/{id:int}/imagem")]
    [Authorize(Roles = PerfilAdministrador)]
    [RequestSizeLimit(ArmazenamentoDeImagens.TamanhoMaximoEmBytes)]
    public async Task<ActionResult<object>> EnviarImagemDaCategoria(
        int id, IFormFile arquivo, CancellationToken ct)
    {
        var url = await _imagens.GravarAsync(arquivo, ct);

        try
        {
            var anterior = await _servico.DefinirImagemAsync(id, url, ct);
            _imagens.Apagar(anterior);
        }
        catch
        {
            _imagens.Apagar(url);
            throw;
        }

        return Ok(new { imagemUrl = url });
    }

    [HttpDelete("categorias/{id:int}/imagem")]
    [Authorize(Roles = PerfilAdministrador)]
    public async Task<IActionResult> RemoverImagemDaCategoria(int id, CancellationToken ct)
    {
        var anterior = await _servico.RemoverImagemAsync(id, ct);
        _imagens.Apagar(anterior);
        return NoContent();
    }
}
