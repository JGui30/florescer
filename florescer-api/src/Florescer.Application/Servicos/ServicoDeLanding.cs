using Florescer.Application.Contratos;
using Florescer.Application.Dtos;
using Florescer.Application.Persistencia;
using Florescer.Domain.Entidades;
using Florescer.Domain.Enums;
using Florescer.Domain.Regras;
using Microsoft.EntityFrameworkCore;

namespace Florescer.Application.Servicos;

public class ServicoDeLanding
{
    private readonly IBancoDeDados _banco;
    private readonly IRelogio _relogio;
    private readonly ServicoDeCatalogo _catalogo;

    public ServicoDeLanding(IBancoDeDados banco, IRelogio relogio, ServicoDeCatalogo catalogo)
    {
        _banco = banco;
        _relogio = relogio;
        _catalogo = catalogo;
    }

    public async Task<LandingResposta> ObterAsync(bool incluirInativos = false, CancellationToken ct = default)
    {
        var estabelecimento = await ObterEstabelecimentoAsync(ct);

        var secoes = await _banco.SecoesLanding
            .Include(s => s.Midias)
            .Where(s => incluirInativos || s.Ativa)
            .OrderBy(s => s.Ordem)
            .ToListAsync(ct);

        var equipe = await _banco.Profissionais
            .Where(p => p.Ativa)
            .OrderBy(p => p.Nome)
            .Select(p => new ProfissionalVitrineResposta(p.Id, p.Nome, p.Especialidade, p.FotoUrl))
            .ToListAsync(ct);

        var faqs = await _banco.Faqs
            .Where(f => incluirInativos || f.Ativa)
            .OrderBy(f => f.Ordem)
            .ToListAsync(ct);

        return new LandingResposta(
            estabelecimento,
            secoes.Select(Converter).ToList(),
            await _catalogo.ListarCategoriasAsync(true, ct),
            equipe,
            faqs.Select(Converter).ToList(),
            await CalcularNumerosAsync(ct));
    }

    private async Task<NumerosDaVitrineResposta> CalcularNumerosAsync(CancellationToken ct)
    {
        var realizados = await _banco.Agendamentos
            .CountAsync(a => a.Status == StatusAgendamento.Concluido, ct);

        var clientes = await _banco.Agendamentos
            .Where(a => a.Status == StatusAgendamento.Concluido)
            .Select(a => a.ClienteId)
            .Distinct()
            .CountAsync(ct);

        var profissionais = await _banco.Profissionais.CountAsync(p => p.Ativa, ct);
        var procedimentos = await _banco.Procedimentos.CountAsync(p => p.Ativo, ct);
        var categorias = await _banco.Categorias.CountAsync(c => c.Ativa, ct);

        var inicio = _relogio.Hoje;
        var fim = inicio.AddDays(7);

        var jornadas = await _banco.HorariosTrabalho
            .Where(h => h.Profissional.Ativa)
            .Select(h => new { h.DiaSemana, h.HoraInicio, h.HoraFim })
            .ToListAsync(ct);

        var minutosDisponiveis = 0d;
        for (var dia = inicio; dia < fim; dia = dia.AddDays(1))
        {
            minutosDisponiveis += jornadas
                .Where(j => j.DiaSemana == dia.DayOfWeek)
                .Sum(j => (j.HoraFim - j.HoraInicio).TotalMinutes);
        }

        var atendimentos = await _banco.Agendamentos
            .Where(a => a.Inicio >= inicio && a.Inicio < fim && a.Status != StatusAgendamento.Cancelado)
            .Select(a => new { a.Inicio, a.Fim })
            .ToListAsync(ct);

        var minutosOcupados = atendimentos.Sum(a => (a.Fim - a.Inicio).TotalMinutes);

        var ocupacao = minutosDisponiveis <= 0
            ? 0
            : Math.Round((decimal)(minutosOcupados / minutosDisponiveis) * 100m, 0);

        var horariosLivres = (int)Math.Max(0, (minutosDisponiveis - minutosOcupados) / 60);

        return new NumerosDaVitrineResposta(
            realizados, clientes, profissionais, procedimentos, categorias, horariosLivres, ocupacao);
    }

    public async Task<EstabelecimentoResposta> ObterEstabelecimentoAsync(CancellationToken ct = default)
    {
        var config = await _banco.ConfiguracoesEstabelecimento.FirstOrDefaultAsync(ct);

        if (config is null)
        {
            config = new ConfiguracaoEstabelecimento();
            _banco.ConfiguracoesEstabelecimento.Add(config);
            await _banco.SalvarAsync(ct);
        }

        return new EstabelecimentoResposta(
            config.Nome, config.Slogan, config.Telefone, config.WhatsApp, config.Email,
            config.Endereco, config.HorarioFuncionamento, config.InstagramUrl,
            config.FacebookUrl, config.MapaUrl);
    }

    public async Task<EstabelecimentoResposta> AtualizarEstabelecimentoAsync(
        EstabelecimentoRequisicao dados, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(dados.Nome))
            throw new RegraDeNegocioException("Informe o nome do estabelecimento.");

        var config = await _banco.ConfiguracoesEstabelecimento.FirstOrDefaultAsync(ct);

        if (config is null)
        {
            config = new ConfiguracaoEstabelecimento();
            _banco.ConfiguracoesEstabelecimento.Add(config);
        }

