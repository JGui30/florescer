import { useState, useEffect } from 'react';
import { profissionais } from '../../../services/api.js';

const DOW = ['dom', 'seg', 'ter', 'qua', 'qui', 'sex', 'sáb'];

function temVagaNoDia(dia) {
  return dia.profissionalTrabalha && dia.horarios.some((h) => h.disponivel);
}

function motivoSemVaga(dia, nomeDaProfissional) {
  if (!dia.profissionalTrabalha) {
    return `${nomeDaProfissional} não atende neste dia da semana. ` +
           'Escolha outra data, ou volte uma etapa para escolher outra profissional.';
  }

  if (dia.horarios.length === 0) {
    return `A jornada de ${nomeDaProfissional} neste dia é curta demais ` +
           'para a duração deste procedimento.';
  }

  const agora = new Date();
  if (dia.horarios.every((h) => new Date(h.inicio) <= agora)) {
    return 'Os horários deste dia já passaram. Escolha uma data mais à frente.';
  }

  return `A agenda de ${nomeDaProfissional} está cheia neste dia. Escolha outra data.`;
}

export default function ScheduleStep({ selection, onSelectSchedule, onBack, onContinue }) {
  const [dias, setDias] = useState([]);
  const [diaSelecionado, setDiaSelecionado] = useState(null);
  const [carregando, setCarregando] = useState(true);
  const [erro, setErro] = useState('');

  const { procedimento, profissional, horario } = selection;

  useEffect(() => {
    setCarregando(true);
    profissionais
      .disponibilidade(profissional.id, procedimento.id, 21)
      .then((resposta) => {
        setDias(resposta);
        const primeiroComVaga = resposta.findIndex((d) => temVagaNoDia(d));
        if (primeiroComVaga >= 0) setDiaSelecionado(primeiroComVaga);
        else setDiaSelecionado(resposta.length > 0 ? 0 : null);
      })
      .catch((err) => setErro(err.message))
      .finally(() => setCarregando(false));
  }, [profissional.id, procedimento.id]);

  if (carregando) {
    return <div className="step-panel active"><p className="step-sub">Buscando horários disponíveis...</p></div>;
  }

  if (erro) {
    return (
      <div className="step-panel active">
        <div className="form-error">{erro}</div>
        <div className="nav-row"><button className="btn-ghost" onClick={onBack}>← Voltar</button></div>
      </div>
    );
  }

  const dia = diaSelecionado !== null ? dias[diaSelecionado] : null;
  const aviso = dia && !temVagaNoDia(dia) ? motivoSemVaga(dia, profissional.nome) : '';

  return (
    <div className="step-panel active">
      <h2 className="step-title">Data e horário</h2>
      <p className="step-sub">Selecione um dia e um horário disponível</p>

      <div className="date-strip">
        {dias.map((d, i) => {
          const data = new Date(d.data);
          return (
            <div
              className={`date-chip ${diaSelecionado === i ? 'selected' : ''} ${temVagaNoDia(d) ? '' : 'sem-vaga'}`}
              key={d.data}
              onClick={() => {
                setDiaSelecionado(i);
                onSelectSchedule(null);
              }}
            >
              <div className="dow">{DOW[data.getDay()]}</div>
              <div className="dnum">{data.getDate()}</div>
            </div>
          );
        })}
      </div>

      {aviso && <p className="aviso-dia">{aviso}</p>}

      {dia && dia.profissionalTrabalha && dia.horarios.length > 0 && (
        <div className="time-grid">
          {dia.horarios.map((h) => {
            const classe = !h.disponivel ? 'taken' : horario?.inicio === h.inicio ? 'selected' : '';
            return (
              <div
                className={`time-slot ${classe}`}
                key={h.inicio}
                onClick={() => h.disponivel && onSelectSchedule(h)}
              >
                {h.hora}
              </div>
            );
          })}
        </div>
      )}

      <div className="nav-row">
        <button className="btn-ghost" onClick={onBack}>← Voltar</button>
        <button className="btn btn-primary" disabled={!horario} onClick={onContinue}>
          Continuar
        </button>
      </div>
    </div>
  );
}
