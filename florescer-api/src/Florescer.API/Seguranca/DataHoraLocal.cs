using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;

namespace Florescer.API.Seguranca;

public static class DataHoraLocal
{
    public const string Formato = "yyyy-MM-ddTHH:mm:ss";

    public static DateTime Normalizar(DateTime valor)
    {
        if (valor.Kind == DateTimeKind.Utc)
            valor = valor.ToLocalTime();

        return DateTime.SpecifyKind(valor, DateTimeKind.Unspecified);
    }
}

public class ConversorDeDataHora : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader leitor, Type tipo, JsonSerializerOptions opcoes)
        => DataHoraLocal.Normalizar(leitor.GetDateTime());

    public override void Write(Utf8JsonWriter escritor, DateTime valor, JsonSerializerOptions opcoes)
        => escritor.WriteStringValue(valor.ToString(DataHoraLocal.Formato));
}

public class ConversorDeDataHoraOpcional : JsonConverter<DateTime?>
{
    public override DateTime? Read(ref Utf8JsonReader leitor, Type tipo, JsonSerializerOptions opcoes)
        => leitor.TokenType == JsonTokenType.Null
            ? null
            : DataHoraLocal.Normalizar(leitor.GetDateTime());

    public override void Write(Utf8JsonWriter escritor, DateTime? valor, JsonSerializerOptions opcoes)
    {
        if (valor.HasValue)
            escritor.WriteStringValue(valor.Value.ToString(DataHoraLocal.Formato));
        else
            escritor.WriteNullValue();
    }
}

public class VinculadorDeDataHora : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext contexto)
    {
        var valor = contexto.ValueProvider.GetValue(contexto.ModelName);

        if (valor == ValueProviderResult.None)
            return Task.CompletedTask;

        contexto.ModelState.SetModelValue(contexto.ModelName, valor);

        var texto = valor.FirstValue;

        if (string.IsNullOrWhiteSpace(texto))
        {
            if (Nullable.GetUnderlyingType(contexto.ModelType) is not null)
                contexto.Result = ModelBindingResult.Success(null);

            return Task.CompletedTask;
        }

        if (DateTime.TryParse(
                texto,
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None,
                out var data))
        {
            contexto.Result = ModelBindingResult.Success(DataHoraLocal.Normalizar(data));
        }
        else
        {
            contexto.ModelState.TryAddModelError(contexto.ModelName, "Data inválida.");
        }

        return Task.CompletedTask;
    }
}

public class ProvedorDeVinculadorDeData : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext contexto)
    {
        var tipo = Nullable.GetUnderlyingType(contexto.Metadata.ModelType) ?? contexto.Metadata.ModelType;

        return tipo == typeof(DateTime) ? new VinculadorDeDataHora() : null;
    }
}