        config.Nome = dados.Nome.Trim();
        config.Slogan = dados.Slogan?.Trim();
        config.Telefone = dados.Telefone?.Trim();
        config.WhatsApp = dados.WhatsApp?.Trim();
        config.Email = dados.Email?.Trim();
        config.Endereco = dados.Endereco?.Trim();
        config.HorarioFuncionamento = dados.HorarioFuncionamento?.Trim();
        config.InstagramUrl = dados.InstagramUrl?.Trim();
        config.FacebookUrl = dados.FacebookUrl?.Trim();
        config.MapaUrl = dados.MapaUrl?.Trim();

        await _banco.SalvarAsync(ct);

        return await ObterEstabelecimentoAsync(ct);
    }

    public async Task<SecaoLandingResposta> AtualizarSecaoAsync(
        int id, SecaoLandingRequisicao dados, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(dados.Titulo))
            throw new RegraDeNegocioException("Informe o título da seção.");

        var secao = await _banco.SecoesLanding
            .Include(s => s.Midias)
            .FirstOrDefaultAsync(s => s.Id == id, ct)
            ?? throw new RegraDeNegocioException("Seção não encontrada.");

        secao.Titulo = dados.Titulo.Trim();
        secao.Subtitulo = dados.Subtitulo?.Trim();
        secao.Texto = dados.Texto?.Trim();
        secao.TextoBotao = dados.TextoBotao?.Trim();
        secao.ImagemUrl = dados.ImagemUrl?.Trim();
        secao.Ativa = dados.Ativa;

        await _banco.SalvarAsync(ct);
        return Converter(secao);
    }

    public async Task<MidiaResposta> AdicionarMidiaAsync(
        int? secaoId, string url, string? legenda, CancellationToken ct = default)
    {
        var ordem = await _banco.MidiasLanding
            .Where(m => m.SecaoLandingId == secaoId)
            .CountAsync(ct);

        var midia = new MidiaLanding
        {
            SecaoLandingId = secaoId,
            Url = url,
            Legenda = legenda?.Trim(),
            Ordem = ordem,
            Ativa = true,
            CriadaEm = _relogio.Agora
        };

        _banco.MidiasLanding.Add(midia);
        await _banco.SalvarAsync(ct);

        return Converter(midia);
    }

    public async Task<string> RemoverMidiaAsync(int id, CancellationToken ct = default)
    {
        var midia = await _banco.MidiasLanding.FirstOrDefaultAsync(m => m.Id == id, ct)
            ?? throw new RegraDeNegocioException("Imagem não encontrada.");

        var caminho = midia.Url;

        _banco.MidiasLanding.Remove(midia);
        await _banco.SalvarAsync(ct);

        return caminho;
    }

    public async Task<FaqResposta> CriarFaqAsync(FaqRequisicao dados, CancellationToken ct = default)
    {
        ValidarFaq(dados);

        var faq = new Faq
        {
            Pergunta = dados.Pergunta.Trim(),
            Resposta = dados.Resposta.Trim(),
            Ordem = dados.Ordem,
            Ativa = dados.Ativa
        };

        _banco.Faqs.Add(faq);
        await _banco.SalvarAsync(ct);

        return Converter(faq);
    }

    public async Task<FaqResposta> AtualizarFaqAsync(
        int id, FaqRequisicao dados, CancellationToken ct = default)
    {
        ValidarFaq(dados);

        var faq = await _banco.Faqs.FirstOrDefaultAsync(f => f.Id == id, ct)
            ?? throw new RegraDeNegocioException("Pergunta não encontrada.");

        faq.Pergunta = dados.Pergunta.Trim();
        faq.Resposta = dados.Resposta.Trim();
        faq.Ordem = dados.Ordem;
        faq.Ativa = dados.Ativa;

        await _banco.SalvarAsync(ct);
        return Converter(faq);
    }

    public async Task RemoverFaqAsync(int id, CancellationToken ct = default)
    {
        var faq = await _banco.Faqs.FirstOrDefaultAsync(f => f.Id == id, ct)
            ?? throw new RegraDeNegocioException("Pergunta não encontrada.");

        _banco.Faqs.Remove(faq);
        await _banco.SalvarAsync(ct);
    }

    private static void ValidarFaq(FaqRequisicao dados)
    {
        if (string.IsNullOrWhiteSpace(dados.Pergunta))
            throw new RegraDeNegocioException("Informe a pergunta.");

        if (string.IsNullOrWhiteSpace(dados.Resposta))
            throw new RegraDeNegocioException("Informe a resposta.");
    }

    private static SecaoLandingResposta Converter(SecaoLanding s)
        => new(s.Id, s.Chave, s.Titulo, s.Subtitulo, s.Texto, s.TextoBotao, s.ImagemUrl,
               s.Ordem, s.Ativa,
               s.Midias.Where(m => m.Ativa).OrderBy(m => m.Ordem).Select(Converter).ToList());

    private static MidiaResposta Converter(MidiaLanding m)
        => new(m.Id, m.Url, m.Legenda, m.Ordem, m.Ativa);

    private static FaqResposta Converter(Faq f)
        => new(f.Id, f.Pergunta, f.Resposta, f.Ordem, f.Ativa);
}
