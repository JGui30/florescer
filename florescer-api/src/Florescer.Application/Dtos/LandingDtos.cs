namespace Florescer.Application.Dtos;

public record MidiaResposta(
    int Id,
    string Url,
    string? Legenda,
    int Ordem,
    bool Ativa);

public record SecaoLandingResposta(
    int Id,
    string Chave,
    string Titulo,
    string? Subtitulo,
    string? Texto,
    string? TextoBotao,
    string? ImagemUrl,
    int Ordem,
    bool Ativa,
    IReadOnlyList<MidiaResposta> Midias);

public record SecaoLandingRequisicao(
    string Titulo,
    string? Subtitulo,
    string? Texto,
    string? TextoBotao,
    string? ImagemUrl,
    bool Ativa);

public record FaqResposta(
    int Id,
    string Pergunta,
    string Resposta,
    int Ordem,
    bool Ativa);

public record FaqRequisicao(
    string Pergunta,
    string Resposta,
    int Ordem,
    bool Ativa);

public record EstabelecimentoResposta(
    string Nome,
    string? Slogan,
    string? Telefone,
    string? WhatsApp,
    string? Email,
    string? Endereco,
    string? HorarioFuncionamento,
    string? InstagramUrl,
    string? FacebookUrl,
    string? MapaUrl);

public record EstabelecimentoRequisicao(
    string Nome,
    string? Slogan,
    string? Telefone,
    string? WhatsApp,
    string? Email,
    string? Endereco,
    string? HorarioFuncionamento,
    string? InstagramUrl,
    string? FacebookUrl,
    string? MapaUrl);

public record ProfissionalVitrineResposta(
    int Id,
    string Nome,
    string Especialidade,
    string? FotoUrl);

public record NumerosDaVitrineResposta(
    int AtendimentosRealizados,
    int ClientesAtendidas,
    int ProfissionaisAtivas,
    int ProcedimentosOferecidos,
    int CategoriasOferecidas,
    int HorariosLivresNaSemana,
    decimal OcupacaoDaSemana);

public record LandingResposta(
    EstabelecimentoResposta Estabelecimento,
    IReadOnlyList<SecaoLandingResposta> Secoes,
    IReadOnlyList<CategoriaResposta> Categorias,
    IReadOnlyList<ProfissionalVitrineResposta> Equipe,
    IReadOnlyList<FaqResposta> Faqs,
    NumerosDaVitrineResposta Numeros);
