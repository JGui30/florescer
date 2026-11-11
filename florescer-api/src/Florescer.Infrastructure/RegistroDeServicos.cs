using Florescer.Application.Contratos;
using Florescer.Application.Persistencia;
using Florescer.Application.Servicos;
using Florescer.Infrastructure.Notificacoes;
using Florescer.Infrastructure.Persistencia;
using Florescer.Infrastructure.Rotinas;
using Florescer.Infrastructure.Seguranca;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Florescer.Infrastructure;

public static class RegistroDeServicos
{
    public static IServiceCollection AdicionarInfraestrutura(
        this IServiceCollection servicos, IConfiguration configuracao)
    {
        var bancoAtivo = configuracao["BancoAtivo"] ?? "Local";
        var conexao = configuracao.GetConnectionString(bancoAtivo);

        if (string.IsNullOrWhiteSpace(conexao))
        {
            throw new InvalidOperationException(
                $"A string de conexão \"{bancoAtivo}\" não foi preenchida no appsettings.json. " +
                "Informe a conexão ou volte a chave \"BancoAtivo\" para \"Local\".");
        }

        servicos.AddDbContext<FlorescerDbContext>(opcoes =>
            opcoes.UseSqlServer(conexao, sql =>
            {
                sql.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(10),
                    errorNumbersToAdd: null);
            }));

        servicos.AddScoped<IBancoDeDados>(sp => sp.GetRequiredService<FlorescerDbContext>());

        servicos.Configure<ConfiguracoesDeToken>(
            configuracao.GetSection(ConfiguracoesDeToken.Secao));

        servicos.Configure<ConfiguracoesDeNotificacao>(
            configuracao.GetSection(ConfiguracoesDeNotificacao.Secao));

        servicos.AddSingleton<IServicoDeSenha, ServicoDeSenha>();
        servicos.AddSingleton<IGeradorDeToken, GeradorDeToken>();
        servicos.AddSingleton<IRelogio, RelogioDoSistema>();

        servicos.AddScoped<ICanalDeNotificacao, CanalDeEmail>();

        servicos.AddScoped<ServicoDeNotificacao>();
        servicos.AddScoped<ServicoDeAutenticacao>();
        servicos.AddScoped<ServicoDeCatalogo>();
        servicos.AddScoped<ServicoDeProfissionais>();
        servicos.AddScoped<ServicoDeDisponibilidade>();
        servicos.AddScoped<ServicoDeAgendamento>();
        servicos.AddScoped<ServicoDePainel>();
        servicos.AddScoped<ServicoFinanceiro>();
        servicos.AddScoped<ServicoDeLanding>();

        servicos.AddSingleton<RotinaDeAgendamentos>();
        servicos.AddHostedService(sp => sp.GetRequiredService<RotinaDeAgendamentos>());

        return servicos;
    }

    public static async Task PrepararBancoAsync(this IServiceProvider provedor)
    {
        using var escopo = provedor.CreateScope();

        var banco = escopo.ServiceProvider.GetRequiredService<FlorescerDbContext>();
        var senhas = escopo.ServiceProvider.GetRequiredService<IServicoDeSenha>();
        var relogio = escopo.ServiceProvider.GetRequiredService<IRelogio>();

        await banco.Database.MigrateAsync();
        await DadosIniciais.PopularAsync(banco, senhas, relogio);
    }
}
