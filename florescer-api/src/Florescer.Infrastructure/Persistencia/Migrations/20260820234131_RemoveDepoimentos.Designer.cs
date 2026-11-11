using System;
using Florescer.Infrastructure.Persistencia;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Florescer.Infrastructure.Persistencia.Migrations
{
    [DbContext(typeof(FlorescerDbContext))]
    [Migration("20260820234131_RemoveDepoimentos")]
    partial class RemoveDepoimentos
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.11")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("Florescer.Domain.Entidades.Agendamento", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime?>("CanceladoEm")
                        .HasColumnType("datetime2");

                    b.Property<int>("ClienteId")
                        .HasColumnType("int");

                    b.Property<DateTime?>("ConcluidoEm")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("ConfirmadoEm")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("CriadoEm")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("Fim")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("Inicio")
                        .HasColumnType("datetime2");

                    b.Property<int?>("MotivoCancelamento")
                        .HasColumnType("int");

                    b.Property<string>("Observacoes")
                        .HasMaxLength(300)
                        .HasColumnType("nvarchar(300)");

                    b.Property<DateTime>("PrazoConfirmacao")
                        .HasColumnType("datetime2");

                    b.Property<int>("ProcedimentoId")
                        .HasColumnType("int");

                    b.Property<int>("ProfissionalId")
                        .HasColumnType("int");

                    b.Property<int>("Status")
                        .HasColumnType("int");

                    b.Property<decimal>("ValorPrevisto")
                        .HasPrecision(10, 2)
                        .HasColumnType("decimal(10,2)");

                    b.HasKey("Id");

                    b.HasIndex("ClienteId");

                    b.HasIndex("Inicio");

                    b.HasIndex("ProcedimentoId");

                    b.HasIndex("Status");

                    b.HasIndex("ProfissionalId", "Inicio");

                    b.ToTable("Agendamentos", (string)null);
                });

            modelBuilder.Entity("Florescer.Domain.Entidades.Bloqueio", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("CriadoEm")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("Fim")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("Inicio")
                        .HasColumnType("datetime2");

                    b.Property<string>("Motivo")
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.Property<int?>("ProfissionalId")
                        .HasColumnType("int");

                    b.Property<int>("Tipo")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("ProfissionalId");

                    b.HasIndex("Inicio", "Fim");

                    b.ToTable("Bloqueios", (string)null);
                });

            modelBuilder.Entity("Florescer.Domain.Entidades.Categoria", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<bool>("Ativa")
                        .HasColumnType("bit");

                    b.Property<string>("Cor")
                        .IsRequired()
                        .HasMaxLength(9)
                        .HasColumnType("nvarchar(9)");

                    b.Property<string>("Nome")
                        .IsRequired()
                        .HasMaxLength(60)
                        .HasColumnType("nvarchar(60)");

                    b.Property<int>("Ordem")
                        .HasColumnType("int");

                    b.Property<string>("Slug")
                        .IsRequired()
                        .HasMaxLength(30)
                        .HasColumnType("nvarchar(30)");

                    b.HasKey("Id");

                    b.HasIndex("Slug")
                        .IsUnique();

                    b.ToTable("Categorias", (string)null);
                });

            modelBuilder.Entity("Florescer.Domain.Entidades.CodigoRecuperacao", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("Canal")
                        .HasColumnType("int");

                    b.Property<string>("Codigo")
                        .IsRequired()
                        .HasMaxLength(6)
                        .HasColumnType("nvarchar(6)");

                    b.Property<DateTime>("CriadoEm")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("ExpiraEm")
                        .HasColumnType("datetime2");

                    b.Property<int>("Tentativas")
                        .HasColumnType("int");

                    b.Property<DateTime?>("UsadoEm")
                        .HasColumnType("datetime2");

                    b.Property<int>("UsuarioId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("UsuarioId", "Codigo");

                    b.ToTable("CodigosRecuperacao", (string)null);
                });

            modelBuilder.Entity("Florescer.Domain.Entidades.ConfiguracaoEstabelecimento", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Email")
                        .HasMaxLength(160)
                        .HasColumnType("nvarchar(160)");

                    b.Property<string>("Endereco")
                        .HasMaxLength(300)
                        .HasColumnType("nvarchar(300)");

                    b.Property<string>("FacebookUrl")
                        .HasMaxLength(300)
                        .HasColumnType("nvarchar(300)");

                    b.Property<string>("HorarioFuncionamento")
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.Property<string>("InstagramUrl")
                        .HasMaxLength(300)
                        .HasColumnType("nvarchar(300)");

                    b.Property<string>("MapaUrl")
                        .HasMaxLength(600)
                        .HasColumnType("nvarchar(600)");

                    b.Property<string>("Nome")
                        .IsRequired()
                        .HasMaxLength(120)
                        .HasColumnType("nvarchar(120)");

                    b.Property<string>("Slogan")
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.Property<string>("Telefone")
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)");

                    b.Property<string>("WhatsApp")
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)");

                    b.HasKey("Id");

                    b.ToTable("ConfiguracaoEstabelecimento", (string)null);
                });

            modelBuilder.Entity("Florescer.Domain.Entidades.Faq", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<bool>("Ativa")
                        .HasColumnType("bit");

                    b.Property<int>("Ordem")
                        .HasColumnType("int");

                    b.Property<string>("Pergunta")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.Property<string>("Resposta")
                        .IsRequired()
                        .HasMaxLength(1000)
                        .HasColumnType("nvarchar(1000)");

                    b.HasKey("Id");

                    b.ToTable("Faqs", (string)null);
                });

            modelBuilder.Entity("Florescer.Domain.Entidades.HorarioTrabalho", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("DiaSemana")
                        .HasColumnType("int");

                    b.Property<TimeSpan>("HoraFim")
                        .HasColumnType("time");

                    b.Property<TimeSpan>("HoraInicio")
                        .HasColumnType("time");

                    b.Property<int>("ProfissionalId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("ProfissionalId", "DiaSemana");

                    b.ToTable("HorariosTrabalho", (string)null);
                });

            modelBuilder.Entity("Florescer.Domain.Entidades.LancamentoFinanceiro", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int?>("AgendamentoId")
                        .HasColumnType("int");

                    b.Property<DateTime>("CriadoEm")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("Data")
                        .HasColumnType("datetime2");

                    b.Property<string>("Descricao")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.Property<int?>("RegistradoPorUsuarioId")
                        .HasColumnType("int");

                    b.Property<int>("Tipo")
                        .HasColumnType("int");

                    b.Property<decimal>("Valor")
                        .HasPrecision(10, 2)
                        .HasColumnType("decimal(10,2)");

                    b.HasKey("Id");

                    b.HasIndex("AgendamentoId")
                        .IsUnique()
                        .HasFilter("[AgendamentoId] IS NOT NULL");

                    b.HasIndex("Data");

                    b.HasIndex("RegistradoPorUsuarioId");

                    b.ToTable("LancamentosFinanceiros", (string)null);
                });

            modelBuilder.Entity("Florescer.Domain.Entidades.MidiaLanding", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<bool>("Ativa")
                        .HasColumnType("bit");

                    b.Property<DateTime>("CriadaEm")
                        .HasColumnType("datetime2");

                    b.Property<string>("Legenda")
                        .HasMaxLength(160)
                        .HasColumnType("nvarchar(160)");

                    b.Property<int>("Ordem")
                        .HasColumnType("int");

                    b.Property<int?>("SecaoLandingId")
                        .HasColumnType("int");

                    b.Property<string>("Url")
                        .IsRequired()
                        .HasMaxLength(300)
                        .HasColumnType("nvarchar(300)");

                    b.HasKey("Id");

                    b.HasIndex("SecaoLandingId");

                    b.ToTable("MidiasLanding", (string)null);
                });

            modelBuilder.Entity("Florescer.Domain.Entidades.Notificacao", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int?>("AgendamentoId")
                        .HasColumnType("int");

                    b.Property<string>("Assunto")
                        .IsRequired()
                        .HasMaxLength(160)
                        .HasColumnType("nvarchar(160)");

                    b.Property<int>("Canal")
                        .HasColumnType("int");

                    b.Property<DateTime>("CriadaEm")
                        .HasColumnType("datetime2");

                    b.Property<string>("Destino")
                        .IsRequired()
                        .HasMaxLength(160)
                        .HasColumnType("nvarchar(160)");

                    b.Property<DateTime?>("EnviadaEm")
                        .HasColumnType("datetime2");

                    b.Property<string>("Erro")
                        .HasMaxLength(400)
                        .HasColumnType("nvarchar(400)");

                    b.Property<string>("Mensagem")
                        .IsRequired()
                        .HasMaxLength(1200)
                        .HasColumnType("nvarchar(1200)");

                    b.Property<int>("Status")
                        .HasColumnType("int");

                    b.Property<int>("Tipo")
                        .HasColumnType("int");

                    b.Property<int>("UsuarioId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("UsuarioId");

                    b.HasIndex("AgendamentoId", "Tipo");

                    b.ToTable("Notificacoes", (string)null);
                });

            modelBuilder.Entity("Florescer.Domain.Entidades.Procedimento", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<bool>("Ativo")
                        .HasColumnType("bit");

                    b.Property<int>("CategoriaId")
                        .HasColumnType("int");

                    b.Property<string>("Descricao")
                        .HasMaxLength(400)
                        .HasColumnType("nvarchar(400)");

                    b.Property<int>("DuracaoMinutos")
                        .HasColumnType("int");

                    b.Property<string>("Nome")
                        .IsRequired()
                        .HasMaxLength(120)
                        .HasColumnType("nvarchar(120)");

                    b.Property<decimal>("Preco")
                        .HasPrecision(10, 2)
                        .HasColumnType("decimal(10,2)");

                    b.HasKey("Id");

                    b.HasIndex("CategoriaId");

                    b.ToTable("Procedimentos", (string)null);
                });

            modelBuilder.Entity("Florescer.Domain.Entidades.Profissional", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<bool>("Ativa")
                        .HasColumnType("bit");

                    b.Property<DateTime>("CriadoEm")
                        .HasColumnType("datetime2");

                    b.Property<string>("Email")
                        .HasMaxLength(160)
                        .HasColumnType("nvarchar(160)");

                    b.Property<string>("Especialidade")
                        .IsRequired()
                        .HasMaxLength(60)
                        .HasColumnType("nvarchar(60)");

                    b.Property<string>("FotoUrl")
                        .HasMaxLength(300)
                        .HasColumnType("nvarchar(300)");

                    b.Property<string>("Nome")
                        .IsRequired()
                        .HasMaxLength(120)
                        .HasColumnType("nvarchar(120)");

                    b.Property<string>("Telefone")
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)");

                    b.HasKey("Id");

                    b.ToTable("Profissionais", (string)null);
                });

            modelBuilder.Entity("Florescer.Domain.Entidades.ProfissionalProcedimento", b =>
                {
                    b.Property<int>("ProfissionalId")
                        .HasColumnType("int");

                    b.Property<int>("ProcedimentoId")
                        .HasColumnType("int");

                    b.HasKey("ProfissionalId", "ProcedimentoId");

                    b.HasIndex("ProcedimentoId");

                    b.ToTable("ProfissionaisProcedimentos", (string)null);
                });

            modelBuilder.Entity("Florescer.Domain.Entidades.SecaoLanding", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<bool>("Ativa")
                        .HasColumnType("bit");

                    b.Property<string>("Chave")
                        .IsRequired()
                        .HasMaxLength(40)
                        .HasColumnType("nvarchar(40)");

                    b.Property<string>("ImagemUrl")
                        .HasMaxLength(300)
                        .HasColumnType("nvarchar(300)");

                    b.Property<int>("Ordem")
                        .HasColumnType("int");

                    b.Property<string>("Subtitulo")
                        .HasMaxLength(300)
                        .HasColumnType("nvarchar(300)");

                    b.Property<string>("Texto")
                        .HasMaxLength(2000)
                        .HasColumnType("nvarchar(2000)");

                    b.Property<string>("TextoBotao")
                        .HasMaxLength(60)
                        .HasColumnType("nvarchar(60)");

                    b.Property<string>("Titulo")
                        .IsRequired()
                        .HasMaxLength(160)
                        .HasColumnType("nvarchar(160)");

                    b.HasKey("Id");

                    b.HasIndex("Chave")
                        .IsUnique();

                    b.ToTable("SecoesLanding", (string)null);
                });

            modelBuilder.Entity("Florescer.Domain.Entidades.Usuario", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<bool>("Ativo")
                        .HasColumnType("bit");

                    b.Property<DateTime>("CriadoEm")
                        .HasColumnType("datetime2");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(160)
                        .HasColumnType("nvarchar(160)");

                    b.Property<string>("Nome")
                        .IsRequired()
                        .HasMaxLength(120)
                        .HasColumnType("nvarchar(120)");

                    b.Property<int>("Perfil")
                        .HasColumnType("int");

                    b.Property<string>("SenhaHash")
                        .IsRequired()
                        .HasMaxLength(300)
                        .HasColumnType("nvarchar(300)");

                    b.Property<string>("Telefone")
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)");

                    b.HasKey("Id");

                    b.HasIndex("Email")
                        .IsUnique();

                    b.ToTable("Usuarios", (string)null);
                });

            modelBuilder.Entity("Florescer.Domain.Entidades.Agendamento", b =>
                {
                    b.HasOne("Florescer.Domain.Entidades.Usuario", "Cliente")
                        .WithMany("Agendamentos")
                        .HasForeignKey("ClienteId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("Florescer.Domain.Entidades.Procedimento", "Procedimento")
                        .WithMany("Agendamentos")
                        .HasForeignKey("ProcedimentoId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("Florescer.Domain.Entidades.Profissional", "Profissional")
                        .WithMany("Agendamentos")
                        .HasForeignKey("ProfissionalId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Cliente");

                    b.Navigation("Procedimento");

                    b.Navigation("Profissional");
                });

            modelBuilder.Entity("Florescer.Domain.Entidades.Bloqueio", b =>
                {
                    b.HasOne("Florescer.Domain.Entidades.Profissional", "Profissional")
                        .WithMany("Bloqueios")
                        .HasForeignKey("ProfissionalId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.Navigation("Profissional");
                });

            modelBuilder.Entity("Florescer.Domain.Entidades.CodigoRecuperacao", b =>
                {
                    b.HasOne("Florescer.Domain.Entidades.Usuario", "Usuario")
                        .WithMany("CodigosRecuperacao")
                        .HasForeignKey("UsuarioId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Usuario");
                });

            modelBuilder.Entity("Florescer.Domain.Entidades.HorarioTrabalho", b =>
                {
                    b.HasOne("Florescer.Domain.Entidades.Profissional", "Profissional")
                        .WithMany("HorariosTrabalho")
                        .HasForeignKey("ProfissionalId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Profissional");
                });

            modelBuilder.Entity("Florescer.Domain.Entidades.LancamentoFinanceiro", b =>
                {
                    b.HasOne("Florescer.Domain.Entidades.Agendamento", "Agendamento")
                        .WithOne("Lancamento")
                        .HasForeignKey("Florescer.Domain.Entidades.LancamentoFinanceiro", "AgendamentoId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Florescer.Domain.Entidades.Usuario", "RegistradoPor")
                        .WithMany()
                        .HasForeignKey("RegistradoPorUsuarioId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.Navigation("Agendamento");

                    b.Navigation("RegistradoPor");
                });

            modelBuilder.Entity("Florescer.Domain.Entidades.MidiaLanding", b =>
                {
                    b.HasOne("Florescer.Domain.Entidades.SecaoLanding", "SecaoLanding")
                        .WithMany("Midias")
                        .HasForeignKey("SecaoLandingId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.Navigation("SecaoLanding");
                });

            modelBuilder.Entity("Florescer.Domain.Entidades.Notificacao", b =>
                {
                    b.HasOne("Florescer.Domain.Entidades.Agendamento", "Agendamento")
                        .WithMany("Notificacoes")
                        .HasForeignKey("AgendamentoId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Florescer.Domain.Entidades.Usuario", "Usuario")
                        .WithMany()
                        .HasForeignKey("UsuarioId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Agendamento");

                    b.Navigation("Usuario");
                });

            modelBuilder.Entity("Florescer.Domain.Entidades.Procedimento", b =>
                {
                    b.HasOne("Florescer.Domain.Entidades.Categoria", "Categoria")
                        .WithMany("Procedimentos")
                        .HasForeignKey("CategoriaId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Categoria");
                });

            modelBuilder.Entity("Florescer.Domain.Entidades.ProfissionalProcedimento", b =>
                {
                    b.HasOne("Florescer.Domain.Entidades.Procedimento", "Procedimento")
                        .WithMany("ProfissionaisHabilitadas")
                        .HasForeignKey("ProcedimentoId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Florescer.Domain.Entidades.Profissional", "Profissional")
                        .WithMany("ProcedimentosHabilitados")
                        .HasForeignKey("ProfissionalId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Procedimento");

                    b.Navigation("Profissional");
                });

            modelBuilder.Entity("Florescer.Domain.Entidades.Agendamento", b =>
                {
                    b.Navigation("Lancamento");

                    b.Navigation("Notificacoes");
                });

            modelBuilder.Entity("Florescer.Domain.Entidades.Categoria", b =>
                {
                    b.Navigation("Procedimentos");
                });

            modelBuilder.Entity("Florescer.Domain.Entidades.Procedimento", b =>
                {
                    b.Navigation("Agendamentos");

                    b.Navigation("ProfissionaisHabilitadas");
                });

            modelBuilder.Entity("Florescer.Domain.Entidades.Profissional", b =>
                {
                    b.Navigation("Agendamentos");

                    b.Navigation("Bloqueios");

                    b.Navigation("HorariosTrabalho");

                    b.Navigation("ProcedimentosHabilitados");
                });

            modelBuilder.Entity("Florescer.Domain.Entidades.SecaoLanding", b =>
                {
                    b.Navigation("Midias");
                });

            modelBuilder.Entity("Florescer.Domain.Entidades.Usuario", b =>
                {
                    b.Navigation("Agendamentos");

                    b.Navigation("CodigosRecuperacao");
                });
#pragma warning restore 612, 618
        }
    }
}
