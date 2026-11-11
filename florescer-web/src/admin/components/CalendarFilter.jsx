import { useState, useEffect } from 'react';
import { agendamentos } from '../../services/api.js';

const MESES = ['Janeiro', 'Fevereiro', 'Março', 'Abril', 'Maio', 'Junho',
  'Julho', 'Agosto', 'Setembro', 'Outubro', 'Novembro', 'Dezembro'];

const DIAS_SEMANA = ['D', 'S', 'T', 'Q', 'Q', 'S', 'S'];

function paraTexto(data) {
  return `${data.getFullYear()}-${String(data.getMonth() + 1).padStart(2, '0')}-${String(data.getDate()).padStart(2, '0')}`;
}

export default function CalendarFilter({ dataSelecionada, onSelecionarData, profissionalId }) {
  const hoje = new Date();
  const [mesVisivel, setMesVisivel] = useState(new Date(hoje.getFullYear(), hoje.getMonth(), 1));
  const [movimento, setMovimento] = useState([]);
  const [limites, setLimites] = useState(null);

  useEffect(() => {
    agendamentos.periodoDaAgenda().then(setLimites).catch(() => setLimites(null));
  }, []);

  useEffect(() => {
    const primeiroDia = new Date(mesVisivel.getFullYear(), mesVisivel.getMonth(), 1);
    const ultimoDia = new Date(mesVisivel.getFullYear(), mesVisivel.getMonth() + 1, 0);

    agendamentos
      .diasComMovimento(paraTexto(primeiroDia), paraTexto(ultimoDia), profissionalId || null)
      .then(setMovimento)
      .catch(() => setMovimento([]));
  }, [mesVisivel, profissionalId]);

  function mudarMes(passos) {
    setMesVisivel(new Date(mesVisivel.getFullYear(), mesVisivel.getMonth() + passos, 1));
  }

  const podeVoltar = () => {
    if (!limites?.primeiroAgendamento) return false;
    const primeiro = new Date(limites.primeiroAgendamento);
    const anterior = new Date(mesVisivel.getFullYear(), mesVisivel.getMonth() - 1, 1);
    return anterior >= new Date(primeiro.getFullYear(), primeiro.getMonth(), 1);
  };

  const podeAvancar = () => {
    if (!limites?.ultimoAgendamento) return false;
    const ultimo = new Date(limites.ultimoAgendamento);
    const proximo = new Date(mesVisivel.getFullYear(), mesVisivel.getMonth() + 1, 1);
    return proximo <= new Date(ultimo.getFullYear(), ultimo.getMonth(), 1);
  };

  const primeiroDiaDoMes = new Date(mesVisivel.getFullYear(), mesVisivel.getMonth(), 1);
  const diasNoMes = new Date(mesVisivel.getFullYear(), mesVisivel.getMonth() + 1, 0).getDate();
  const celulas = [];

  for (let i = 0; i < primeiroDiaDoMes.getDay(); i++) celulas.push(null);
  for (let dia = 1; dia <= diasNoMes; dia++) celulas.push(dia);

  const maiorMovimento = Math.max(1, ...movimento.map((m) => m.total));

  return (
    <div className="calendario">
      <div className="cal-topo">
        <button className="cal-nav" onClick={() => mudarMes(-1)} disabled={!podeVoltar()}>‹</button>
        <div className="cal-mes">{MESES[mesVisivel.getMonth()]} {mesVisivel.getFullYear()}</div>
        <button className="cal-nav" onClick={() => mudarMes(1)} disabled={!podeAvancar()}>›</button>
      </div>

      <div className="cal-grade">
        {DIAS_SEMANA.map((d, i) => (
          <div className="cal-cabecalho" key={i}>{d}</div>
        ))}

        {celulas.map((dia, i) => {
          if (dia === null) return <div key={`vazio-${i}`} />;

          const data = new Date(mesVisivel.getFullYear(), mesVisivel.getMonth(), dia);
          const texto = paraTexto(data);
          const doDia = movimento.find((m) => m.data.startsWith(texto));
          const total = doDia?.total || 0;

          const ehHoje = paraTexto(hoje) === texto;
          const selecionado = dataSelecionada === texto;

          const intensidade = total === 0 ? 0 : 0.2 + (total / maiorMovimento) * 0.8;

          return (
            <div
              key={texto}
              className={`cal-dia ${selecionado ? 'selecionado' : ''} ${ehHoje ? 'hoje' : ''} ${total ? 'com-movimento' : ''}`}
              style={total && !selecionado ? { background: `rgba(194, 38, 86, ${intensidade * 0.22})` } : undefined}
              onClick={() => onSelecionarData(selecionado ? null : texto)}
              title={total ? `${total} atendimento(s)` : 'Sem agendamentos'}
            >
              <span className="cal-num">{dia}</span>
              {total > 0 && <span className="cal-badge">{total}</span>}
            </div>
          );
        })}
      </div>

      <div className="cal-rodape">
        {dataSelecionada ? (
          <button className="cal-limpar" onClick={() => onSelecionarData(null)}>Limpar filtro de data</button>
        ) : (
          <span className="cal-dica">Clique em um dia para filtrar a agenda</span>
        )}
      </div>
    </div>
  );
}
