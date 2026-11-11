using Florescer.Application.Persistencia;
using Florescer.Domain.Entidades;
using Microsoft.EntityFrameworkCore;

namespace Florescer.Infrastructure.Persistencia;

public class FlorescerDbContext : DbContext, IBancoDeDados
{
    public FlorescerDbContext(DbContextOptions<FlorescerDbContext> opcoes) : base(opcoes)
    {
    }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Procedimento> Procedimentos => Set<Procedimento>();
    public DbSet<Profissional> Profissionais => Set<Profissional>();
    public DbSet<ProfissionalProcedimento> ProfissionaisProcedimentos => Set<ProfissionalProcedimento>();
    public DbSet<HorarioTrabalho> HorariosTrabalho => Set<HorarioTrabalho>();
    public DbSet<Bloqueio> Bloqueios => Set<Bloqueio>();
    public DbSet<Agendamento> Agendamentos => Set<Agendamento>();
    public DbSet<LancamentoFinanceiro> Lancamentos => Set<LancamentoFinanceiro>();
    public DbSet<Notificacao> Notificacoes => Set<Notificacao>();
    public DbSet<CodigoRecuperacao> CodigosRecuperacao => Set<CodigoRecuperacao>();
    public DbSet<SecaoLanding> SecoesLanding => Set<SecaoLanding>();
    public DbSet<MidiaLanding> MidiasLanding => Set<MidiaLanding>();
    public DbSet<Faq> Faqs => Set<Faq>();
    public DbSet<ConfiguracaoEstabelecimento> ConfiguracoesEstabelecimento => Set<ConfiguracaoEstabelecimento>();

    public Task<int> SalvarAsync(CancellationToken cancelamento = default)
        => SaveChangesAsync(cancelamento);

