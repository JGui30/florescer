using Florescer.Application.Contratos;
using Florescer.Domain.Entidades;
using Florescer.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Florescer.Infrastructure.Persistencia;

public static class DadosIniciais
{
    public const string EmailAdminPadrao = "admin@florescer.com.br";
    public const string SenhaAdminPadrao = "Florescer@2025";

    public static async Task PopularAsync(
        FlorescerDbContext banco,
        IServicoDeSenha servicoDeSenha,
        IRelogio relogio)
    {
        await CriarAdministradorAsync(banco, servicoDeSenha, relogio);
        await CriarCatalogoAsync(banco);
        await CriarEquipeAsync(banco, relogio);
        await CriarConteudoDaLandingAsync(banco);
        await CriarHistoricoDeDemonstracaoAsync(banco, servicoDeSenha, relogio);
    }

    private static async Task CriarAdministradorAsync(
        FlorescerDbContext banco, IServicoDeSenha senhas, IRelogio relogio)
    {
        if (await banco.Usuarios.AnyAsync(u => u.Perfil == PerfilUsuario.Administrador))
            return;

        banco.Usuarios.Add(new Usuario
        {
            Nome = "Administrador",
            Email = EmailAdminPadrao,
            Telefone = null,
            SenhaHash = senhas.GerarHash(SenhaAdminPadrao),
            Perfil = PerfilUsuario.Administrador,
            Ativo = true,
            CriadoEm = relogio.Agora
        });

        await banco.SalvarAsync();
    }

    private static async Task CriarCatalogoAsync(FlorescerDbContext banco)
    {
        if (await banco.Categorias.AnyAsync())
            return;

        var unhas = new Categoria { Nome = "Unhas", Slug = "unhas", Cor = "#E8306E", Ordem = 1 };
        var cabelo = new Categoria { Nome = "Cabelo", Slug = "cabelo", Cor = "#B5540E", Ordem = 2 };
        var spa = new Categoria { Nome = "Spa", Slug = "spa", Cor = "#128F5D", Ordem = 3 };
        var estetica = new Categoria { Nome = "Estética", Slug = "estetica", Cor = "#E8A93C", Ordem = 4 };

        banco.Categorias.AddRange(unhas, cabelo, spa, estetica);

        AdicionarProcedimentos(banco, unhas,
            ("Manicure", 40, 35m),
            ("Pedicure", 50, 40m),
            ("Esmaltação em gel", 60, 70m),
            ("Alongamento de unhas", 100, 120m),
            ("Manutenção de alongamento", 70, 80m),
            ("Nail art", 45, 50m),
            ("Blindagem", 55, 65m));

        AdicionarProcedimentos(banco, cabelo,
            ("Escova", 50, 60m),
            ("Corte feminino", 60, 80m),
            ("Corte masculino", 30, 45m),
            ("Hidratação", 45, 70m),
            ("Nutrição", 45, 75m),
            ("Reconstrução", 60, 90m),
            ("Progressiva", 150, 250m),
            ("Botox capilar", 120, 200m),
            ("Coloração", 90, 150m),
            ("Mechas", 150, 280m),
            ("Tonalização", 60, 110m));

        AdicionarProcedimentos(banco, spa,
            ("Massagem relaxante", 60, 120m),
            ("Drenagem linfática", 60, 130m),
            ("Spa dos pés", 50, 90m),
            ("Spa facial", 60, 140m),
            ("Limpeza de pele", 70, 150m));

        AdicionarProcedimentos(banco, estetica,
            ("Aplicação de Botox", 40, 800m),
            ("Preenchimento facial", 50, 1200m),
            ("Bioestimulador de colágeno", 50, 1400m),
            ("Skinbooster", 40, 900m),
            ("Lifting facial", 60, 1600m),
            ("Harmonização facial", 70, 2000m),
            ("Microagulhamento", 60, 350m),
            ("Peeling", 40, 250m),
            ("Limpeza de pele avançada", 70, 220m),
            ("Tratamentos faciais diversos", 50, 280m));

        await banco.SalvarAsync();
    }

    private static void AdicionarProcedimentos(
        FlorescerDbContext banco,
        Categoria categoria,
        params (string Nome, int Duracao, decimal Preco)[] itens)
    {
        foreach (var item in itens)
        {
            banco.Procedimentos.Add(new Procedimento
            {
                Categoria = categoria,
                Nome = item.Nome,
                DuracaoMinutos = item.Duracao,
                Preco = item.Preco,
                Ativo = true
            });
        }
    }

