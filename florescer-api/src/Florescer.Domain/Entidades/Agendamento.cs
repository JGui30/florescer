using Florescer.Domain.Enums;

namespace Florescer.Domain.Entidades;

public class Agendamento
{
    public int Id { get; set; }

    public int ClienteId { get; set; }
    public Usuario Cliente { get; set; } = null!;

    public int ProfissionalId { get; set; }
    public Profissional Profissional { get; set; } = null!;

    public int ProcedimentoId { get; set; }
    public Procedimento Procedimento { get; set; } = null!;

    public DateTime Inicio { get; set; }

    public DateTime Fim { get; set; }

    public StatusAgendamento Status { get; set; } = StatusAgendamento.Agendado;

    public decimal ValorPrevisto { get; set; }

    public DateTime PrazoConfirmacao { get; set; }

    public DateTime? ConfirmadoEm { get; set; }

    public DateTime? ConcluidoEm { get; set; }

    public DateTime? CanceladoEm { get; set; }

    public MotivoCancelamento? MotivoCancelamento { get; set; }

    public string? Observacoes { get; set; }

    public DateTime CriadoEm { get; set; }

    public LancamentoFinanceiro? Lancamento { get; set; }

    public ICollection<Notificacao> Notificacoes { get; set; } = new List<Notificacao>();

    public bool OcupaAgenda =>
        Status == StatusAgendamento.Agendado ||
        Status == StatusAgendamento.Confirmado ||
        Status == StatusAgendamento.Concluido;
}
