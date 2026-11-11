using Florescer.Application.Contratos;
using Florescer.Application.Dtos;
using Florescer.Application.Persistencia;
using Florescer.Domain.Entidades;
using Florescer.Domain.Enums;
using Florescer.Domain.Regras;
using Microsoft.EntityFrameworkCore;

namespace Florescer.Application.Servicos;

public class ServicoDeCatalogo
{
    private readonly IBancoDeDados _banco;
    private readonly IRelogio _relogio;

    public ServicoDeCatalogo(IBancoDeDados banco, IRelogio relogio)
    {
        _banco = banco;
        _relogio = relogio;
    }

    public async Task<IReadOnlyList<CategoriaResposta>> ListarCategoriasAsync(
        bool apenasAtivos = true, CancellationToken ct = default)
    {
        var categorias = await _banco.Categorias
            .Include(c => c.Procedimentos)
            .Where(c => !apenasAtivos || c.Ativa)
            .OrderBy(c => c.Ordem)
            .ToListAsync(ct);

        return categorias.Select(c => new CategoriaResposta(
            c.Id, c.Nome, c.Slug, c.Cor, c.ImagemUrl,
            c.Procedimentos
                .Where(p => !apenasAtivos || p.Ativo)
                .OrderBy(p => p.Nome)
                .Select(p => Converter(p, c))
                .ToList()))
            .ToList();
    }

    public async Task<IReadOnlyList<ProcedimentoResposta>> ListarProcedimentosAsync(
        bool apenasAtivos = true, CancellationToken ct = default)
    {
        var procedimentos = await _banco.Procedimentos
            .Include(p => p.Categoria)
            .Where(p => !apenasAtivos || p.Ativo)
            .OrderBy(p => p.Categoria.Ordem).ThenBy(p => p.Nome)
            .ToListAsync(ct);

        return procedimentos.Select(p => Converter(p, p.Categoria)).ToList();
    }

    public async Task<ProcedimentoResposta> CriarProcedimentoAsync(
        ProcedimentoRequisicao dados, CancellationToken ct = default)
    {
        Validar(dados);

        var categoria = await _banco.Categorias.FirstOrDefaultAsync(c => c.Id == dados.CategoriaId, ct)
            ?? throw new RegraDeNegocioException("Categoria não encontrada.");

        var procedimento = new Procedimento
        {
            Nome = dados.Nome.Trim(),
            Descricao = dados.Descricao?.Trim(),
            CategoriaId = dados.CategoriaId,
            DuracaoMinutos = dados.DuracaoMinutos,
            Preco = dados.Preco,
            Ativo = dados.Ativo
        };

        _banco.Procedimentos.Add(procedimento);
        await _banco.SalvarAsync(ct);

        return Converter(procedimento, categoria);
    }

    public async Task<ProcedimentoResposta> AtualizarProcedimentoAsync(
        int id, ProcedimentoRequisicao dados, CancellationToken ct = default)
    {
        Validar(dados);

        var procedimento = await _banco.Procedimentos
            .Include(p => p.Categoria)
            .FirstOrDefaultAsync(p => p.Id == id, ct)
            ?? throw new RegraDeNegocioException("Procedimento não encontrado.");

        procedimento.Nome = dados.Nome.Trim();
        procedimento.Descricao = dados.Descricao?.Trim();
        procedimento.CategoriaId = dados.CategoriaId;
        procedimento.DuracaoMinutos = dados.DuracaoMinutos;
        procedimento.Preco = dados.Preco;
        procedimento.Ativo = dados.Ativo;

        await _banco.SalvarAsync(ct);

        var categoria = await _banco.Categorias.FirstAsync(c => c.Id == dados.CategoriaId, ct);
        return Converter(procedimento, categoria);
    }

    public async Task DesativarProcedimentoAsync(int id, CancellationToken ct = default)
    {
        var procedimento = await _banco.Procedimentos.FirstOrDefaultAsync(p => p.Id == id, ct)
            ?? throw new RegraDeNegocioException("Procedimento não encontrado.");

        procedimento.Ativo = false;
        await _banco.SalvarAsync(ct);
    }

    public async Task<CategoriaResposta> CriarCategoriaAsync(
        CategoriaRequisicao dados, CancellationToken ct = default)
    {
        ValidarCategoria(dados);

        var slug = GerarSlug(dados.Nome);

        if (await _banco.Categorias.AnyAsync(c => c.Slug == slug, ct))
            throw new RegraDeNegocioException($"Já existe uma categoria chamada \"{dados.Nome}\".");

        var categoria = new Categoria
        {
            Nome = dados.Nome.Trim(),
            Slug = slug,
            Cor = dados.Cor.Trim(),
            Ordem = dados.Ordem,
            Ativa = dados.Ativa
        };

        _banco.Categorias.Add(categoria);
        await _banco.SalvarAsync(ct);

        return new CategoriaResposta(
            categoria.Id, categoria.Nome, categoria.Slug, categoria.Cor,
            categoria.ImagemUrl, Array.Empty<ProcedimentoResposta>());
    }

