using Florescer.Application.Contratos;
using Florescer.Application.Persistencia;
using Florescer.Application.Servicos;
using Florescer.Domain.Enums;
using Florescer.Domain.Regras;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Florescer.Infrastructure.Rotinas;

public class RotinaDeAgendamentos : BackgroundService
{
    private static readonly TimeSpan IntervaloEntreExecucoes = TimeSpan.FromMinutes(5);

    private readonly IServiceScopeFactory _fabricaDeEscopos;
    private readonly ILogger<RotinaDeAgendamentos> _log;

    public RotinaDeAgendamentos(
        IServiceScopeFactory fabricaDeEscopos, ILogger<RotinaDeAgendamentos> log)
    {
        _fabricaDeEscopos = fabricaDeEscopos;
        _log = log;
    }

    protected override async Task ExecuteAsync(CancellationToken parada)
    {
        _log.LogInformation(
            "Rotina de agendamentos iniciada. Revisão a cada {Minutos} minutos.",
            IntervaloEntreExecucoes.TotalMinutes);

        while (!parada.IsCancellationRequested)
        {
            try
            {
                await RevisarAgendaAsync(parada);
            }
            catch (Exception erro)
            {
                _log.LogError(erro, "Falha ao revisar a agenda.");
            }

            try
            {
                await Task.Delay(IntervaloEntreExecucoes, parada);
            }
            catch (TaskCanceledException)
            {
                break;
            }
        }
    }

    public async Task RevisarAgendaAsync(CancellationToken ct = default)
    {
        using var escopo = _fabricaDeEscopos.CreateScope();

        var banco = escopo.ServiceProvider.GetRequiredService<IBancoDeDados>();
        var relogio = escopo.ServiceProvider.GetRequiredService<IRelogio>();
        var notificacoes = escopo.ServiceProvider.GetRequiredService<ServicoDeNotificacao>();

        await CancelarNaoConfirmadosAsync(banco, relogio, notificacoes, ct);
        await EnviarLembretesDeConfirmacaoAsync(banco, relogio, notificacoes, ct);
        await EnviarLembretesDeAtendimentoAsync(banco, relogio, notificacoes, ct);
    }

    private async Task CancelarNaoConfirmadosAsync(
        IBancoDeDados banco, IRelogio relogio, ServicoDeNotificacao notificacoes, CancellationToken ct)
    {
        var agora = relogio.Agora;

        var vencidos = await banco.Agendamentos
            .Include(a => a.Cliente)
            .Include(a => a.Procedimento)
            .Include(a => a.Profissional)
            .Where(a => a.Status == StatusAgendamento.Agendado &&
                        a.PrazoConfirmacao < agora &&
                        a.Inicio > agora)
            .ToListAsync(ct);

        if (vencidos.Count == 0) return;

        foreach (var agendamento in vencidos)
        {
            agendamento.Status = StatusAgendamento.Cancelado;
            agendamento.CanceladoEm = agora;
            agendamento.MotivoCancelamento = MotivoCancelamento.FaltaDeConfirmacao;
        }

        await banco.SalvarAsync(ct);

        foreach (var agendamento in vencidos)
        {
            await notificacoes.EnviarAsync(
                agendamento.Cliente,
                TipoNotificacao.CanceladoPorFaltaDeConfirmacao,
                "Agendamento cancelado — Florescer",
                ServicoDeNotificacao.TextoCanceladoPorFaltaDeConfirmacao(
                    agendamento, agendamento.Procedimento.Nome),
                agendamento,
                ct);
        }

        _log.LogInformation(
            "{Total} agendamento(s) cancelado(s) por falta de confirmação.", vencidos.Count);
    }

    private async Task EnviarLembretesDeConfirmacaoAsync(
        IBancoDeDados banco, IRelogio relogio, ServicoDeNotificacao notificacoes, CancellationToken ct)
    {
        var agora = relogio.Agora;
        var janela = agora.AddHours(RegrasDeConfirmacao.HorasDeAntecedenciaDoLembrete);

        var candidatos = await banco.Agendamentos
            .Include(a => a.Cliente)
            .Include(a => a.Procedimento)
            .Where(a => a.Status == StatusAgendamento.Agendado &&
                        a.PrazoConfirmacao > agora &&
                        a.PrazoConfirmacao <= janela &&
                        !a.Notificacoes.Any(n => n.Tipo == TipoNotificacao.LembreteDeConfirmacao))
            .ToListAsync(ct);

        foreach (var agendamento in candidatos)
        {
            await notificacoes.EnviarAsync(
                agendamento.Cliente,
                TipoNotificacao.LembreteDeConfirmacao,
                "Confirme seu agendamento — Florescer",
                ServicoDeNotificacao.TextoLembreteDeConfirmacao(
                    agendamento, agendamento.Procedimento.Nome),
                agendamento,
                ct);
        }

        if (candidatos.Count > 0)
            _log.LogInformation("{Total} lembrete(s) de confirmação enviado(s).", candidatos.Count);
    }

    private async Task EnviarLembretesDeAtendimentoAsync(
        IBancoDeDados banco, IRelogio relogio, ServicoDeNotificacao notificacoes, CancellationToken ct)
    {
        var agora = relogio.Agora;
        var em24Horas = agora.AddHours(24);

        var candidatos = await banco.Agendamentos
            .Include(a => a.Cliente)
            .Include(a => a.Procedimento)
            .Include(a => a.Profissional)
            .Where(a => a.Status == StatusAgendamento.Confirmado &&
                        a.Inicio > agora &&
                        a.Inicio <= em24Horas &&
                        !a.Notificacoes.Any(n => n.Tipo == TipoNotificacao.LembreteDeAtendimento))
            .ToListAsync(ct);

        foreach (var agendamento in candidatos)
        {
            await notificacoes.EnviarAsync(
                agendamento.Cliente,
                TipoNotificacao.LembreteDeAtendimento,
                "Seu atendimento é amanhã — Florescer",
                ServicoDeNotificacao.TextoLembreteDeAtendimento(
                    agendamento, agendamento.Procedimento.Nome, agendamento.Profissional.Nome),
                agendamento,
                ct);
        }

        if (candidatos.Count > 0)
            _log.LogInformation("{Total} lembrete(s) de atendimento enviado(s).", candidatos.Count);
    }
}
