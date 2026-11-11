import { useState } from 'react';
import BloomLogo from '../../assets/BloomLogo.jsx';
import RecoveryFlow from './RecoveryFlow.jsx';
import { auth } from '../../services/api.js';

export default function AuthScreen({ onAuthenticated }) {
  const [tab, setTab] = useState('login');
  const [recovering, setRecovering] = useState(false);
  const [loading, setLoading] = useState(false);

  const [loginEmail, setLoginEmail] = useState('');
  const [loginPass, setLoginPass] = useState('');

  const [name, setName] = useState('');
  const [email, setEmail] = useState('');
  const [pass, setPass] = useState('');
  const [pass2, setPass2] = useState('');
  const [terms, setTerms] = useState(false);
  const [error, setError] = useState('');

  async function handleLogin(e) {
    e.preventDefault();
    if (!loginEmail || !loginPass) {
      setError('Informe e-mail e senha para entrar.');
      return;
    }

    setError('');
    setLoading(true);
    try {
      const usuario = await auth.entrar(loginEmail, loginPass);
      onAuthenticated(usuario);
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  }

  async function handleSignup(e) {
    e.preventDefault();
    if (!name || !email || !pass) {
      setError('Preencha nome, e-mail e senha para continuar.');
      return;
    }
    if (pass !== pass2) {
      setError('As senhas não coincidem.');
      return;
    }
    if (!terms) {
      setError('É preciso aceitar os termos de uso para criar a conta.');
      return;
    }

    setError('');
    setLoading(true);
    try {
      const usuario = await auth.criarConta(name, email, pass);
      onAuthenticated(usuario);
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  }

  if (recovering) {
    return <RecoveryFlow onVoltar={() => setRecovering(false)} />;
  }

  return (
    <div className="content">
      <div className="auth-hero">
        <BloomLogo size="lg" />
        <div className="auth-brand">Florescer</div>
        <div className="auth-tagline">Agende seu momento de cuidado</div>
      </div>

      <div className="auth-tabs">
        <div className={`auth-tab ${tab === 'login' ? 'active' : ''}`} onClick={() => { setTab('login'); setError(''); }}>
          Entrar
        </div>
        <div className={`auth-tab ${tab === 'signup' ? 'active' : ''}`} onClick={() => { setTab('signup'); setError(''); }}>
          Criar conta
        </div>
      </div>

      {tab === 'login' && (
        <form className="auth-form" onSubmit={handleLogin}>
          <div className="field">
            <label>E-mail</label>
            <input type="email" value={loginEmail} onChange={(e) => setLoginEmail(e.target.value)} placeholder="voce@email.com" />
          </div>
          <div className="field">
            <label>Senha</label>
            <input type="password" value={loginPass} onChange={(e) => setLoginPass(e.target.value)} placeholder="••••••••" />
          </div>
          {error && <div className="form-error">{error}</div>}
          <button className="btn btn-primary" type="submit" disabled={loading}>
            {loading ? 'Entrando...' : 'Entrar'}
          </button>
          <div className="auth-switch">
            <span onClick={() => setRecovering(true)}>Esqueci minha senha</span>
          </div>
          <div className="auth-switch">
            Não tem conta? <span onClick={() => { setTab('signup'); setError(''); }}>Criar conta</span>
          </div>
        </form>
      )}

      {tab === 'signup' && (
        <form className="auth-form" onSubmit={handleSignup}>
          <div className="field">
            <label>Nome completo</label>
            <input value={name} onChange={(e) => setName(e.target.value)} placeholder="Seu nome" />
          </div>
          <div className="field">
            <label>E-mail</label>
            <input type="email" value={email} onChange={(e) => setEmail(e.target.value)} placeholder="voce@email.com" />
          </div>
          <div className="field">
            <label>Senha</label>
            <input type="password" value={pass} onChange={(e) => setPass(e.target.value)} placeholder="Crie uma senha" />
          </div>
          <div className="field">
            <label>Confirmar senha</label>
            <input type="password" value={pass2} onChange={(e) => setPass2(e.target.value)} placeholder="Repita a senha" />
          </div>
          <label className="check-row">
            <input type="checkbox" checked={terms} onChange={(e) => setTerms(e.target.checked)} />
            Concordo com os termos de uso e política de privacidade
          </label>
          {error && <div className="form-error">{error}</div>}
          <button className="btn btn-primary" type="submit" disabled={loading}>
            {loading ? 'Criando conta...' : 'Criar conta'}
          </button>
          <div className="auth-switch">
            Já tem conta? <span onClick={() => { setTab('login'); setError(''); }}>Entrar</span>
          </div>
        </form>
      )}

    </div>
  );
}
