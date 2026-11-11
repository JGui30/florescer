import { useState } from 'react';
import BloomLogo from '../../assets/BloomLogo.jsx';
import { auth } from '../../services/api.js';

export default function AdminLogin({ onAutenticado }) {
  const [email, setEmail] = useState('');
  const [senha, setSenha] = useState('');
  const [erro, setErro] = useState('');
  const [carregando, setCarregando] = useState(false);

  async function entrar(e) {
    e.preventDefault();
    setErro('');
    setCarregando(true);
    try {
      const usuario = await auth.entrarComoAdmin(email, senha);
      onAutenticado(usuario);
    } catch (err) {
      setErro(err.message);
    } finally {
      setCarregando(false);
    }
  }

  return (
    <div className="admin-login-page">
      <div className="admin-login-box">
        <div className="admin-login-brand">
          <BloomLogo size="lg" />
          <div className="admin-login-nome">Florescer</div>
          <div className="admin-login-sub">Painel administrativo</div>
        </div>

        <form onSubmit={entrar}>
          <div className="field">
            <label>E-mail</label>
            <input
              type="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              placeholder="admin@florescer.com.br"
              autoComplete="username"
            />
          </div>
          <div className="field">
            <label>Senha</label>
            <input
              type="password"
              value={senha}
              onChange={(e) => setSenha(e.target.value)}
              placeholder="••••••••"
              autoComplete="current-password"
            />
          </div>

          {erro && <div className="form-error">{erro}</div>}

          <button className="btn btn-primary" type="submit" disabled={carregando}>
            {carregando ? 'Entrando...' : 'Entrar no painel'}
          </button>
        </form>

        {}
        {import.meta.env.DEV && (
          <div className="admin-login-dica">
            Acesso padrão: <code>admin@florescer.com.br</code> / <code>Florescer@2025</code>
          </div>
        )}
      </div>
    </div>
  );
}
