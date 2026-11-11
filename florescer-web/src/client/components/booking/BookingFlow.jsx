import { useState, useEffect } from 'react';
import RibbonProgress from '../RibbonProgress.jsx';
import CategoryStep from './CategoryStep.jsx';
import ProcedureStep from './ProcedureStep.jsx';
import ProfessionalStep from './ProfessionalStep.jsx';
import ScheduleStep from './ScheduleStep.jsx';
import ConfirmStep from './ConfirmStep.jsx';
import { catalogo } from '../../../services/api.js';

const SELECAO_VAZIA = {
  categoria: null,
  procedimento: null,
  profissional: null,
  horario: null,
};

export default function BookingFlow({ onAgendamentoCriado }) {
  const [step, setStep] = useState(0);
  const [selection, setSelection] = useState(SELECAO_VAZIA);
  const [categorias, setCategorias] = useState([]);
  const [carregando, setCarregando] = useState(true);
  const [erro, setErro] = useState('');

  useEffect(() => {
    catalogo
      .categorias()
      .then(setCategorias)
      .catch((err) => setErro(err.message))
      .finally(() => setCarregando(false));
  }, []);

  function resetFlow() {
    setSelection(SELECAO_VAZIA);
    setStep(0);
  }

  function selectCategory(categoria) {
    setSelection({ ...SELECAO_VAZIA, categoria });
    setStep(1);
  }

  function selectProcedure(procedimento) {
    setSelection((s) => ({ ...s, procedimento, profissional: null, horario: null }));
    setStep(2);
  }

  function selectProfessional(profissional) {
    setSelection((s) => ({ ...s, profissional, horario: null }));
    setStep(3);
  }

  function selectSchedule(horario) {
    setSelection((s) => ({ ...s, horario }));
  }

  if (carregando) {
    return <div className="content"><p className="step-sub">Carregando serviços...</p></div>;
  }

  if (erro) {
    return <div className="content"><div className="form-error">{erro}</div></div>;
  }

  return (
    <>
      <RibbonProgress currentStep={step} />
      <div className="content">
        {step === 0 && <CategoryStep categorias={categorias} onSelect={selectCategory} />}

        {step === 1 && (
          <ProcedureStep categoria={selection.categoria} onSelect={selectProcedure} onBack={() => setStep(0)} />
        )}

        {step === 2 && (
          <ProfessionalStep
            procedimento={selection.procedimento}
            onSelect={selectProfessional}
            onBack={() => setStep(1)}
          />
        )}

        {step === 3 && (
          <ScheduleStep
            selection={selection}
            onSelectSchedule={selectSchedule}
            onBack={() => setStep(2)}
            onContinue={() => setStep(4)}
          />
        )}

        {step === 4 && (
          <ConfirmStep
            selection={selection}
            onReset={resetFlow}
            onBack={() => setStep(3)}
            onAgendamentoCriado={onAgendamentoCriado}
          />
        )}
      </div>
    </>
  );
}