    public async Task<CategoriaResposta> AtualizarCategoriaAsync(
        int id, CategoriaRequisicao dados, CancellationToken ct = default)
    {
        ValidarCategoria(dados);

        var categoria = await _banco.Categorias
            .Include(c => c.Procedimentos)
            .FirstOrDefaultAsync(c => c.Id == id, ct)
            ?? throw new RegraDeNegocioException("Categoria não encontrada.");

        categoria.Nome = dados.Nome.Trim();
        categoria.Cor = dados.Cor.Trim();
        categoria.Ordem = dados.Ordem;
        categoria.Ativa = dados.Ativa;

        await _banco.SalvarAsync(ct);

        return new CategoriaResposta(
            categoria.Id, categoria.Nome, categoria.Slug, categoria.Cor, categoria.ImagemUrl,
            categoria.Procedimentos.OrderBy(p => p.Nome).Select(p => Converter(p, categoria)).ToList());
    }

    public async Task DesativarCategoriaAsync(int id, CancellationToken ct = default)
    {
        var categoria = await _banco.Categorias
            .Include(c => c.Procedimentos)
            .FirstOrDefaultAsync(c => c.Id == id, ct)
            ?? throw new RegraDeNegocioException("Categoria não encontrada.");

        var procedimentoIds = categoria.Procedimentos.Select(p => p.Id).ToList();

        var temAgendamentosFuturos = await _banco.Agendamentos.AnyAsync(a =>
            procedimentoIds.Contains(a.ProcedimentoId) &&
            a.Inicio > _relogio.Agora &&
            (a.Status == StatusAgendamento.Agendado || a.Status == StatusAgendamento.Confirmado), ct);

        if (temAgendamentosFuturos)
        {
            throw new RegraDeNegocioException(
                "Existem atendimentos futuros marcados para procedimentos desta categoria. " +
                "Cancele ou remarque antes de desativá-la.");
        }

        categoria.Ativa = false;
        foreach (var procedimento in categoria.Procedimentos)
            procedimento.Ativo = false;

        await _banco.SalvarAsync(ct);
    }

    private static void ValidarCategoria(CategoriaRequisicao dados)
    {
        if (string.IsNullOrWhiteSpace(dados.Nome))
            throw new RegraDeNegocioException("Informe o nome da categoria.");

        if (string.IsNullOrWhiteSpace(dados.Cor) || !dados.Cor.StartsWith("#") || dados.Cor.Length is not (4 or 7))
            throw new RegraDeNegocioException("Informe a cor em hexadecimal, no formato #RRGGBB.");
    }

    private static string GerarSlug(string nome)
    {
        var texto = nome.Trim().ToLowerInvariant();

        var acentuadas = "áàâãäéèêëíìîïóòôõöúùûüçñ";
        var simples    = "aaaaaeeeeiiiiooooouuuucn";

        var construtor = new System.Text.StringBuilder();
        foreach (var letra in texto)
        {
            var posicao = acentuadas.IndexOf(letra);
            if (posicao >= 0) construtor.Append(simples[posicao]);
            else if (char.IsLetterOrDigit(letra)) construtor.Append(letra);
            else if (letra == ' ' && construtor.Length > 0 && construtor[^1] != '-') construtor.Append('-');
        }

        return construtor.ToString().Trim('-');
    }

    public async Task<string?> DefinirImagemAsync(
        int categoriaId, string url, CancellationToken ct = default)
    {
        var categoria = await _banco.Categorias.FirstOrDefaultAsync(c => c.Id == categoriaId, ct)
            ?? throw new RegraDeNegocioException("Categoria não encontrada.");

        var anterior = categoria.ImagemUrl;
        categoria.ImagemUrl = url;

        await _banco.SalvarAsync(ct);
        return anterior;
    }

    public async Task<string?> RemoverImagemAsync(int categoriaId, CancellationToken ct = default)
    {
        var categoria = await _banco.Categorias.FirstOrDefaultAsync(c => c.Id == categoriaId, ct)
            ?? throw new RegraDeNegocioException("Categoria não encontrada.");

        var anterior = categoria.ImagemUrl;
        categoria.ImagemUrl = null;

        await _banco.SalvarAsync(ct);
        return anterior;
    }

    private static void Validar(ProcedimentoRequisicao dados)
    {
        if (string.IsNullOrWhiteSpace(dados.Nome))
            throw new RegraDeNegocioException("Informe o nome do procedimento.");

        if (dados.DuracaoMinutos <= 0)
            throw new RegraDeNegocioException("A duração deve ser maior que zero.");

        if (dados.Preco < 0)
            throw new RegraDeNegocioException("O preço não pode ser negativo.");
    }

    private static ProcedimentoResposta Converter(Procedimento p, Categoria c)
        => new(p.Id, p.Nome, p.Descricao, c.Id, c.Slug, c.Nome, p.DuracaoMinutos, p.Preco, p.Ativo);
}