    private static async Task CriarEquipeAsync(FlorescerDbContext banco, IRelogio relogio)
    {
        if (await banco.Profissionais.AnyAsync())
            return;

        var equipe = new[]
        {
            ("Ana Paula",      "Manicure",       "unhas",    new[] { 1, 1, 1, 1, 1, 0, 0 }),
            ("Bianca Souza",   "Manicure",       "unhas",    new[] { 0, 1, 1, 1, 1, 1, 0 }),
            ("Carla Menezes",  "Cabeleireira",   "cabelo",   new[] { 1, 1, 1, 1, 1, 0, 0 }),
            ("Débora Lima",    "Cabeleireira",   "cabelo",   new[] { 0, 1, 1, 1, 1, 1, 1 }),
            ("Elaine Rocha",   "Massoterapeuta", "spa",      new[] { 1, 0, 1, 0, 1, 0, 1 }),
            ("Fernanda Alves", "Esteticista",    "estetica", new[] { 1, 1, 0, 1, 1, 0, 0 }),
            ("Giovana Reis",   "Esteticista",    "estetica", new[] { 0, 1, 1, 0, 1, 1, 0 })
        };

        var procedimentosPorCategoria = await banco.Procedimentos
            .Include(p => p.Categoria)
            .ToListAsync();

        foreach (var (nome, especialidade, slugCategoria, dias) in equipe)
        {
            var profissional = new Profissional
            {
                Nome = nome,
                Especialidade = especialidade,
                Email = GerarEmail(nome),
                Telefone = "(11) 97000-0000",
                Ativa = true,
                CriadoEm = relogio.Agora
            };

            for (var posicao = 0; posicao < dias.Length; posicao++)
            {
                if (dias[posicao] == 0) continue;

                profissional.HorariosTrabalho.Add(new HorarioTrabalho
                {
                    DiaSemana = ConverterParaDiaDaSemana(posicao),
                    HoraInicio = new TimeSpan(9, 0, 0),
                    HoraFim = new TimeSpan(18, 0, 0)
                });
            }

            foreach (var procedimento in procedimentosPorCategoria.Where(p => p.Categoria.Slug == slugCategoria))
            {
                profissional.ProcedimentosHabilitados.Add(new ProfissionalProcedimento
                {
                    Procedimento = procedimento
                });
            }

            banco.Profissionais.Add(profissional);
        }

        await banco.SalvarAsync();
    }

    private static DayOfWeek ConverterParaDiaDaSemana(int posicaoNoFront)
    {
        return posicaoNoFront switch
        {
            0 => DayOfWeek.Monday,
            1 => DayOfWeek.Tuesday,
            2 => DayOfWeek.Wednesday,
            3 => DayOfWeek.Thursday,
            4 => DayOfWeek.Friday,
            5 => DayOfWeek.Saturday,
            _ => DayOfWeek.Sunday
        };
    }

    private static string GerarEmail(string nome)
    {
        var primeiro = nome.Split(' ')[0]
            .ToLowerInvariant()
            .Replace("é", "e").Replace("ê", "e")
            .Replace("á", "a").Replace("ã", "a")
            .Replace("í", "i").Replace("ó", "o").Replace("ú", "u");

        return $"{primeiro}@florescer.com.br";
    }

