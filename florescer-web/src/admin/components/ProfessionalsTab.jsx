import { useState, useEffect, useCallback } from 'react';
import NewProfessionalModal from './NewProfessionalModal.jsx';
import { profissionais } from '../../services/api.js';

const DIAS = ['S', 'T', 'Q', 'Q', 'S', 'S', 'D'];

export default function ProfessionalsTab() {
  const [equipe, setEquipe] = useState([]);
  const [editando, setEditando] = useState(null);
  const [erro, setErro] = useState('');

  const carregar = useCallback(async () => {
    try {
      setEquipe(await profissionais.listar());
      setErro('');
    } catch (err) {
      setErro(err.message);
    }
  }, []);

  useEffect(() => { carregar(); }, [carregar]);

  async function desativar(p) {
    const confirmado = window.confirm(
      `Desativar ${p.nome}?\n\nEla deixa de receber novos agendamentos, mas o histórico de atendimentos é preservado.`
    );
    if (!confirmado) return;

    try {
      await profissionais.desativar(p.id);
      await carregar();
    } catch (err) {
      setErro(err.message);
    }
  }

  return (
    <div className="tab-panel active">
      <div className="topbar">
        <div>
          <div className="page-title">Profissionais</div>
          <div className="page-date">Equipe cadastrada</div>
        </div>
      </div>

      {erro && <div className="form-error">{erro}</div>}

      <div className="pro-grid">
        {equipe.map((p) => (
          <div className={`pro-card ${p.ativa ? '' : 'inativa'}`} key={p.id}>
            <div className="pro-top">
              <div className="pro-avatar-lg">{p.nome.charAt(0)}</div>
              <div>
                <div className="pro-name">
                  {p.nome}
                  {!p.ativa && <span className="tag-inativo">inativa</span>}
                </div>
                <div className="pro-role">{p.especialidade}</div>
              </div>
            </div>

            <div className="pro-days">
              {p.dias.map((d, di) => (
                <div className={`day-dot ${d ? 'on' : ''}`} key={di}>{DIAS[di]}</div>
              ))}
            </div>

            <div className="pro-servicos">{p.procedimentoIds.length} procedimentos habilitados</div>

            <div className="pro-actions">
              <button className="icon-btn" onClick={() => setEditando(p)}>Editar</button>
              {p.ativa && (
                <button className="icon-btn danger" onClick={() => desativar(p)}>Desativar</button>
              )}
            </div>
          </div>
        ))}

        <div className="add-card" onClick={() => setEditando({ nova: true })}>
          <div className="add-plus">+</div>Nova profissional
        </div>
      </div>

      {editando && (
        <NewProfessionalModal
          profissional={editando.nova ? null : editando}
          onClose={() => setEditando(null)}
          onSalvo={async () => { setEditando(null); await carregar(); }}
        />
      )}
    </div>
  );
}
