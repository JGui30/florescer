using Florescer.Domain.Regras;

namespace Florescer.API.Seguranca;

public class TratadorDeErros
{
    private readonly RequestDelegate _proximo;
    private readonly ILogger<TratadorDeErros> _log;

    public TratadorDeErros(RequestDelegate proximo, ILogger<TratadorDeErros> log)
    {
        _proximo = proximo;
        _log = log;
    }

    public async Task InvokeAsync(HttpContext contexto)
    {
        try
        {
            await _proximo(contexto);
        }
        catch (RegraDeNegocioException erro)
        {
            _log.LogWarning("Regra de negócio: {Mensagem}", erro.Message);

            contexto.Response.StatusCode = StatusCodes.Status400BadRequest;
            contexto.Response.ContentType = "application/json";
            await contexto.Response.WriteAsJsonAsync(new { erro = erro.Message });
        }
        catch (Exception erro)
        {
            _log.LogError(erro, "Erro inesperado ao processar {Caminho}", contexto.Request.Path);

            contexto.Response.StatusCode = StatusCodes.Status500InternalServerError;
            contexto.Response.ContentType = "application/json";
            await contexto.Response.WriteAsJsonAsync(new
            {
                erro = "Não foi possível concluir a operação. Tente novamente em instantes."
            });
        }
    }
}
