using Florescer.Domain.Regras;

namespace Florescer.API.Armazenamento;

public class ArmazenamentoDeImagens
{
    private static readonly string[] ExtensoesPermitidas =
        { ".jpg", ".jpeg", ".png", ".webp", ".gif" };

    public const long TamanhoMaximoEmBytes = 5 * 1024 * 1024;

    private readonly IWebHostEnvironment _ambiente;

    public ArmazenamentoDeImagens(IWebHostEnvironment ambiente) => _ambiente = ambiente;

    private string Pasta => Path.Combine(_ambiente.WebRootPath ?? "wwwroot", "uploads");

    public async Task<string> GravarAsync(IFormFile arquivo, CancellationToken ct = default)
    {
        if (arquivo is null || arquivo.Length == 0)
            throw new RegraDeNegocioException("Selecione uma imagem para enviar.");

        if (arquivo.Length > TamanhoMaximoEmBytes)
            throw new RegraDeNegocioException("A imagem deve ter no máximo 5 MB.");

        var extensao = Path.GetExtension(arquivo.FileName).ToLowerInvariant();

        if (!ExtensoesPermitidas.Contains(extensao))
            throw new RegraDeNegocioException("Formato não aceito. Envie JPG, PNG, WEBP ou GIF.");

        if (!await PareceImagemAsync(arquivo, cancelamento: ct))
            throw new RegraDeNegocioException("O arquivo enviado não é uma imagem válida.");

        Directory.CreateDirectory(Pasta);

        var nomeGerado = $"{Guid.NewGuid():N}{extensao}";
        var caminho = Path.Combine(Pasta, nomeGerado);

        await using (var destino = File.Create(caminho))
        {
            await arquivo.CopyToAsync(destino, ct);
        }

        return $"/uploads/{nomeGerado}";
    }

    private static async Task<bool> PareceImagemAsync(
        IFormFile arquivo, CancellationToken cancelamento)
    {
        var inicio = new byte[12];

        await using (var fluxo = arquivo.OpenReadStream())
        {
            var lidos = await fluxo.ReadAsync(inicio, cancelamento);
            if (lidos < 12) return false;
        }

        if (inicio[0] == 0xFF && inicio[1] == 0xD8 && inicio[2] == 0xFF) return true;

        if (inicio[0] == 0x89 && inicio[1] == 0x50 && inicio[2] == 0x4E && inicio[3] == 0x47 &&
            inicio[4] == 0x0D && inicio[5] == 0x0A && inicio[6] == 0x1A && inicio[7] == 0x0A) return true;

        if (inicio[0] == 0x47 && inicio[1] == 0x49 && inicio[2] == 0x46) return true;

        if (inicio[0] == 0x52 && inicio[1] == 0x49 && inicio[2] == 0x46 && inicio[3] == 0x46 &&
            inicio[8] == 0x57 && inicio[9] == 0x45 && inicio[10] == 0x42 && inicio[11] == 0x50) return true;

        return false;
    }

    public void Apagar(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return;

        var nome = Path.GetFileName(url);
        if (string.IsNullOrWhiteSpace(nome)) return;

        var caminho = Path.Combine(Pasta, nome);

        if (!Path.GetFullPath(caminho).StartsWith(Path.GetFullPath(Pasta), StringComparison.OrdinalIgnoreCase))
            return;

        if (File.Exists(caminho))
            File.Delete(caminho);
    }
}
