using Florescer.Application.Contratos;
using Florescer.Domain.Enums;
using Florescer.Domain.Regras;
using Microsoft.AspNetCore.Mvc;

namespace Florescer.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public abstract class ControllerDeApi : ControllerBase
{
    protected const string PerfilAdministrador = nameof(PerfilUsuario.Administrador);

    protected const string PerfilCliente = nameof(PerfilUsuario.Cliente);

    private readonly IUsuarioLogado _usuarioLogado;

    protected ControllerDeApi(IUsuarioLogado usuarioLogado) => _usuarioLogado = usuarioLogado;

    protected int UsuarioAtualId =>
        _usuarioLogado.Id ?? throw new RegraDeNegocioException("Sessão expirada. Faça login novamente.");

    protected bool EhAdministrador => _usuarioLogado.EhAdministrador;

    protected int? FiltroDeCliente => EhAdministrador ? null : UsuarioAtualId;

    protected int? ClienteAutenticadoOuNulo =>
        EhAdministrador ? null : _usuarioLogado.Id;
}
