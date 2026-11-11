import { useState, useEffect, useCallback } from 'react';
import './client.css';
import AuthScreen from './components/AuthScreen.jsx';
import AppHeader from './components/AppHeader.jsx';
import BottomNav from './components/BottomNav.jsx';
import BookingFlow from './components/booking/BookingFlow.jsx';
import AppointmentsList from './components/appointments/AppointmentsList.jsx';
import { agendamentos, auth } from '../services/api.js';
import { lerToken, lerUsuario } from '../services/http.js';

export default function ClientApp() {
  const [usuario, setUsuario] = useState(() => (lerToken() ? lerUsuario() : null));
  const [activeView, setActiveView] = useState('booking');
  const [appointments, setAppointments] = useState([]);
  const [erro, setErro] = useState('');

  const carregarAgendamentos = useCallback(async () => {
    try {
      setAppointments(await agendamentos.meus());
      setErro('');
    } catch (err) {
      setErro(err.message);
      if (!lerToken()) setUsuario(null);
    }
  }, []);

  useEffect(() => {
    if (usuario) carregarAgendamentos();
  }, [usuario, carregarAgendamentos]);

  function handleAuthenticated(dadosDoUsuario) {
    setUsuario(dadosDoUsuario);
    setActiveView('booking');
  }

  function sair() {
    auth.sair();
    setUsuario(null);
    setAppointments([]);
  }

  return (
    <div className="page-wrap">
      <div className="phone">
        {!usuario && <AuthScreen onAuthenticated={handleAuthenticated} />}

        {usuario && (
          <>
            <AppHeader userName={usuario.nome?.split(' ')[0]} onSair={sair} />

            {erro && <div className="banner-erro">{erro}</div>}

            {activeView === 'booking' && (
              <BookingFlow onAgendamentoCriado={carregarAgendamentos} />
            )}

            {activeView === 'appointments' && (
              <AppointmentsList appointments={appointments} onRecarregar={carregarAgendamentos} />
            )}

            <BottomNav activeView={activeView} onChange={setActiveView} />
          </>
        )}
      </div>
    </div>
  );
}
