using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Florescer.Application.Contratos;
using Florescer.Domain.Entidades;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Florescer.Infrastructure.Seguranca;

public class GeradorDeToken : IGeradorDeToken
{
    private readonly ConfiguracoesDeToken _config;

    public GeradorDeToken(IOptions<ConfiguracoesDeToken> config)
    {
        _config = config.Value;
    }

    public int HorasDeValidade => _config.HorasDeValidade;

    public string Gerar(Usuario usuario)
    {
        var informacoes = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new(ClaimTypes.Name, usuario.Nome),
            new(ClaimTypes.Email, usuario.Email),
            new(ClaimTypes.Role, usuario.Perfil.ToString())
        };

        var chave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.Chave));
        var assinatura = new SigningCredentials(chave, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _config.Emissor,
            audience: _config.Audiencia,
            claims: informacoes,
            expires: DateTime.UtcNow.AddHours(_config.HorasDeValidade),
            signingCredentials: assinatura);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