    protected override void OnModelCreating(ModelBuilder modelo)
    {
        modelo.Entity<Usuario>(e =>
        {
            e.ToTable("Usuarios");
            e.HasKey(u => u.Id);
            e.Property(u => u.Nome).HasMaxLength(120).IsRequired();
            e.Property(u => u.Email).HasMaxLength(160).IsRequired();
            e.Property(u => u.Telefone).HasMaxLength(20);
            e.Property(u => u.SenhaHash).HasMaxLength(300).IsRequired();
            e.Property(u => u.Perfil).HasConversion<int>();

            e.HasIndex(u => u.Email).IsUnique();
        });

        modelo.Entity<Categoria>(e =>
        {
            e.ToTable("Categorias");
            e.HasKey(c => c.Id);
            e.Property(c => c.Nome).HasMaxLength(60).IsRequired();
            e.Property(c => c.Slug).HasMaxLength(30).IsRequired();
            e.Property(c => c.Cor).HasMaxLength(9);
            e.Property(c => c.ImagemUrl).HasMaxLength(300);
            e.HasIndex(c => c.Slug).IsUnique();
        });

        modelo.Entity<Procedimento>(e =>
        {
            e.ToTable("Procedimentos");
            e.HasKey(p => p.Id);
            e.Property(p => p.Nome).HasMaxLength(120).IsRequired();
            e.Property(p => p.Descricao).HasMaxLength(400);
            e.Property(p => p.Preco).HasPrecision(10, 2);

            e.HasOne(p => p.Categoria)
             .WithMany(c => c.Procedimentos)
             .HasForeignKey(p => p.CategoriaId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        modelo.Entity<Profissional>(e =>
        {
            e.ToTable("Profissionais");
            e.HasKey(p => p.Id);
            e.Property(p => p.Nome).HasMaxLength(120).IsRequired();
            e.Property(p => p.Especialidade).HasMaxLength(60).IsRequired();
            e.Property(p => p.Email).HasMaxLength(160);
            e.Property(p => p.Telefone).HasMaxLength(20);
            e.Property(p => p.FotoUrl).HasMaxLength(300);
        });

        modelo.Entity<ProfissionalProcedimento>(e =>
        {
            e.ToTable("ProfissionaisProcedimentos");

            e.HasKey(pp => new { pp.ProfissionalId, pp.ProcedimentoId });

            e.HasOne(pp => pp.Profissional)
             .WithMany(p => p.ProcedimentosHabilitados)
             .HasForeignKey(pp => pp.ProfissionalId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(pp => pp.Procedimento)
             .WithMany(p => p.ProfissionaisHabilitadas)
             .HasForeignKey(pp => pp.ProcedimentoId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        modelo.Entity<HorarioTrabalho>(e =>
        {
            e.ToTable("HorariosTrabalho");
            e.HasKey(h => h.Id);
            e.Property(h => h.DiaSemana).HasConversion<int>();

            e.HasOne(h => h.Profissional)
             .WithMany(p => p.HorariosTrabalho)
             .HasForeignKey(h => h.ProfissionalId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasIndex(h => new { h.ProfissionalId, h.DiaSemana });
        });

        modelo.Entity<Bloqueio>(e =>
        {
            e.ToTable("Bloqueios");
            e.HasKey(b => b.Id);
            e.Property(b => b.Motivo).HasMaxLength(200);
            e.Property(b => b.Tipo).HasConversion<int>();

            e.HasOne(b => b.Profissional)
             .WithMany(p => p.Bloqueios)
             .HasForeignKey(b => b.ProfissionalId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasIndex(b => new { b.Inicio, b.Fim });
        });

        modelo.Entity<Agendamento>(e =>
        {
            e.ToTable("Agendamentos");
            e.HasKey(a => a.Id);
            e.Property(a => a.Status).HasConversion<int>();
            e.Property(a => a.MotivoCancelamento).HasConversion<int>();
            e.Property(a => a.ValorPrevisto).HasPrecision(10, 2);
            e.Property(a => a.Observacoes).HasMaxLength(300);

            e.Ignore(a => a.OcupaAgenda);

            e.HasOne(a => a.Cliente)
             .WithMany(u => u.Agendamentos)
             .HasForeignKey(a => a.ClienteId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(a => a.Profissional)
             .WithMany(p => p.Agendamentos)
             .HasForeignKey(a => a.ProfissionalId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(a => a.Procedimento)
             .WithMany(p => p.Agendamentos)
             .HasForeignKey(a => a.ProcedimentoId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(a => new { a.ProfissionalId, a.Inicio });
            e.HasIndex(a => a.Inicio);
            e.HasIndex(a => a.Status);
        });

        modelo.Entity<LancamentoFinanceiro>(e =>
        {
            e.ToTable("LancamentosFinanceiros");
            e.HasKey(l => l.Id);
            e.Property(l => l.Tipo).HasConversion<int>();
            e.Property(l => l.Valor).HasPrecision(10, 2);
            e.Property(l => l.Descricao).HasMaxLength(200).IsRequired();

            e.HasOne(l => l.Agendamento)
             .WithOne(a => a.Lancamento)
             .HasForeignKey<LancamentoFinanceiro>(l => l.AgendamentoId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(l => l.RegistradoPor)
             .WithMany()
             .HasForeignKey(l => l.RegistradoPorUsuarioId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(l => l.Data);
        });

        modelo.Entity<Notificacao>(e =>
        {
            e.ToTable("Notificacoes");
            e.HasKey(n => n.Id);
            e.Property(n => n.Canal).HasConversion<int>();
            e.Property(n => n.Tipo).HasConversion<int>();
            e.Property(n => n.Status).HasConversion<int>();
            e.Property(n => n.Destino).HasMaxLength(160).IsRequired();
            e.Property(n => n.Assunto).HasMaxLength(160).IsRequired();
            e.Property(n => n.Mensagem).HasMaxLength(1200).IsRequired();
            e.Property(n => n.Erro).HasMaxLength(400);

            e.HasOne(n => n.Usuario)
             .WithMany()
             .HasForeignKey(n => n.UsuarioId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(n => n.Agendamento)
             .WithMany(a => a.Notificacoes)
             .HasForeignKey(n => n.AgendamentoId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasIndex(n => new { n.AgendamentoId, n.Tipo });
        });

        modelo.Entity<CodigoRecuperacao>(e =>
        {
            e.ToTable("CodigosRecuperacao");
            e.HasKey(c => c.Id);
            e.Property(c => c.CodigoHash).HasMaxLength(200).IsRequired();
            e.Property(c => c.Canal).HasConversion<int>();

            e.HasOne(c => c.Usuario)
             .WithMany(u => u.CodigosRecuperacao)
             .HasForeignKey(c => c.UsuarioId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasIndex(c => new { c.UsuarioId, c.UsadoEm });
        });

        modelo.Entity<SecaoLanding>(e =>
        {
            e.ToTable("SecoesLanding");
            e.HasKey(s => s.Id);
            e.Property(s => s.Chave).HasMaxLength(40).IsRequired();
            e.Property(s => s.Titulo).HasMaxLength(160).IsRequired();
            e.Property(s => s.Subtitulo).HasMaxLength(300);
            e.Property(s => s.Texto).HasMaxLength(2000);
            e.Property(s => s.TextoBotao).HasMaxLength(60);
            e.Property(s => s.ImagemUrl).HasMaxLength(300);
            e.HasIndex(s => s.Chave).IsUnique();
        });

        modelo.Entity<MidiaLanding>(e =>
        {
            e.ToTable("MidiasLanding");
            e.HasKey(m => m.Id);
            e.Property(m => m.Url).HasMaxLength(300).IsRequired();
            e.Property(m => m.Legenda).HasMaxLength(160);

            e.HasOne(m => m.SecaoLanding)
             .WithMany(s => s.Midias)
             .HasForeignKey(m => m.SecaoLandingId)
             .OnDelete(DeleteBehavior.SetNull);
        });

        modelo.Entity<Faq>(e =>
        {
            e.ToTable("Faqs");
            e.HasKey(f => f.Id);
            e.Property(f => f.Pergunta).HasMaxLength(200).IsRequired();
            e.Property(f => f.Resposta).HasMaxLength(1000).IsRequired();
        });

        modelo.Entity<ConfiguracaoEstabelecimento>(e =>
        {
            e.ToTable("ConfiguracaoEstabelecimento");
            e.HasKey(c => c.Id);
            e.Property(c => c.Nome).HasMaxLength(120).IsRequired();
            e.Property(c => c.Slogan).HasMaxLength(200);
            e.Property(c => c.Telefone).HasMaxLength(20);
            e.Property(c => c.WhatsApp).HasMaxLength(20);
            e.Property(c => c.Email).HasMaxLength(160);
            e.Property(c => c.Endereco).HasMaxLength(300);
            e.Property(c => c.HorarioFuncionamento).HasMaxLength(200);
            e.Property(c => c.InstagramUrl).HasMaxLength(300);
            e.Property(c => c.FacebookUrl).HasMaxLength(300);
            e.Property(c => c.MapaUrl).HasMaxLength(600);
        });

        base.OnModelCreating(modelo);
    }
}