    private static async Task CriarConteudoDaLandingAsync(FlorescerDbContext banco)
    {
        if (!await banco.ConfiguracoesEstabelecimento.AnyAsync())
        {
            banco.ConfiguracoesEstabelecimento.Add(new ConfiguracaoEstabelecimento
            {
                Nome = "Florescer",
                Slogan = "Centro de beleza e estética",
                Telefone = "(11) 3000-0000",
                WhatsApp = "5511970000000",
                Email = "contato@florescer.com.br",
                Endereco = "Rua das Acácias, 210 — Vila Mariana, São Paulo/SP",
                HorarioFuncionamento = "Segunda a sexta, 9h às 18h · Sábado, 9h às 16h",
                InstagramUrl = "https://instagram.com/florescer",
                FacebookUrl = "https://facebook.com/florescer"
            });
        }

        if (!await banco.SecoesLanding.AnyAsync())
        {
            banco.SecoesLanding.AddRange(
                new SecaoLanding
                {
                    Chave = "hero",
                    Titulo = "Seu momento de cuidado começa aqui",
                    Subtitulo = "Unhas, cabelo, spa e estética avançada em um só lugar, com profissionais especializadas e agendamento online em menos de um minuto.",
                    TextoBotao = "Agendar agora",
                    Ordem = 1,
                    Ativa = true
                },
                new SecaoLanding
                {
                    Chave = "diferenciais",
                    Titulo = "Por que escolher o Florescer",
                    Subtitulo = "Cuidado que se percebe do primeiro contato ao resultado final.",
                    Ordem = 2,
                    Ativa = true
                },
                new SecaoLanding
                {
                    Chave = "servicos",
                    Titulo = "Nossos serviços",
                    Subtitulo = "Preços e durações sempre atualizados, direto da nossa agenda.",
                    Ordem = 3,
                    Ativa = true
                },
                new SecaoLanding
                {
                    Chave = "equipe",
                    Titulo = "Quem cuida de você",
                    Subtitulo = "Uma equipe especializada em cada área, para que você seja atendida por quem realmente domina o procedimento.",
                    Ordem = 4,
                    Ativa = true
                },
                new SecaoLanding
                {
                    Chave = "galeria",
                    Titulo = "Resultados que falam por si",
                    Subtitulo = "Trabalhos realizados por nossa equipe.",
                    Ordem = 5,
                    Ativa = true
                },
                new SecaoLanding
                {
                    Chave = "faq",
                    Titulo = "Perguntas frequentes",
                    Subtitulo = "Tudo o que você precisa saber antes de agendar.",
                    Ordem = 7,
                    Ativa = true
                },
                new SecaoLanding
                {
                    Chave = "cta",
                    Titulo = "Reserve seu horário agora",
                    Subtitulo = "A agenda desta semana está enchendo rápido. Escolha o melhor dia para você e garanta seu atendimento.",
                    TextoBotao = "Criar minha conta",
                    Ordem = 8,
                    Ativa = true
                });
        }

        if (!await banco.Faqs.AnyAsync())
        {
            banco.Faqs.AddRange(
                new Faq { Ordem = 1, Pergunta = "Como faço para agendar?", Resposta = "Crie sua conta gratuitamente, escolha o procedimento, a profissional e o horário disponível. Todo o processo leva menos de um minuto e você recebe a confirmação na hora." },
                new Faq { Ordem = 2, Pergunta = "Preciso confirmar meu agendamento?", Resposta = "Sim. Você tem até 24 horas antes do atendimento para confirmar pelo aplicativo. Se o agendamento for feito com menos de 24 horas de antecedência, o prazo passa a ser até 2 horas antes. Sem a confirmação, o horário é liberado automaticamente para outra cliente." },
                new Faq { Ordem = 3, Pergunta = "Posso cancelar ou remarcar?", Resposta = "Pode, sem custo, até 2 horas antes do horário marcado, direto pelo aplicativo. Depois desse prazo é só falar com a recepção pelo WhatsApp." },
                new Faq { Ordem = 4, Pergunta = "Quais formas de pagamento vocês aceitam?", Resposta = "Aceitamos dinheiro, PIX e cartões de débito e crédito. O pagamento é feito presencialmente, no dia do atendimento." },
                new Faq { Ordem = 5, Pergunta = "Vocês atendem sem agendamento?", Resposta = "Damos prioridade a quem agenda, para garantir que cada cliente tenha o tempo necessário. Se houver horário livre no dia, o atendimento por ordem de chegada é possível." },
                new Faq { Ordem = 6, Pergunta = "Os procedimentos estéticos são seguros?", Resposta = "Todos são realizados por profissionais habilitadas, com produtos registrados na ANVISA e protocolos rigorosos de higienização. Antes de qualquer procedimento é feita uma avaliação individual." });
        }

        await banco.SalvarAsync();
    }

