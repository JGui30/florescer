import { useState } from 'react';
import { agendamentos } from '../../services/api.js';

export default function ConcluirModal({ agendamento, onFechar, onConcluido }) {
  const [valor, setValor] = useState(agendamento.valorPrevisto.toFixed(2));
  const [erro, setErro] = useState('');
  const [salvando, setSalvando] = useState(false);

  async function confirmar() {
    const numero = Number(String(valor).replace(',', '.'));

    if (Number.isNaN(numero) || numero < 0) {
      setErro('Informe um valor válido.');
      return;
    }

    setSalvando(true);
    setErro('');
    try {
      await agendamentos.concluir(agendamento.id, numero);
      await onConcluido();
    } catch (err) {
      setErro(err.message);
      setSalvando(false);
    }
  }

  const previsto = agendamento.valorPrevisto;
  const cobrado = Number(String(valor).replace(',', '.')) || 0;
  const diferenca = cobrado - previsto;

  return (
    <div className="modal-overlay open">
      <div className="modal-box admin-modal">
        <h3>Dar baixa no atendimento</h3>

        <div className="baixa-resumo">
          <div className="baixa-linha"><span>Cliente</span><strong>{agendamento.clienteNome}</strong></div>
          <div className="baixa-linha"><span>Procedimento</span><strong>{agendamento.procedimentoNome}</strong></div>
          <div className="baixa-linha"><span>Profissional</span><strong>{agendamento.profissionalNome}</strong></div>
          <div className="baixa-linha"><span>Horário</span><strong>{agendamento.dataLabel} às {agendamento.hora}</strong></div>
          <div className="baixa-linha">
            <span>Preço de tabela</span>
            <strong>R$ {previsto.toFixed(2).replace('.', ',')}</strong>
          </div>
        </div>

        <div className="field">
          <label>Valor efetivamente cobrado (R$)</label>
          <input value={valor} onChange={(e) => setValor(e.target.value)} inputMode="decimal" />
        </div>

        {diferenca !== 0 && (
          <div className="baixa-diferenca">
            {diferenca < 0
              ? `Desconto de R$ ${Math.abs(diferenca).toFixed(2).replace('.', ',')}`
              : `Acréscimo de R$ ${diferenca.toFixed(2).replace('.', ',')}`}
          </div>
        )}

        <div className="aviso-financeiro">
          Ao dar baixa, este valor entra no faturamento do dia {agendamento.dataLabel} e passa a
          contar nos gráficos do painel financeiro.
        </div>

        {erro && <div className="form-error">{erro}</div>}

        <div className="modal-close-row">
          <button className="btn-ghost" style={{ width: 'auto', flexShrink: 0 }} onClick={onFechar}>Cancelar</button>
          <button className="btn btn-primary" onClick={confirmar} disabled={salvando}>
            {salvando ? 'Registrando...' : 'Confirmar baixa'}
          </button>
        </div>
      </div>
    </div>
  );
}
