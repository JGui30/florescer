import { useState } from 'react';
import BloomLogo from '../../assets/BloomLogo.jsx';
import { auth } from '../../services/api.js';

export default function RecoveryFlow({ onVoltar }) {
  const [etapa, setEtapa] = useState('email');
  const [email, setEmail] = useState('');
  const [codigo, setCodigo] = useState('');
  const [novaSenha, setNovaSenha] = useState('');
  const [novaSenha2, setNovaSenha2] = useState('');

  const [envio, setEnvio] = useState(null);
  const [erro, setErro] = useState('');
  const [carregando, setCarregando] = useState(false);

  async function solicitar(e) {
    e.preventDefault();
    if (!email) {
      setErro('Informe o e-mail cadastrado.');
      return;
    }

    setErro('');
    setCarregando(true);
    try {
      const resposta = await auth.solicitarRecuperacao(email);
      setEnvio(resposta);
      setEtapa('codigo');
    } catch (err) {
      setErro(err.message);
    } finally {
      setCarregando(false);
    }
  }

  async function validar(e) {
    e.preventDefault();
    if (codigo.length !== 6) {
      setErro('O código tem 6 dígitos.');
      return;
    }

    setErro('');
    setCarregando(true);
    try {
      await auth.validarCodigo(email, codigo);
      setEtapa('senha');
    } catch (err) {
      setErro(err.message);
    } finally {
      setCarregando(false);
    }
  }

  async function redefinir(e) {
    e.preventDefault();
    if (novaSenha.length < 6) {
      setErro('A nova senha deve ter pelo menos 6 caracteres.');
      return;
    }
    if (novaSenha !== novaSenha2) {
      setErro('As senhas não coincidem.');
      return;
    }

    setErro('');
    setCarregando(true);
    try {
      await auth.redefinirSenha(email, codigo, novaSenha);
      setEtapa('pronto');
    } catch (err) {
      setErro(err.message);
    } finally {
      setCarregando(false);
    }
  }

  return (
    <div className="content">
      <div className="auth-hero">
        <BloomLogo size="lg" />
        <div className="auth-brand">Recuperar conta</div>
        <div className="auth-tagline">
          {etapa === 'email' && 'Enviaremos um código para o seu e-mail'}
          {etapa === 'codigo' && 'Digite o código que enviamos'}
          {etapa === 'senha' && 'Crie uma nova senha'}
          {etapa === 'pronto' && 'Senha alterada com sucesso'}
        </div>
      </div>

      {etapa === 'email' && (
        <form className="auth-form" onSubmit={solicitar}>
          <div className="field">
            <label>E-mail cadastrado</label>
            <input type="email" value={email} onChange={(e) => setEmail(e.target.value)} placeholder="voce@email.com" />
          </div>

          {erro && <div className="form-error">{erro}</div>}
          <button className="btn btn-primary" type="submit" disabled={carregando}>
            {carregando ? 'Enviando...' : 'Enviar código'}
          </button>
          <div className="auth-switch">
            <span onClick={onVoltar}>← Voltar para o login</span>
          </div>
        </form>
      )}

      {etapa === 'codigo' && (
        <form className="auth-form" onSubmit={validar}>
          <div className="recovery-info">
            {envio?.mensagem} Enviamos para <strong>{envio?.destinoMascarado}</strong>.
          </div>

          {}
          {envio?.codigoParaDemonstracao && (
            <div className="recovery-demo">
              Modo demonstração — código gerado: <strong>{envio.codigoParaDemonstracao}</strong>
            </div>
          )}
          <div className="field">
            <label>Código de 6 dígitos</label>
            <input
              value={codigo}
              onChange={(e) => setCodigo(e.target.value.replace(/\D/g, '').slice(0, 6))}
              placeholder="000000"
              inputMode="numeric"
              className="codigo-input"
            />
          </div>
          {erro && <div className="form-error">{erro}</div>}
          <button className="btn btn-primary" type="submit" disabled={carregando}>
            {carregando ? 'Validando...' : 'Validar código'}
          </button>
          <div className="auth-switch">
            <span onClick={() => { setEtapa('email'); setErro(''); }}>← Corrigir o e-mail</span>
          </div>
        </form>
      )}
      {etapa === 'senha' && (
        <form className="auth-form" onSubmit={redefinir}>
          <div className="field">
            <label>Nova senha</label>
            <input type="password" value={novaSenha} onChange={(e) => setNovaSenha(e.target.value)} placeholder="Mínimo 6 caracteres" />
          </div>
          <div className="field">
            <label>Confirmar nova senha</label>
            <input type="password" value={novaSenha2} onChange={(e) => setNovaSenha2(e.target.value)} placeholder="Repita a senha" />
          </div>
          {erro && <div className="form-error">{erro}</div>}
          <button className="btn btn-primary" type="submit" disabled={carregando}>
            {carregando ? 'Salvando...' : 'Salvar nova senha'}
          </button>
        </form>
      )}
      {etapa === 'pronto' && (
        <div className="success-wrap">
          <div className="success-mark">✓</div>
          <div className="success-title">Senha alterada</div>
          <div className="success-sub">Agora é só entrar com a sua nova senha.</div>
          <button className="btn btn-primary" onClick={onVoltar}>Ir para o login</button>
        </div>
      )}
    </div>
  );
}
