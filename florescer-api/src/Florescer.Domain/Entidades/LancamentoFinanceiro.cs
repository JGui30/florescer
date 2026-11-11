using Florescer.Domain.Enums;

namespace Florescer.Domain.Entidades;

public class LancamentoFinanceiro
{
    public int Id { get; set; }

    public TipoLancamento Tipo { get; set; } = TipoLancamento.Receita;

    public int? AgendamentoId { get; set; }
    public Agendamento? Agendamento { get; set; }

    public decimal Valor { get; set; }

    public DateTime Data { get; set; }

    public string Descricao { get; set; } = string.Empty;

    public int? RegistradoPorUsuarioId { get; set; }
    public Usuario? RegistradoPor { get; set; }

    public DateTime CriadoEm { get; set; }
}
