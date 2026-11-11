using Florescer.Domain.Enums;

namespace Florescer.Domain.Entidades;

public class Usuario
{
    public int Id { get; set; }

    public string Nome { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? Telefone { get; set; }

    public string SenhaHash { get; set; } = string.Empty;

    public PerfilUsuario Perfil { get; set; } = PerfilUsuario.Cliente;

    public bool Ativo { get; set; } = true;

    public DateTime CriadoEm { get; set; }

    public ICollection<Agendamento> Agendamentos { get; set; } = new List<Agendamento>();
    public ICollection<CodigoRecuperacao> CodigosRecuperacao { get; set; } = new List<CodigoRecuperacao>();
}
