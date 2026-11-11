import { useState, useEffect } from 'react';
import { profissionais } from '../../../services/api.js';

const DIAS_CURTOS = ['seg', 'ter', 'qua', 'qui', 'sex', 'sáb', 'dom'];

function textoDosDias(dias) {
  if (!Array.isArray(dias) || dias.length !== 7) return '';

  const indices = dias.map((d, i) => (d === 1 ? i : -1)).filter((i) => i >= 0);
  if (indices.length === 0) return '';
  if (indices.length === 7) return 'Atende todos os dias';

  const partes = [];
  let inicio = indices[0];
  let anterior = indices[0];

  const fecharGrupo = () => {
    if (anterior - inicio + 1 >= 3) {
      partes.push(`${DIAS_CURTOS[inicio]} a ${DIAS_CURTOS[anterior]}`);
      return;
    }

    for (let i = inicio; i <= anterior; i++) partes.push(DIAS_CURTOS[i]);
  };

  for (const atual of indices.slice(1)) {
    if (atual !== anterior + 1) {
      fecharGrupo();
      inicio = atual;
    }
    anterior = atual;
  }
  fecharGrupo();

  if (partes.length === 1) return `Atende ${partes[0]}`;

  return `Atende ${partes.slice(0, -1).join(', ')} e ${partes[partes.length - 1]}`;
}

function textoDoHistorico(p) {
  const quando = p.ultimoAtendimento
    ? new Date(p.ultimoAtendimento).toLocaleDateString('pt-BR', { month: 'long', year: 'numeric' })
    : null;

  if (p.atendimentosAnteriores > 1) {
    return `Já atendeu você ${p.atendimentosAnteriores} vezes`;
  }

  return quando ? `Já atendeu você em ${quando}` : 'Já atendeu você';
}

export default function ProfessionalStep({ procedimento, onSelect, onBack }) {
  const [lista, setLista] = useState([]);
  const [carregando, setCarregando] = useState(true);
  const [erro, setErro] = useState('');

  useEffect(() => {
    profissionais
      .queRealizam(procedimento.id)
      .then(setLista)
      .catch((err) => setErro(err.message))
      .finally(() => setCarregando(false));
  }, [procedimento.id]);

  const temRecomendacao = lista.some((p) => p.jaAtendeu);

  return (
    <div className="step-panel active">
      <h2 className="step-title">Escolha a profissional</h2>
      <p className="step-sub">
        {temRecomendacao
          ? 'Quem já cuidou de você aparece primeiro'
          : `Disponíveis para ${procedimento.nome}`}
      </p>

      {carregando && <p className="step-sub">Carregando...</p>}
      {erro && <div className="form-error">{erro}</div>}

      {!carregando && lista.length === 0 && (
        <p className="step-sub">Nenhuma profissional disponível para este procedimento no momento.</p>
      )}

      <div>
        {lista.map((p) => {
          const escala = textoDosDias(p.dias);

          return (
            <div
              className={`list-item ${p.jaAtendeu ? 'recomendada' : ''}`}
              key={p.id}
              onClick={() => onSelect(p)}
            >
              <div className="li-main" style={{ flexDirection: 'row', alignItems: 'center', gap: 10 }}>
                <div className="pro-avatar">{p.nome.charAt(0)}</div>
                <div>
                  <div className="li-title">{p.nome}</div>
                  <div className="li-meta">{p.especialidade}</div>
                  {escala && <div className="li-escala">{escala}</div>}
                  {p.jaAtendeu && (
                    <div className="selo-recomendada">★ {textoDoHistorico(p)}</div>
                  )}
                </div>
              </div>
              <span style={{ color: 'var(--gold)', fontSize: 18 }}>›</span>
            </div>
          );
        })}
      </div>

      <div className="nav-row">
        <button className="btn-ghost" onClick={onBack}>← Voltar</button>
      </div>
    </div>
  );
}
