using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Florescer.API.Armazenamento;
using Florescer.API.Seguranca;
using Florescer.Application.Contratos;
using Florescer.Infrastructure;
using Florescer.Infrastructure.Seguranca;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

const string LimiteDeLogin = "login";
const string LimiteDeRecuperacao = "recuperacao";

var construtor = WebApplication.CreateBuilder(args);

construtor.Services.AddControllers(opcoes =>
    {
        opcoes.ModelBinderProviders.Insert(0, new ProvedorDeVinculadorDeData());
    })
    .AddJsonOptions(opcoes =>
    {
        opcoes.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());

        opcoes.JsonSerializerOptions.Converters.Add(new ConversorDeDataHora());
        opcoes.JsonSerializerOptions.Converters.Add(new ConversorDeDataHoraOpcional());
    });

construtor.Services.AddHttpContextAccessor();
construtor.Services.AddScoped<IUsuarioLogado, UsuarioLogado>();

construtor.Services.AddScoped<ArmazenamentoDeImagens>();

construtor.Services.AdicionarInfraestrutura(construtor.Configuration);

var configuracaoDeToken = construtor.Configuration
    .GetSection(ConfiguracoesDeToken.Secao)
    .Get<ConfiguracoesDeToken>() ?? new ConfiguracoesDeToken();

if (string.IsNullOrWhiteSpace(configuracaoDeToken.Chave))
{
    throw new InvalidOperationException(
        "A chave JWT não foi configurada. Em desenvolvimento ela fica em " +
        "appsettings.Development.json; em produção, defina a variável de " +
        "ambiente Jwt__Chave no servidor de hospedagem.");
}

construtor.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opcoes =>
    {
        opcoes.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = configuracaoDeToken.Emissor,
            ValidAudience = configuracaoDeToken.Audiencia,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(configuracaoDeToken.Chave)),
            ClockSkew = TimeSpan.Zero
        };
    });

construtor.Services.AddAuthorization();

construtor.Services.AddRateLimiter(opcoes =>
{
    opcoes.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    opcoes.AddPolicy(LimiteDeLogin, contexto =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: contexto.Connection.RemoteIpAddress?.ToString() ?? "desconhecido",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));

    opcoes.AddPolicy(LimiteDeRecuperacao, contexto =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: contexto.Connection.RemoteIpAddress?.ToString() ?? "desconhecido",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 3,
                Window = TimeSpan.FromMinutes(10),
                QueueLimit = 0
            }));
});

const string PoliticaDoFront = "PermitirFrontEnd";

var enderecosDoFront = construtor.Configuration
    .GetSection("Cors:OrigensPermitidas")
    .Get<string[]>() ?? new[] { "http://localhost:5173" };

construtor.Services.AddCors(opcoes =>
{
    opcoes.AddPolicy(PoliticaDoFront, politica => politica
        .WithOrigins(enderecosDoFront)
        .AllowAnyHeader()
        .AllowAnyMethod());
});

construtor.Services.AddEndpointsApiExplorer();
construtor.Services.AddSwaggerGen(opcoes =>
{
    opcoes.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Florescer — API",
        Version = "v1",
        Description =
            "API do sistema de agendamento do centro de beleza e estética Florescer. " +
            "Trabalho de Conclusão de Curso — Sistemas de Informação."
    });

    opcoes.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Informe apenas o token gerado no login."
    });

    opcoes.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var aplicacao = construtor.Build();

await aplicacao.Services.PrepararBancoAsync();

aplicacao.UseMiddleware<TratadorDeErros>();

if (aplicacao.Environment.IsDevelopment())
{
    aplicacao.UseSwagger();
    aplicacao.UseSwaggerUI(opcoes =>
    {
        opcoes.SwaggerEndpoint("/swagger/v1/swagger.json", "Florescer API v1");
        opcoes.DocumentTitle = "Florescer — Documentação da API";
    });
}
else
{
    aplicacao.UseHsts();
    aplicacao.UseHttpsRedirection();
}

aplicacao.Use(async (contexto, proximo) =>
{
    contexto.Response.Headers["X-Content-Type-Options"] = "nosniff";
    await proximo();
});

aplicacao.UseStaticFiles();

aplicacao.UseCors(PoliticaDoFront);

aplicacao.UseRateLimiter();

aplicacao.UseAuthentication();
aplicacao.UseAuthorization();

aplicacao.MapControllers();

if (aplicacao.Environment.IsDevelopment())
{
    aplicacao.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();
}
else
{
    aplicacao.MapGet("/", () => Results.Ok(new { servico = "Florescer API", situacao = "no ar" }))
             .ExcludeFromDescription();
}

aplicacao.Run();
