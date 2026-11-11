namespace Florescer.Application.Dtos;

public record CriarAgendamentoRequisicao(
    int ProcedimentoId,
    int ProfissionalId,
    DateTime Inicio,
    string? Observacoes);

public record ReagendarRequisicao(
    DateTime NovoInicio,
    int? NovoProfissionalId);

public record CancelarRequisicao(string? Motivo);

public record ConcluirAtendimentoRequisicao(decimal? ValorCobrado);

public record AgendamentoResposta(
    int Id,
    int ClienteId,
    string ClienteNome,
    string? ClienteTelefone,
    int ProfissionalId,
    string ProfissionalNome,
    int ProcedimentoId,
    string ProcedimentoNome,
    string CategoriaSlug,
    string CategoriaNome,
    DateTime Inicio,
    DateTime Fim,
    string DataLabel,
    string Hora,
    string Status,
    decimal ValorPrevisto,
    decimal? ValorCobrado,
    DateTime PrazoConfirmacao,
    bool PrecisaConfirmar,
    bool PrazoVencido,
    bool PodeCancelar,
    bool PodeReagendar,
    string? MotivoCancelamento,
    string? AvisoDeConfirmacao,
    string? Observacoes);

public record HorarioDisponivelResposta(
    string Hora,
    DateTime Inicio,
    bool Disponivel);

public record DisponibilidadeDoDiaResposta(
    DateTime Data,
    string DataLabel,
    bool ProfissionalTrabalha,
    IReadOnlyList<HorarioDisponivelResposta> Horarios);

public record PreviaDeAgendamentoResposta(
    DateTime Inicio,
    DateTime Fim,
    decimal Valor,
    DateTime PrazoConfirmacao,
    bool NasceConfirmado,
    string Aviso);

public record DiaComMovimentoResposta(
    DateTime Data,
    int Total,
    int Confirmados,
    int Pendentes,
    int Cancelados,
    int Concluidos);

public record PeriodoDaAgendaResposta(
    DateTime? PrimeiroAgendamento,
    DateTime? UltimoAgendamento);
