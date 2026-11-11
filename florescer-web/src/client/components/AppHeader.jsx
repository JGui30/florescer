import BloomLogo from '../../assets/BloomLogo.jsx';

function IconePino() {
  return (
    <svg viewBox="0 0 24 24" width="13" height="13" aria-hidden="true">
      <path
        d="M12 2c-3.6 0-6.5 2.9-6.5 6.5C5.5 13.4 12 22 12 22s6.5-8.6 6.5-13.5C18.5 4.9 15.6 2 12 2z"
        fill="none" stroke="currentColor" strokeWidth="1.8" strokeLinejoin="round"
      />
      <circle cx="12" cy="8.5" r="2.4" fill="none" stroke="currentColor" strokeWidth="1.8" />
    </svg>
  );
}

export default function AppHeader({ userName, onSair }) {
  return (
    <div className="app-header">
      <div className="brand">
        <BloomLogo size="sm" />
        Florescer
        {onSair && <span className="sair-link" onClick={onSair}>sair</span>}
      </div>

      <div className="tagline">Olá, {userName}! Bem-vindo(a) ao Florescer</div>

      <div className="estabelecimento">
        <div className="estab-item">
          <span className="estab-icone"><IconePino /></span>
          <div>
            <div className="estab-rua">Praça Quatorze de Novembro, Centro</div>
            <div className="estab-cidade">Santana de Parnaíba · SP · CEP 06513-001</div>
          </div>
        </div>
      </div>
    </div>
  );
}
