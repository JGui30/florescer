namespace Florescer.Application.Dtos;

public record ProcedimentoResposta(
    int Id,
    string Nome,
    string? Descricao,
    int CategoriaId,
    string CategoriaSlug,
    string CategoriaNome,
    int DuracaoMinutos,
    decimal Preco,
    bool Ativo);

public record CategoriaResposta(
    int Id,
    string Nome,
    string Slug,
    string Cor,
    string? ImagemUrl,
    IReadOnlyList<ProcedimentoResposta> Procedimentos);

public record CategoriaRequisicao(
    string Nome,
    string Cor,
    int Ordem,
    bool Ativa = true);

public record ProcedimentoRequisicao(
    string Nome,
    string? Descricao,
    int CategoriaId,
    int DuracaoMinutos,
    decimal Preco,
    bool Ativo = true);

public record ProfissionalResposta(
    int Id,
    string Nome,
    string Especialidade,
    string? Email,
    string? Telefone,
    string? FotoUrl,
    bool Ativa,
    IReadOnlyList<int> Dias,
    IReadOnlyList<int> ProcedimentoIds,
    bool JaAtendeu,
    int AtendimentosAnteriores,
    DateTime? UltimoAtendimento);

public record ProfissionalRequisicao(
    string Nome,
    string Especialidade,
    string? Email,
    string? Telefone,
    bool Ativa,
    IReadOnlyList<int> Dias,
    IReadOnlyList<int> ProcedimentoIds,
    string HoraInicio = "09:00",
    string HoraFim = "18:00");

public record BloqueioRequisicao(
    int? ProfissionalId,
    DateTime Inicio,
    DateTime Fim,
    int Tipo,
    string? Motivo);

public record BloqueioResposta(
    int Id,
    int? ProfissionalId,
    string? ProfissionalNome,
    DateTime Inicio,
    DateTime Fim,
    string Tipo,
    string? Motivo);
