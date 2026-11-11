using Florescer.Domain.Entidades;

namespace Florescer.Application.Contratos;

public interface IServicoDeSenha
{
    string GerarHash(string senha);

    bool Conferir(string senha, string hashArmazenado);
}

public interface IGeradorDeToken
{
    string Gerar(Usuario usuario);

    int HorasDeValidade { get; }
}

public interface IUsuarioLogado
{
    int? Id { get; }

    bool EhAdministrador { get; }
}
