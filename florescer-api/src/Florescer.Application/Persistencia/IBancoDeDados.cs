using Florescer.Domain.Entidades;
using Microsoft.EntityFrameworkCore;

namespace Florescer.Application.Persistencia;

public interface IBancoDeDados
{
    DbSet<Usuario> Usuarios { get; }
    DbSet<Categoria> Categorias { get; }
    DbSet<Procedimento> Procedimentos { get; }
    DbSet<Profissional> Profissionais { get; }
    DbSet<ProfissionalProcedimento> ProfissionaisProcedimentos { get; }
    DbSet<HorarioTrabalho> HorariosTrabalho { get; }
    DbSet<Bloqueio> Bloqueios { get; }
    DbSet<Agendamento> Agendamentos { get; }
    DbSet<LancamentoFinanceiro> Lancamentos { get; }
    DbSet<Notificacao> Notificacoes { get; }
    DbSet<CodigoRecuperacao> CodigosRecuperacao { get; }
    DbSet<SecaoLanding> SecoesLanding { get; }
    DbSet<MidiaLanding> MidiasLanding { get; }
    DbSet<Faq> Faqs { get; }
    DbSet<ConfiguracaoEstabelecimento> ConfiguracoesEstabelecimento { get; }

    Task<int> SalvarAsync(CancellationToken cancelamento = default);
}
