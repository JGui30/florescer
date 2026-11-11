import { useState } from 'react';
import './admin.css';
import AdminLogin from './components/AdminLogin.jsx';
import Sidebar from './components/Sidebar.jsx';
import OverviewTab from './components/OverviewTab.jsx';
import AgendaTab from './components/AgendaTab.jsx';
import ProfessionalsTab from './components/ProfessionalsTab.jsx';
import ProceduresTab from './components/ProceduresTab.jsx';
import FinanceiroTab from './components/FinanceiroTab.jsx';
import VitrineTab from './components/VitrineTab.jsx';
import { auth } from '../services/api.js';
import { lerToken, lerUsuario } from '../services/http.js';

export default function AdminApp() {
  const [usuario, setUsuario] = useState(() => {
    const guardado = lerToken() ? lerUsuario() : null;
    return guardado?.perfil === 'Administrador' ? guardado : null;
  });

  const [tab, setTab] = useState('overview');

  function sair() {
    auth.sair();
    setUsuario(null);
  }

  if (!usuario) {
    return <AdminLogin onAutenticado={setUsuario} />;
  }

  return (
    <div className="admin-page">
      <div className="shell">
        <Sidebar activeTab={tab} onChange={setTab} usuario={usuario} onSair={sair} />
        <div className="main">
          {tab === 'overview' && <OverviewTab />}
          {tab === 'agenda' && <AgendaTab />}
          {tab === 'pros' && <ProfessionalsTab />}
          {tab === 'procs' && <ProceduresTab />}
          {tab === 'financeiro' && <FinanceiroTab />}
          {tab === 'vitrine' && <VitrineTab />}
        </div>
      </div>
    </div>
  );
}
