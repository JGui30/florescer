namespace Florescer.Application.Dtos;

public record IndicadorResposta(
    string Rotulo,
    decimal Valor,
    decimal ValorAnterior,
    decimal VariacaoPercentual);

public record VisaoGeralResposta(
    DateTime Data,
    int AgendamentosHoje,
    int AgendamentosOntem,
    decimal TaxaOcupacao,
    decimal TaxaOcupacaoOntem,
    decimal FaturamentoHoje,
    decimal FaturamentoOntem,
    int CancelamentosHoje,
    int CancelamentosOntem,
    int AguardandoConfirmacao,
    int AtendimentosAFechar,
    IReadOnlyList<EstatisticaCategoriaResposta> PorCategoria,
    IReadOnlyList<AgendamentoResposta> ProximosAtendimentos);

public record EstatisticaCategoriaResposta(
    string Slug,
    string Nome,
    string Cor,
    int Quantidade,
    decimal Percentual,
    decimal Valor);

public record PontoDaSerieResposta(
    string Rotulo,
    DateTime Data,
    decimal Valor,
    int Quantidade);

public record ReceitaPorProfissionalResposta(
    int ProfissionalId,
    string Nome,
    string Especialidade,
    decimal Total,
    int Atendimentos,
    decimal TicketMedio);

public record ReceitaPorProcedimentoResposta(
    int ProcedimentoId,
    string Nome,
    string CategoriaSlug,
    decimal Total,
    int Atendimentos);

public record ResumoFinanceiroResposta(
    string Periodo,
    DateTime Inicio,
    DateTime Fim,
    decimal Receita,
    decimal ReceitaPeriodoAnterior,
    decimal VariacaoPercentual,
    int AtendimentosConcluidos,
    decimal TicketMedio,
    decimal ReceitaPrevista,
    int AtendimentosAFechar,
    decimal TaxaDeCancelamento,
    decimal PerdaPorFaltaDeConfirmacao,
    IReadOnlyList<PontoDaSerieResposta> Serie,
    IReadOnlyList<EstatisticaCategoriaResposta> PorCategoria,
    IReadOnlyList<ReceitaPorProfissionalResposta> PorProfissional,
    IReadOnlyList<ReceitaPorProcedimentoResposta> TopProcedimentos);

public record AtendimentoAFecharResposta(
    int AgendamentoId,
    DateTime Inicio,
    string ClienteNome,
    string ProfissionalNome,
    string ProcedimentoNome,
    decimal ValorPrevisto);

public record LancamentoResposta(
    int Id,
    string Tipo,
    decimal Valor,
    DateTime Data,
    string Descricao,
    int? AgendamentoId,
    string? RegistradoPor);
