import BloomLogo from '../../assets/BloomLogo.jsx';

const NAV_ITEMS = [
  { key: 'overview', label: 'Visão geral', icon: '◆' },
  { key: 'agenda', label: 'Agenda', icon: '▤' },
  { key: 'financeiro', label: 'Financeiro', icon: '◈' },
  { key: 'pros', label: 'Profissionais', icon: '◐' },
  { key: 'procs', label: 'Procedimentos', icon: '✦' },
  { key: 'vitrine', label: 'Vitrine', icon: '❋' },
];

export default function Sidebar({ activeTab, onChange, usuario, onSair }) {
  const inicial = usuario?.nome?.charAt(0)?.toUpperCase() || 'A';

  return (
    <div className="sidebar">
      <div className="side-brand">
        <BloomLogo size="sm" dark />
        Florescer
      </div>
      <div className="side-sub">Painel administrativo</div>

      {NAV_ITEMS.map((item) => (
        <div
          key={item.key}
          className={`nav-item ${activeTab === item.key ? 'active' : ''}`}
          onClick={() => onChange(item.key)}
        >
          <span className="nav-icon">{item.icon}</span> {item.label}
        </div>
      ))}

      <div className="side-foot">
        <div className="side-admin">
          <div className="side-avatar">{inicial}</div>
          <div>
            <div className="side-admin-name">{usuario?.nome}</div>
            <div className="side-admin-role">Acesso administrativo</div>
          </div>
        </div>
        <div className="side-sair" onClick={onSair}>Sair do painel</div>
      </div>
    </div>
  );
}
