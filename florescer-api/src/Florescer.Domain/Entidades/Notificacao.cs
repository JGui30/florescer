using Florescer.Domain.Enums;

namespace Florescer.Domain.Entidades;

public class Notificacao
{
    public int Id { get; set; }

    public int UsuarioId { get; set; }
    public Usuario Usuario { get; set; } = null!;

    public int? AgendamentoId { get; set; }
    public Agendamento? Agendamento { get; set; }

    public CanalNotificacao Canal { get; set; }

    public TipoNotificacao Tipo { get; set; }

    public string Destino { get; set; } = string.Empty;

    public string Assunto { get; set; } = string.Empty;

    public string Mensagem { get; set; } = string.Empty;

    public StatusNotificacao Status { get; set; } = StatusNotificacao.Pendente;

    public string? Erro { get; set; }

    public DateTime? EnviadaEm { get; set; }

    public DateTime CriadaEm { get; set; }
}
