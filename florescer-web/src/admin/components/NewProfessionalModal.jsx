import { useState, useEffect } from 'react';
import { profissionais, catalogo } from '../../services/api.js';

const DIAS = ['Segunda', 'Terça', 'Quarta', 'Quinta', 'Sexta', 'Sábado', 'Domingo'];

export default function NewProfessionalModal({ profissional, onClose, onSalvo }) {
  const editando = Boolean(profissional);

  const [nome, setNome] = useState(profissional?.nome || '');
  const [especialidade, setEspecialidade] = useState(profissional?.especialidade || '');
  const [email, setEmail] = useState(profissional?.email || '');
  const [telefone, setTelefone] = useState(profissional?.telefone || '');
  const [ativa, setAtiva] = useState(profissional?.ativa ?? true);
  const [dias, setDias] = useState(profissional?.dias || [1, 1, 1, 1, 1, 0, 0]);
  const [selecionados, setSelecionados] = useState(profissional?.procedimentoIds || []);
  const [horaInicio, setHoraInicio] = useState('09:00');
  const [horaFim, setHoraFim] = useState('18:00');

  const [categorias, setCategorias] = useState([]);
  const [erro, setErro] = useState('');
  const [salvando, setSalvando] = useState(false);

  useEffect(() => {
    catalogo.todasAsCategorias().then(setCategorias).catch(() => setCategorias([]));
  }, []);

  function alternarDia(indice) {
    setDias((atual) => atual.map((d, i) => (i === indice ? (d ? 0 : 1) : d)));
  }

  function alternarProcedimento(id) {
    setSelecionados((atual) =>
      atual.includes(id) ? atual.filter((x) => x !== id) : [...atual, id]
    );
  }

  function alternarCategoria(categoria) {
    const ids = categoria.procedimentos.map((p) => p.id);
    const todosMarcados = ids.every((id) => selecionados.includes(id));

    setSelecionados((atual) =>
      todosMarcados
        ? atual.filter((id) => !ids.includes(id))
        : [...new Set([...atual, ...ids])]
    );
  }

  async function salvar(e) {
    e.preventDefault();
    setErro('');
    setSalvando(true);

    const dados = {
      nome,
      especialidade,
      email: email || null,
      telefone: telefone || null,
      ativa,
      dias,
      procedimentoIds: selecionados,
      horaInicio,
      horaFim,
    };

    try {
      if (editando) await profissionais.atualizar(profissional.id, dados);
      else await profissionais.criar(dados);

      await onSalvo();
    } catch (err) {
      setErro(err.message);
      setSalvando(false);
    }
  }

  return (
    <div className="modal-overlay open">
      <div className="modal-box admin-modal largo">
        <h3>{editando ? 'Editar profissional' : 'Nova profissional'}</h3>

        <form onSubmit={salvar}>
          <div className="campo-duplo">
            <div className="field">
              <label>Nome</label>
              <input value={nome} onChange={(e) => setNome(e.target.value)} placeholder="Nome completo" />
            </div>
            <div className="field">
              <label>Especialidade</label>
              <input
                value={especialidade}
                onChange={(e) => setEspecialidade(e.target.value)}
                placeholder="Ex.: Cabeleireira"
              />
            </div>
          </div>

          <div className="campo-duplo">
            <div className="field">
              <label>E-mail</label>
              <input type="email" value={email} onChange={(e) => setEmail(e.target.value)} />
            </div>
            <div className="field">
              <label>Telefone</label>
              <input value={telefone} onChange={(e) => setTelefone(e.target.value)} placeholder="(11) 90000-0000" />
            </div>
          </div>

          <div className="field">
            <label>Dias de trabalho</label>
            <div className="dias-selecao">
              {DIAS.map((d, i) => (
                <div
                  key={d}
                  className={`dia-toggle ${dias[i] ? 'on' : ''}`}
                  onClick={() => alternarDia(i)}
                >
                  {d.slice(0, 3)}
                </div>
              ))}
            </div>
          </div>

          <div className="campo-duplo">
            <div className="field">
              <label>Início do expediente</label>
              <input type="time" value={horaInicio} onChange={(e) => setHoraInicio(e.target.value)} />
            </div>
            <div className="field">
              <label>Fim do expediente</label>
              <input type="time" value={horaFim} onChange={(e) => setHoraFim(e.target.value)} />
            </div>
          </div>

          <div className="field">
            <label>Procedimentos que realiza ({selecionados.length} selecionados)</label>
            <div className="proc-selecao">
              {categorias.map((cat) => (
                <div key={cat.id} className="proc-categoria">
                  <div className="proc-cat-titulo" onClick={() => alternarCategoria(cat)}>
                    <span className="cat-dot" style={{ background: cat.cor }} />
                    {cat.nome}
                    <span className="proc-cat-acao">marcar todos</span>
                  </div>
                  <div className="proc-chips">
                    {cat.procedimentos.map((p) => (
                      <div
                        key={p.id}
                        className={`proc-chip ${selecionados.includes(p.id) ? 'on' : ''}`}
                        onClick={() => alternarProcedimento(p.id)}
                      >
                        {p.nome}
                      </div>
                    ))}
                  </div>
                </div>
              ))}
            </div>
          </div>

          <label className="check-row">
            <input type="checkbox" checked={ativa} onChange={(e) => setAtiva(e.target.checked)} />
            Recebendo agendamentos
          </label>

          {erro && <div className="form-error">{erro}</div>}

          <div className="modal-close-row">
            <button type="button" className="btn-ghost" style={{ width: 'auto', flexShrink: 0 }} onClick={onClose}>
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
