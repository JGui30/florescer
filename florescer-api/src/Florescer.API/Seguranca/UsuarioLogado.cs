using System.Security.Claims;
using Florescer.Application.Contratos;
using Florescer.Domain.Enums;

namespace Florescer.API.Seguranca;

public class UsuarioLogado : IUsuarioLogado
{
    private readonly IHttpContextAccessor _acessor;

    public UsuarioLogado(IHttpContextAccessor acessor) => _acessor = acessor;

    public int? Id
    {
        get
        {
            var texto = _acessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(texto, out var id) ? id : null;
        }
    }

    public bool EhAdministrador =>
        _acessor.HttpContext?.User?.IsInRole(nameof(PerfilUsuario.Administrador)) ?? false;
}
