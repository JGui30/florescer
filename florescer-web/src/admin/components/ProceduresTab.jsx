import { useState, useEffect, useCallback } from 'react';
import ProcedureModal from './ProcedureModal.jsx';
import CategoryModal from './CategoryModal.jsx';
import { catalogo } from '../../services/api.js';

export default function ProceduresTab() {
  const [categorias, setCategorias] = useState([]);
  const [editando, setEditando] = useState(null);
  const [categoriaEmEdicao, setCategoriaEmEdicao] = useState(null);
  const [erro, setErro] = useState('');

  const carregar = useCallback(async () => {
    try {
      setCategorias(await catalogo.todasAsCategorias());
      setErro('');
    } catch (err) {
      setErro(err.message);
    }
  }, []);

  useEffect(() => { carregar(); }, [carregar]);

  async function desativar(procedimento) {
    const confirmado = window.confirm(
      `Desativar "${procedimento.nome}"?\n\nEle deixa de aparecer para as clientes, mas o histórico de atendimentos é preservado.`
    );
    if (!confirmado) return;

    try {
      await catalogo.desativarProcedimento(procedimento.id);
      await carregar();
    } catch (err) {
      setErro(err.message);
    }
  }

  async function desativarCategoria(cat) {
    const confirmado = window.confirm(
      `Desativar a categoria "${cat.nome}"?

` +
      `Os ${cat.procedimentos.length} procedimentos dela também saem da vitrine. ` +
      'O histórico de atendimentos é preservado.'
    );
    if (!confirmado) return;

    try {
      await catalogo.desativarCategoria(cat.id);
      await carregar();
    } catch (err) {
      setErro(err.message);
    }
  }

  return (
    <div className="tab-panel active">
      <div className="topbar">
        <div>
          <div className="page-title">Procedimentos</div>
          <div className="page-date">Catálogo por categoria</div>
        </div>
        <div style={{ display: 'flex', gap: 8 }}>
          <button className="icon-btn" onClick={() => setCategoriaEmEdicao({ nova: true })}>
            + Nova categoria
          </button>
          <button className="icon-btn" onClick={() => setEditando({ novo: true })}>
            + Novo procedimento
          </button>
        </div>
      </div>

      {erro && <div className="form-error">{erro}</div>}

      <div>
        {categorias.map((cat) => (
          <div key={cat.id}>
            <div className="cat-header">
              <span className="cat-dot" style={{ background: cat.cor }} />
              {cat.nome}
              <span className="cat-header-acoes">
                <span className="acao-link" onClick={() => setCategoriaEmEdicao(cat)}>editar</span>
                <span className="acao-link" onClick={() => desativarCategoria(cat)}>desativar</span>
              </span>
            </div>
            <table className="appt-table" style={{ marginBottom: 8 }}>
              <thead>
                <tr><th>Procedimento</th><th>Duração</th><th>Preço</th><th></th></tr>
              </thead>
              <tbody>
                {cat.procedimentos.length === 0 && (
                  <tr><td colSpan="4" className="tabela-vazia">Nenhum procedimento nesta categoria.</td></tr>
                )}
                {cat.procedimentos.map((p) => (
                  <tr key={p.id} className={p.ativo ? '' : 'linha-inativa'}>
                    <td>
                      {p.nome}
                      {!p.ativo && <span className="tag-inativo">inativo</span>}
                    </td>
                    <td className="appt-time">{p.duracaoMinutos} min</td>
                    <td className="appt-time">R$ {p.preco.toFixed(2).replace('.', ',')}</td>
                    <td style={{ textAlign: 'right' }}>
                      <button className="icon-btn" onClick={() => setEditando({ ...p, categoriaId: cat.id })}>
                        Editar
                      </button>{' '}
                      {p.ativo && (
                        <button className="icon-btn danger" onClick={() => desativar(p)}>Desativar</button>
                      )}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        ))}
      </div>

      {categoriaEmEdicao && (
        <CategoryModal
          categoria={categoriaEmEdicao.nova ? null : categoriaEmEdicao}
          onFechar={() => setCategoriaEmEdicao(null)}
          onSalvo={async () => { setCategoriaEmEdicao(null); await carregar(); }}
        />
      )}

      {editando && (
        <ProcedureModal
          procedimento={editando.novo ? null : editando}
          categorias={categorias}
          onFechar={() => setEditando(null)}
          onSalvo={async () => { setEditando(null); await carregar(); }}
        />
      )}
    </div>
  );
}
