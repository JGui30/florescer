using Florescer.Application.Contratos;
using Florescer.Application.Dtos;
using Florescer.Application.Servicos;
using Florescer.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Florescer.API.Controllers;

[Route("api/financeiro")]
[Authorize(Roles = nameof(PerfilUsuario.Administrador))]
public class FinanceiroController : ControllerDeApi
{
    private readonly ServicoFinanceiro _servico;

    public FinanceiroController(ServicoFinanceiro servico, IUsuarioLogado usuarioLogado)
        : base(usuarioLogado)
    {
        _servico = servico;
    }

    [HttpGet("resumo")]
    public async Task<ActionResult<ResumoFinanceiroResposta>> Resumo(
        [FromQuery] string periodo = "mes",
        [FromQuery] DateTime? referencia = null,
        [FromQuery] DateTime? de = null,
        [FromQuery] DateTime? ate = null,
        CancellationToken ct = default)
    {
        return Ok(await _servico.ObterResumoAsync(periodo, referencia, de, ate, ct));
    }

    [HttpGet("a-fechar")]
    public async Task<ActionResult<IReadOnlyList<AtendimentoAFecharResposta>>> AFechar(
        CancellationToken ct)
    {
        return Ok(await _servico.ListarAtendimentosAFecharAsync(ct));
    }

    [HttpGet("lancamentos")]
    public async Task<ActionResult<IReadOnlyList<LancamentoResposta>>> Lancamentos(
        [FromQuery] DateTime? de = null,
        [FromQuery] DateTime? ate = null,
        CancellationToken ct = default)
    {
        return Ok(await _servico.ListarLancamentosAsync(de, ate, ct));
    }
}
