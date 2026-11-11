using Florescer.Application.Contratos;
using Florescer.Application.Dtos;
using Florescer.Application.Servicos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Florescer.API.Controllers;

[Route("api/profissionais")]
public class ProfissionaisController : ControllerDeApi
{
    private readonly ServicoDeProfissionais _servico;
    private readonly ServicoDeDisponibilidade _disponibilidade;

    public ProfissionaisController(
        ServicoDeProfissionais servico,
        ServicoDeDisponibilidade disponibilidade,
        IUsuarioLogado usuarioLogado)
        : base(usuarioLogado)
    {
        _servico = servico;
        _disponibilidade = disponibilidade;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyList<ProfissionalResposta>>> Listar(
        [FromQuery] bool apenasAtivas = false,
        [FromQuery] int? procedimentoId = null,
        CancellationToken ct = default)
    {
        return Ok(await _servico.ListarAsync(
            apenasAtivas, procedimentoId, ClienteAutenticadoOuNulo, ct));
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<ProfissionalResposta>> Obter(int id, CancellationToken ct)
    {
        return Ok(await _servico.ObterAsync(id, ct));
    }

    [HttpGet("{id:int}/disponibilidade")]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyList<DisponibilidadeDoDiaResposta>>> Disponibilidade(
        int id,
        [FromQuery] int procedimentoId,
        [FromQuery] DateTime? de = null,
        [FromQuery] int dias = 7,
        CancellationToken ct = default)
    {
        var inicio = de ?? DateTime.Today;
        var quantidade = Math.Clamp(dias, 1, 31);

        return Ok(await _disponibilidade.ObterDeVariosDiasAsync(id, procedimentoId, inicio, quantidade, ct));
    }

    [HttpPost]
    [Authorize(Roles = PerfilAdministrador)]
    public async Task<ActionResult<ProfissionalResposta>> Criar(
        ProfissionalRequisicao dados, CancellationToken ct)
    {
        return Ok(await _servico.CriarAsync(dados, ct));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = PerfilAdministrador)]
    public async Task<ActionResult<ProfissionalResposta>> Atualizar(
        int id, ProfissionalRequisicao dados, CancellationToken ct)
    {
        return Ok(await _servico.AtualizarAsync(id, dados, ct));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = PerfilAdministrador)]
    public async Task<IActionResult> Desativar(int id, CancellationToken ct)
    {
        await _servico.DesativarAsync(id, ct);
        return NoContent();
    }

    [HttpGet("/api/bloqueios")]
    [Authorize(Roles = PerfilAdministrador)]
    public async Task<ActionResult<IReadOnlyList<BloqueioResposta>>> ListarBloqueios(
        [FromQuery] DateTime? de = null,
        [FromQuery] DateTime? ate = null,
        CancellationToken ct = default)
    {
        return Ok(await _servico.ListarBloqueiosAsync(de, ate, ct));
    }

    [HttpPost("/api/bloqueios")]
    [Authorize(Roles = PerfilAdministrador)]
    public async Task<ActionResult<BloqueioResposta>> CriarBloqueio(
        BloqueioRequisicao dados, CancellationToken ct)
    {
        return Ok(await _servico.CriarBloqueioAsync(dados, ct));
    }

    [HttpDelete("/api/bloqueios/{id:int}")]
    [Authorize(Roles = PerfilAdministrador)]
    public async Task<IActionResult> RemoverBloqueio(int id, CancellationToken ct)
    {
        await _servico.RemoverBloqueioAsync(id, ct);
        return NoContent();
    }
}
