import { useState } from 'react';
import AppointmentCard from './AppointmentCard.jsx';
import RescheduleModal from './RescheduleModal.jsx';
import { agendamentos } from '../../../services/api.js';

export default function AppointmentsList({ appointments, onRecarregar }) {
  const [remarcando, setRemarcando] = useState(null);
  const [erro, setErro] = useState('');
  const [ocupado, setOcupado] = useState(false);

  async function executar(acao) {
    setOcupado(true);
    setErro('');
    try {
      await acao();
      await onRecarregar();
    } catch (err) {
      setErro(err.message);

      await onRecarregar();
    } finally {
      setOcupado(false);
    }
  }

  const pendentes = appointments.filter((a) => a.precisaConfirmar && !a.prazoVencido);
  const demais = appointments.filter((a) => !(a.precisaConfirmar && !a.prazoVencido));

  return (
    <div className="content">
      <h2 className="step-title">Meus agendamentos</h2>
      <p className="step-sub">Confirme, remarque ou cancele seus horários</p>

      {erro && <div className="form-error">{erro}</div>}

      {appointments.length === 0 && (
        <p className="step-sub">Você ainda não tem agendamentos. Toque em "Agendar" para marcar seu horário.</p>
      )}

      {pendentes.length > 0 && (
        <div className="secao-pendente">
          <div className="secao-titulo">Aguardando sua confirmação</div>
          {pendentes.map((appt) => (
            <AppointmentCard
              key={appt.id}
              appt={appt}
              ocupado={ocupado}
              onConfirm={() => executar(() => agendamentos.confirmar(appt.id))}
              onCancel={() => executar(() => agendamentos.cancelar(appt.id))}
              onReschedule={() => setRemarcando(appt)}
            />
          ))}
        </div>
      )}

      {demais.map((appt) => (
        <AppointmentCard
          key={appt.id}
          appt={appt}
          ocupado={ocupado}
          onConfirm={() => executar(() => agendamentos.confirmar(appt.id))}
          onCancel={() => executar(() => agendamentos.cancelar(appt.id))}
          onReschedule={() => setRemarcando(appt)}
        />
      ))}

      {remarcando && (
        <RescheduleModal
          appt={remarcando}
          onClose={() => setRemarcando(null)}
          onConfirmar={async (novoInicio) => {
            await executar(() => agendamentos.reagendar(remarcando.id, novoInicio));
            setRemarcando(null);
          }}
        />
      )}
    </div>
  );
}
