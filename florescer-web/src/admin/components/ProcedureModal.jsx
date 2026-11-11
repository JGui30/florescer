import { useState } from 'react';
import { catalogo } from '../../services/api.js';

export default function ProcedureModal({ procedimento, categorias, onFechar, onSalvo }) {
  const editando = Boolean(procedimento);

  const [nome, setNome] = useState(procedimento?.nome || '');
  const [descricao, setDescricao] = useState(procedimento?.descricao || '');
  const [categoriaId, setCategoriaId] = useState(procedimento?.categoriaId || categorias[0]?.id || '');
  const [duracao, setDuracao] = useState(procedimento?.duracaoMinutos || 60);
  const [preco, setPreco] = useState(procedimento?.preco?.toFixed(2) || '0.00');
  const [ativo, setAtivo] = useState(procedimento?.ativo ?? true);

  const [erro, setErro] = useState('');
  const [salvando, setSalvando] = useState(false);

  async function salvar(e) {
    e.preventDefault();
    setErro('');
    setSalvando(true);

    const dados = {
      nome,
      descricao: descricao || null,
      categoriaId: Number(categoriaId),
      duracaoMinutos: Number(duracao),
      preco: Number(String(preco).replace(',', '.')),
      ativo,
    };

    try {
      if (editando) await catalogo.atualizarProcedimento(procedimento.id, dados);
      else await catalogo.criarProcedimento(dados);

      await onSalvo();
    } catch (err) {
      setErro(err.message);
      setSalvando(false);
    }
  }

  return (
    <div className="modal-overlay open">
      <div className="modal-box admin-modal">
        <h3>{editando ? 'Editar procedimento' : 'Novo procedimento'}</h3>

        <form onSubmit={salvar}>
          <div className="field">
            <label>Nome</label>
            <input value={nome} onChange={(e) => setNome(e.target.value)} placeholder="Ex.: Hidratação profunda" />
          </div>

          <div className="field">
            <label>Categoria</label>
            <select value={categoriaId} onChange={(e) => setCategoriaId(e.target.value)}>
              {categorias.map((c) => (
                <option key={c.id} value={c.id}>{c.nome}</option>
              ))}
            </select>
          </div>

          <div className="campo-duplo">
            <div className="field">
              <label>Duração (minutos)</label>
              <input type="number" min="5" step="5" value={duracao} onChange={(e) => setDuracao(e.target.value)} />
            </div>
            <div className="field">
              <label>Preço (R$)</label>
              <input value={preco} onChange={(e) => setPreco(e.target.value)} inputMode="decimal" />
            </div>
          </div>

          <div className="field">
            <label>Descrição (opcional)</label>
            <textarea
              rows="2"
              value={descricao}
              onChange={(e) => setDescricao(e.target.value)}
              placeholder="Aparece na vitrine e ajuda a cliente a escolher"
            />
          </div>

          <label className="check-row">
            <input type="checkbox" checked={ativo} onChange={(e) => setAtivo(e.target.checked)} />
            Disponível para agendamento
          </label>

          {erro && <div className="form-error">{erro}</div>}

          <div className="modal-close-row">
            <button type="button" className="btn-ghost" style={{ width: 'auto', flexShrink: 0 }} onClick={onFechar}>
              Cancelar
            </button>
            <button className="btn btn-primary" type="submit" disabled={salvando}>
              {salvando ? 'Salvando...' : 'Salvar'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
