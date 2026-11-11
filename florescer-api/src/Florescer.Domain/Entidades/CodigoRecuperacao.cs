using Florescer.Domain.Enums;

namespace Florescer.Domain.Entidades;

public class CodigoRecuperacao
{
    public int Id { get; set; }

    public int UsuarioId { get; set; }
    public Usuario Usuario { get; set; } = null!;

    public string CodigoHash { get; set; } = string.Empty;

    public CanalNotificacao Canal { get; set; }

    public DateTime ExpiraEm { get; set; }

    public DateTime? UsadoEm { get; set; }

    public int Tentativas { get; set; }

    public DateTime CriadoEm { get; set; }
}
