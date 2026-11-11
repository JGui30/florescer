import { useState } from 'react';
import { catalogo } from '../../services/api.js';

const CORES_SUGERIDAS = [
  '#E8306E', '#B5540E', '#128F5D', '#E8A93C',
  '#8C1440', '#1E9C74', '#7A4E08', '#5E0E2B',
];

export default function CategoryModal({ categoria, onFechar, onSalvo }) {
  const editando = Boolean(categoria);

  const [nome, setNome] = useState(categoria?.nome || '');
  const [cor, setCor] = useState(categoria?.cor || CORES_SUGERIDAS[0]);
  const [ordem, setOrdem] = useState(categoria?.ordem ?? 99);
  const [ativa, setAtiva] = useState(categoria?.ativa ?? true);

  const [erro, setErro] = useState('');
  const [salvando, setSalvando] = useState(false);

  async function salvar(e) {
    e.preventDefault();
    setErro('');
    setSalvando(true);

    const dados = { nome, cor, ordem: Number(ordem), ativa };

    try {
      if (editando) await catalogo.atualizarCategoria(categoria.id, dados);
      else await catalogo.criarCategoria(dados);

      await onSalvo();
    } catch (err) {
      setErro(err.message);
      setSalvando(false);
    }
  }

  return (
    <div className="modal-overlay open">
      <div className="modal-box admin-modal">
        <h3>{editando ? 'Editar categoria' : 'Nova categoria'}</h3>

        <form onSubmit={salvar}>
          <div className="field">
            <label>Nome</label>
            <input
              value={nome}
              onChange={(e) => setNome(e.target.value)}
              placeholder="Ex.: Sobrancelhas"
              autoFocus
            />
          </div>

          <div className="field">
            <label>Cor de identificação</label>
            <div className="campo-ajuda">
              Usada nos gráficos do painel e no círculo que a cliente vê quando
              a categoria ainda não tem foto.
            </div>
            <div className="cor-escolha">
              {CORES_SUGERIDAS.map((c) => (
                <button
                  type="button"
                  key={c}
                  className={`cor-opcao ${cor.toLowerCase() === c.toLowerCase() ? 'ativa' : ''}`}
                  style={{ background: c }}
                  onClick={() => setCor(c)}
                  title={c}
                />
              ))}
              <input
                type="color"
                className="cor-livre"
                value={cor}
                onChange={(e) => setCor(e.target.value)}
                title="Escolher outra cor"
              />
            </div>
          </div>

          <div className="field">
            <label>Ordem de exibição</label>
            <div className="campo-ajuda">
              Menor número aparece primeiro, para a cliente e no painel.
            </div>
            <input
              type="number"
              min="1"
              value={ordem}
              onChange={(e) => setOrdem(e.target.value)}
            />
          </div>

          <label className="check-row">
            <input type="checkbox" checked={ativa} onChange={(e) => setAtiva(e.target.checked)} />
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