    private static async Task CriarHistoricoDeDemonstracaoAsync(
        FlorescerDbContext banco, IServicoDeSenha senhas, IRelogio relogio)
    {
        if (await banco.Agendamentos.AnyAsync())
            return;

        var nomesDeClientes = new[]
        {
            "Renata Alves", "Juliana Ferreira", "Patrícia Gomes", "Camila Nunes",
            "Beatriz Lima", "Larissa Souza", "Vanessa Rocha", "Isabela Martins",
            "Fernanda Castro", "Gabriela Dias", "Mariana Prado", "Letícia Barros"
        };

        var clientes = new List<Usuario>();
        foreach (var nome in nomesDeClientes)
        {
            var cliente = new Usuario
            {
                Nome = nome,
                Email = $"{GerarEmail(nome).Split('@')[0]}.cliente@email.com",
                Telefone = "(11) 96000-0000",
                SenhaHash = senhas.GerarHash("Cliente@123"),
                Perfil = PerfilUsuario.Cliente,
                Ativo = true,
                CriadoEm = relogio.Agora.AddDays(-120)
            };
            clientes.Add(cliente);
            banco.Usuarios.Add(cliente);
        }

        await banco.SalvarAsync();

        var profissionais = await banco.Profissionais
            .Include(p => p.HorariosTrabalho)
            .Include(p => p.ProcedimentosHabilitados)
                .ThenInclude(pp => pp.Procedimento)
            .ToListAsync();

        var sorteio = new Random(2025);
        var agendamentos = new List<Agendamento>();
        var lancamentos = new List<LancamentoFinanceiro>();

        for (var deslocamento = -90; deslocamento <= 14; deslocamento++)
        {
            var dia = relogio.Hoje.AddDays(deslocamento);

            var atendimentosNoDia = dia.DayOfWeek == DayOfWeek.Sunday
                ? sorteio.Next(0, 3)
                : sorteio.Next(4, 11);

            var horariosUsados = new Dictionary<int, List<(DateTime Inicio, DateTime Fim)>>();

            for (var i = 0; i < atendimentosNoDia; i++)
            {
                var profissional = profissionais[sorteio.Next(profissionais.Count)];

                var jornada = profissional.HorariosTrabalho.FirstOrDefault(h => h.DiaSemana == dia.DayOfWeek);
                if (jornada is null) continue;

                var habilitados = profissional.ProcedimentosHabilitados.ToList();
                if (habilitados.Count == 0) continue;

                var procedimento = habilitados[sorteio.Next(habilitados.Count)].Procedimento;

                var blocos = (int)((jornada.HoraFim - jornada.HoraInicio).TotalMinutes / 30);
                var inicio = dia.Add(jornada.HoraInicio).AddMinutes(30 * sorteio.Next(blocos));
                var fim = inicio.AddMinutes(procedimento.DuracaoMinutos);

                if (fim.TimeOfDay > jornada.HoraFim) continue;

                if (!horariosUsados.TryGetValue(profissional.Id, out var ocupados))
                {
                    ocupados = new List<(DateTime, DateTime)>();
                    horariosUsados[profissional.Id] = ocupados;
                }
                if (ocupados.Any(o => inicio < o.Fim && o.Inicio < fim)) continue;
                ocupados.Add((inicio, fim));

                var cliente = clientes[sorteio.Next(clientes.Count)];

                var agendamento = new Agendamento
                {
                    ClienteId = cliente.Id,
                    ProfissionalId = profissional.Id,
                    ProcedimentoId = procedimento.Id,
                    Inicio = inicio,
                    Fim = fim,
                    ValorPrevisto = procedimento.Preco,
                    PrazoConfirmacao = inicio.AddHours(-24),
                    CriadoEm = inicio.AddDays(-sorteio.Next(2, 12))
                };

                if (deslocamento < 0)
                {
                    var sorte = sorteio.Next(100);
                    if (sorte < 82)
                    {
                        agendamento.Status = StatusAgendamento.Concluido;
                        agendamento.ConfirmadoEm = inicio.AddDays(-1);
                        agendamento.ConcluidoEm = fim;

                        lancamentos.Add(new LancamentoFinanceiro
                        {
                            Agendamento = agendamento,
                            Tipo = TipoLancamento.Receita,
                            Valor = procedimento.Preco,
                            Data = inicio.Date,
                            Descricao = $"{procedimento.Nome} — {cliente.Nome}",
                            CriadoEm = fim
                        });
                    }
                    else if (sorte < 92)
                    {
                        agendamento.Status = StatusAgendamento.Cancelado;
                        agendamento.MotivoCancelamento = MotivoCancelamento.FaltaDeConfirmacao;
                        agendamento.CanceladoEm = agendamento.PrazoConfirmacao;
                    }
                    else
                    {
                        agendamento.Status = StatusAgendamento.Cancelado;
                        agendamento.MotivoCancelamento = MotivoCancelamento.CanceladoPeloCliente;
                        agendamento.CanceladoEm = inicio.AddDays(-1);
                    }
                }
                else
                {
                    if (sorteio.Next(100) < 55)
                    {
                        agendamento.Status = StatusAgendamento.Confirmado;
                        agendamento.ConfirmadoEm = relogio.Agora.AddHours(-sorteio.Next(1, 48));
                    }
                    else
                    {
                        agendamento.Status = StatusAgendamento.Agendado;
                    }
                }

                agendamentos.Add(agendamento);
            }
        }

        banco.Agendamentos.AddRange(agendamentos);
        banco.Lancamentos.AddRange(lancamentos);
        await banco.SalvarAsync();
    }
}
