import { useState, useEffect } from 'react';
import { profissionais } from '../../../services/api.js';

const DOW = ['dom', 'seg', 'ter', 'qua', 'qui', 'sex', 'sáb'];

export default function RescheduleModal({ appt, onClose, onConfirmar }) {
  const [dias, setDias] = useState([]);
  const [diaSelecionado, setDiaSelecionado] = useState(null);
  const [horario, setHorario] = useState(null);
  const [carregando, setCarregando] = useState(true);
  const [salvando, setSalvando] = useState(false);
  const [erro, setErro] = useState('');

  useEffect(() => {
    profissionais
      .disponibilidade(appt.profissionalId, appt.procedimentoId, 21)
      .then(setDias)
      .catch((err) => setErro(err.message))
      .finally(() => setCarregando(false));
  }, [appt.profissionalId, appt.procedimentoId]);

  async function confirmar() {
    setSalvando(true);
    setErro('');
    try {
      await onConfirmar(horario.inicio);
    } catch (err) {
      setErro(err.message);
      setSalvando(false);
    }
  }

  const dia = diaSelecionado !== null ? dias[diaSelecionado] : null;

  return (
    <div className="modal-overlay open">
      <div className="modal-box">
        <h3>Remarcar horário</h3>
        <p>{appt.procedimentoNome} com {appt.profissionalNome}</p>

        {}
        <div className="aviso-prazo compacto">
          Ao remarcar, o novo horário também precisará ser confirmado dentro do prazo.
        </div>
        {carregando && <p className="step-sub">Buscando horários...</p>}
        {erro && <div className="form-error">{erro}</div>}
        {!carregando && (
          <>
            <div className="date-strip">
              {dias.map((d, i) => {
                const temVaga = d.profissionalTrabalha && d.horarios.some((h) => h.disponivel);
                const data = new Date(d.data);
                return (
                  <div
                    className={`date-chip ${diaSelecionado === i ? 'selected' : ''} ${temVaga ? '' : 'sem-vaga'}`}
                    key={d.data}
                    onClick={() => temVaga && (setDiaSelecionado(i), setHorario(null))}
                  >
                    <div className="dow">{DOW[data.getDay()]}</div>
                    <div className="dnum">{data.getDate()}</div>
                  </div>
                );
              })}
            </div>
            {dia && dia.profissionalTrabalha && (
              <div className="time-grid">
                {dia.horarios.map((h) => {
                  const classe = !h.disponivel ? 'taken' : horario?.inicio === h.inicio ? 'selected' : '';
                  return (
                    <div
                      className={`time-slot ${classe}`}
                      key={h.inicio}
                      onClick={() => h.disponivel && setHorario(h)}
                    >
                      {h.hora}
                    </div>
                  );
                })}
              </div>
            )}
          </>
        )}
        <div className="modal-close-row">
          <button className="btn-ghost" style={{ width: 'auto', flexShrink: 0 }} onClick={onClose}>Cancelar</button>
          <button className="btn btn-primary" disabled={!horario || salvando} onClick={confirmar}>
            {salvando ? 'Remarcando...' : 'Confirmar novo horário'}
          </button>
        </div>
      </div>
    </div>
  );
}
