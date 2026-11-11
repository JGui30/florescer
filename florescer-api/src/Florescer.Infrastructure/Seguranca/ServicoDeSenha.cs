using System.Security.Cryptography;
using Florescer.Application.Contratos;

namespace Florescer.Infrastructure.Seguranca;

public class ServicoDeSenha : IServicoDeSenha
{
    private const int TamanhoDoSalt = 16;
    private const int TamanhoDoHash = 32;
    private const int Iteracoes = 100_000;

    public string GerarHash(string senha)
    {
        var salt = RandomNumberGenerator.GetBytes(TamanhoDoSalt);
        var hash = Calcular(senha, salt);

        return $"{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }

    public bool Conferir(string senha, string hashArmazenado)
    {
        var partes = hashArmazenado.Split('.');
        if (partes.Length != 2)
            return false;

        try
        {
            var salt = Convert.FromBase64String(partes[0]);
            var hashEsperado = Convert.FromBase64String(partes[1]);
            var hashCalculado = Calcular(senha, salt);

            return CryptographicOperations.FixedTimeEquals(hashCalculado, hashEsperado);
        }
        catch (FormatException)
        {
            return false;
        }
    }

    private static byte[] Calcular(string senha, byte[] salt)
    {
        return Rfc2898DeriveBytes.Pbkdf2(
            password: senha,
            salt: salt,
            iterations: Iteracoes,
            hashAlgorithm: HashAlgorithmName.SHA256,
            outputLength: TamanhoDoHash);
    }
}
