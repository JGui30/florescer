import { useState, useEffect } from 'react';
import { agendamentos } from '../../../services/api.js';

const DOW = ['dom', 'seg', 'ter', 'qua', 'qui', 'sex', 'sáb'];

function formatarData(iso) {
  const d = new Date(iso);
  return `${DOW[d.getDay()]}, ${String(d.getDate()).padStart(2, '0')}/${String(d.getMonth() + 1).padStart(2, '0')}`;
}

function formatarHora(iso) {
  const d = new Date(iso);
  return `${String(d.getHours()).padStart(2, '0')}:${String(d.getMinutes()).padStart(2, '0')}`;
}

export default function ConfirmStep({ selection, onReset, onBack, onAgendamentoCriado }) {
  const { procedimento, profissional, horario } = selection;

  const [previa, setPrevia] = useState(null);
  const [criado, setCriado] = useState(null);
  const [salvando, setSalvando] = useState(false);
  const [erro, setErro] = useState('');

  useEffect(() => {
    agendamentos
      .previa(procedimento.id, horario.inicio)
      .then(setPrevia)
      .catch((err) => setErro(err.message));
  }, [procedimento.id, horario.inicio]);

  async function confirmar() {
    setSalvando(true);
    setErro('');
    try {
      const novo = await agendamentos.criar(procedimento.id, profissional.id, horario.inicio, null);
      setCriado(novo);
      if (onAgendamentoCriado) onAgendamentoCriado();
    } catch (err) {
      setErro(err.message);
    } finally {
      setSalvando(false);
    }
  }

  if (criado) {
    return (
      <div className="step-panel active">
        <div className="success-wrap">
          <div className="success-mark">✓</div>
          <div className="success-title">Tudo certo!</div>
          <div className="success-sub">
            {criado.procedimentoNome} com {criado.profissionalNome}, dia {criado.dataLabel} às {criado.hora}.
          </div>

          {criado.precisaConfirmar ? (
            <div className="aviso-prazo">
              <strong>Falta confirmar.</strong> {criado.avisoDeConfirmacao} Você pode confirmar agora mesmo
              na aba "Meus agendamentos".
            </div>
          ) : (
            <div className="aviso-ok">
              Seu agendamento já está <strong>confirmado</strong>.
            </div>
          )}

          <button className="btn btn-primary" onClick={onReset}>Novo agendamento</button>
        </div>
      </div>
    );
  }

  return (
    <div className="step-panel active">
      <h2 className="step-title">Confirme seu agendamento</h2>
      <p className="step-sub">Revise os detalhes antes de confirmar</p>

      <div className="summary-card">
        <div className="summary-row"><span className="k">Procedimento</span><span className="v">{procedimento.nome}</span></div>
        <div className="summary-row"><span className="k">Profissional</span><span className="v">{profissional.nome}</span></div>
        <div className="summary-row">
          <span className="k">Data</span>
          <span className="v">{formatarData(horario.inicio)} às {formatarHora(horario.inicio)}</span>
        </div>
        <div className="summary-row"><span className="k">Duração</span><span className="v">{procedimento.duracaoMinutos} min</span></div>
        <div className="summary-row">
          <span className="k">Total</span>
          <span className="v summary-total">R$ {procedimento.preco.toFixed(2).replace('.', ',')}</span>
        </div>
      </div>

      {}
      {previa && (
        <div className={previa.nasceConfirmado ? 'aviso-ok' : 'aviso-prazo'}>
          {previa.aviso}
        </div>
      )}
      {erro && <div className="form-error">{erro}</div>}
      <button className="btn btn-primary" onClick={confirmar} disabled={salvando || !previa}>
        {salvando ? 'Agendando...' : 'Confirmar agendamento'}
      </button>
      <div className="nav-row">
        <button className="btn-ghost" onClick={onBack}>← Voltar</button>
      </div>
    </div>
  );
}
