using Florescer.Infrastructure.Seguranca;

namespace Florescer.Tests;

public class ServicoDeSenhaTestes
{
    private readonly ServicoDeSenha _servico = new();

    [Fact]
    public void Senha_correta_e_aceita()
    {
        var hash = _servico.GerarHash("MinhaSenha@123");

        Assert.True(_servico.Conferir("MinhaSenha@123", hash));
    }

    [Fact]
    public void Senha_errada_e_recusada()
    {
        var hash = _servico.GerarHash("MinhaSenha@123");

        Assert.False(_servico.Conferir("SenhaErrada", hash));
    }

    [Fact]
    public void Senha_original_nunca_aparece_no_hash()
    {
        const string senha = "MinhaSenha@123";

        var hash = _servico.GerarHash(senha);

        Assert.DoesNotContain(senha, hash);
    }

    [Fact]
    public void Senhas_iguais_geram_hashes_diferentes()
    {
        var primeiro = _servico.GerarHash("MesmaSenha");
        var segundo = _servico.GerarHash("MesmaSenha");

        Assert.NotEqual(primeiro, segundo);

        Assert.True(_servico.Conferir("MesmaSenha", primeiro));
        Assert.True(_servico.Conferir("MesmaSenha", segundo));
    }

    [Fact]
    public void Hash_malformado_nao_derruba_o_sistema()
    {
        Assert.False(_servico.Conferir("qualquer", "isto-nao-e-um-hash"));
        Assert.False(_servico.Conferir("qualquer", "sem-ponto-separador"));
        Assert.False(_servico.Conferir("qualquer", "@@@.###"));
    }
}
