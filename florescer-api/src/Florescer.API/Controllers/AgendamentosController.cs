using Florescer.Application.Contratos;
using Florescer.Application.Dtos;
using Florescer.Application.Servicos;
using Florescer.Domain.Enums;
using Florescer.Infrastructure.Rotinas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Florescer.API.Controllers;

[Route("api/agendamentos")]
[Authorize]
public class AgendamentosController : ControllerDeApi
{
    private readonly ServicoDeAgendamento _servico;
    private readonly RotinaDeAgendamentos _rotina;

    public AgendamentosController(
        ServicoDeAgendamento servico,
        RotinaDeAgendamentos rotina,
        IUsuarioLogado usuarioLogado)
        : base(usuarioLogado)
    {
        _servico = servico;
        _rotina = rotina;
    }

    [HttpGet("meus")]
    public async Task<ActionResult<IReadOnlyList<AgendamentoResposta>>> Meus(CancellationToken ct)
    {
        return Ok(await _servico.ListarDoClienteAsync(UsuarioAtualId, ct));
    }

    [HttpGet("previa")]
    public async Task<ActionResult<PreviaDeAgendamentoResposta>> Previa(
        [FromQuery] int procedimentoId,
        [FromQuery] DateTime inicio,
        CancellationToken ct)
    {
        return Ok(await _servico.CalcularPreviaAsync(procedimentoId, inicio, ct));
    }

    [HttpPost]
    public async Task<ActionResult<AgendamentoResposta>> Criar(
        CriarAgendamentoRequisicao dados, CancellationToken ct)
    {
        return Ok(await _servico.CriarAsync(UsuarioAtualId, dados, ct));
    }

    [HttpPost("{id:int}/confirmar")]
    public async Task<ActionResult<AgendamentoResposta>> Confirmar(int id, CancellationToken ct)
    {
        return Ok(await _servico.ConfirmarAsync(id, FiltroDeCliente, ct));
    }

    [HttpPost("{id:int}/cancelar")]
    public async Task<ActionResult<AgendamentoResposta>> Cancelar(int id, CancellationToken ct)
    {
        return Ok(await _servico.CancelarAsync(id, FiltroDeCliente, EhAdministrador, ct));
    }

    [HttpPost("{id:int}/reagendar")]
    public async Task<ActionResult<AgendamentoResposta>> Reagendar(
        int id, ReagendarRequisicao dados, CancellationToken ct)
    {
        return Ok(await _servico.ReagendarAsync(id, FiltroDeCliente, dados, EhAdministrador, ct));
    }

    [HttpGet]
    [Authorize(Roles = PerfilAdministrador)]
    public async Task<ActionResult<IReadOnlyList<AgendamentoResposta>>> Listar(
        [FromQuery] DateTime? data = null,
        [FromQuery] DateTime? de = null,
        [FromQuery] DateTime? ate = null,
        [FromQuery] int? profissionalId = null,
        [FromQuery] StatusAgendamento? status = null,
        CancellationToken ct = default)
    {
        return Ok(await _servico.ListarAsync(data, de, ate, profissionalId, status, ct));
    }

    [HttpPost("{id:int}/concluir")]
    [Authorize(Roles = PerfilAdministrador)]
    public async Task<ActionResult<AgendamentoResposta>> Concluir(
        int id, ConcluirAtendimentoRequisicao dados, CancellationToken ct)
    {
        return Ok(await _servico.ConcluirAsync(id, UsuarioAtualId, dados, ct));
    }

    [HttpPost("{id:int}/falta")]
    [Authorize(Roles = PerfilAdministrador)]
    public async Task<ActionResult<AgendamentoResposta>> RegistrarFalta(int id, CancellationToken ct)
    {
        return Ok(await _servico.RegistrarFaltaAsync(id, ct));
    }

    [HttpGet("/api/agenda/dias-com-movimento")]
    [Authorize(Roles = PerfilAdministrador)]
    public async Task<ActionResult<IReadOnlyList<DiaComMovimentoResposta>>> DiasComMovimento(
        [FromQuery] DateTime de,
        [FromQuery] DateTime ate,
        [FromQuery] int? profissionalId = null,
        CancellationToken ct = default)
    {
        return Ok(await _servico.ListarDiasComMovimentoAsync(de, ate, profissionalId, ct));
    }

    [HttpGet("/api/agenda/periodo")]
    [Authorize(Roles = PerfilAdministrador)]
    public async Task<ActionResult<PeriodoDaAgendaResposta>> PeriodoDaAgenda(CancellationToken ct)
    {
        return Ok(await _servico.ObterPeriodoDaAgendaAsync(ct));
    }

    [HttpPost("/api/agenda/revisar")]
    [Authorize(Roles = PerfilAdministrador)]
    public async Task<IActionResult> RevisarAgora(CancellationToken ct)
    {
        await _rotina.RevisarAgendaAsync(ct);
        return Ok(new { mensagem = "Revisão da agenda executada." });
    }
}
