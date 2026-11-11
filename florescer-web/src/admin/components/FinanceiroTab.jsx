import { useState, useEffect, useCallback } from 'react';
import ConcluirModal from './ConcluirModal.jsx';
import { financeiro } from '../../services/api.js';

const PERIODOS = [
  { chave: 'dia', rotulo: 'Hoje' },
  { chave: 'semana', rotulo: 'Semana' },
  { chave: 'mes', rotulo: 'Mês' },
  { chave: 'ano', rotulo: 'Ano' },
];

function moeda(valor) {
  return (valor ?? 0).toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' });
}

function moedaCurta(valor) {
  if (valor >= 1000) return `R$ ${(valor / 1000).toFixed(1).replace('.', ',')}k`;
  return `R$ ${Math.round(valor)}`;
}

function hojeTexto() {
  const d = new Date();
  return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`;
}

export default function FinanceiroTab() {
  const [periodo, setPeriodo] = useState('mes');
  const [de, setDe] = useState(hojeTexto());
  const [ate, setAte] = useState(hojeTexto());
  const [resumo, setResumo] = useState(null);
  const [aFechar, setAFechar] = useState([]);
  const [concluindo, setConcluindo] = useState(null);
  const [erro, setErro] = useState('');

  const carregar = useCallback(async () => {
    try {
      const consulta = periodo === 'personalizado'
        ? financeiro.resumoPorIntervalo(de, ate)
        : financeiro.resumo(periodo);

      const [dados, pendentes] = await Promise.all([consulta, financeiro.aFechar()]);
      setResumo(dados);
      setAFechar(pendentes);
      setErro('');
    } catch (err) {
      setErro(err.message);
    }
  }, [periodo, de, ate]);

  useEffect(() => { carregar(); }, [carregar]);

  if (erro) return <div className="tab-panel active"><div className="form-error">{erro}</div></div>;
  if (!resumo) return <div className="tab-panel active"><div className="page-date">Carregando dados financeiros...</div></div>;

  const variacaoPositiva = resumo.variacaoPercentual >= 0;
  const maiorPonto = Math.max(1, ...resumo.serie.map((p) => p.valor));

  const serieDensa = resumo.serie.length > 12;

  const intervalo = `${new Date(resumo.inicio).toLocaleDateString('pt-BR')} a ${new Date(resumo.fim).toLocaleDateString('pt-BR')}`;

  return (
    <div className="tab-panel active">
      <div className="topbar">
        <div>
          <div className="page-title">Financeiro</div>
          <div className="page-date">{intervalo}</div>
        </div>
      </div>

      <div className="filter-bar">
        {PERIODOS.map((p) => (
          <div
            key={p.chave}
            className={`chip ${periodo === p.chave ? 'active' : ''}`}
            onClick={() => setPeriodo(p.chave)}
          >
            {p.rotulo}
          </div>
        ))}
        <div
          className={`chip ${periodo === 'personalizado' ? 'active' : ''}`}
          onClick={() => setPeriodo('personalizado')}
        >
          Escolher período
        </div>

        {periodo === 'personalizado' && (
          <div className="intervalo-livre">
            <label>
              de
              <input type="date" value={de} max={ate} onChange={(e) => setDe(e.target.value)} />
            </label>
            <label>
              até
              <input type="date" value={ate} min={de} onChange={(e) => setAte(e.target.value)} />
            </label>
          </div>
        )}
      </div>

      {}
      {aFechar.length > 0 && (
        <div className="fechar-card">
          <div className="fechar-topo">
            <div>
              <div className="fechar-titulo">{aFechar.length} atendimento(s) aguardando baixa</div>
              <div className="fechar-sub">
                Já aconteceram, mas ainda não entraram no faturamento. Registre a baixa para o relatório ficar completo.
              </div>
            </div>
          </div>
          <div className="fechar-lista">
            {aFechar.slice(0, 6).map((a) => (
              <div className="fechar-item" key={a.agendamentoId}>
                <div>
                  <div className="fechar-cliente">{a.clienteNome}</div>
                  <div className="fechar-detalhe">
                    {a.procedimentoNome} · {a.profissionalNome} ·{' '}
                    {new Date(a.inicio).toLocaleDateString('pt-BR')} às{' '}
                    {new Date(a.inicio).toLocaleTimeString('pt-BR', { hour: '2-digit', minute: '2-digit' })}
                  </div>
                </div>
                <button
                  className="icon-btn"
                  onClick={() => setConcluindo({
                    id: a.agendamentoId,
                    clienteNome: a.clienteNome,
                    procedimentoNome: a.procedimentoNome,
                    profissionalNome: a.profissionalNome,
                    valorPrevisto: a.valorPrevisto,
                    dataLabel: new Date(a.inicio).toLocaleDateString('pt-BR'),
                    hora: new Date(a.inicio).toLocaleTimeString('pt-BR', { hour: '2-digit', minute: '2-digit' }),
                  })}
                >
                  Dar baixa · {moeda(a.valorPrevisto)}
                </button>
              </div>
            ))}
          </div>
        </div>
      )}
      <div className="stat-grid">
        <div className="stat-card">
          <div className="stat-label">Receita do período</div>
          <div className="stat-value">{moeda(resumo.receita)}</div>
          <div className={`stat-delta ${variacaoPositiva ? 'up' : 'down'}`}>
            {variacaoPositiva ? '↑' : '↓'} {Math.abs(resumo.variacaoPercentual)}% vs. período anterior
          </div>
        </div>
        <div className="stat-card">
          <div className="stat-label">Atendimentos concluídos</div>
          <div className="stat-value">{resumo.atendimentosConcluidos}</div>
          <div className="stat-delta">no período selecionado</div>
        </div>
        <div className="stat-card">
          <div className="stat-label">Ticket médio</div>
          <div className="stat-value">{moeda(resumo.ticketMedio)}</div>
          <div className="stat-delta">receita ÷ atendimentos</div>
        </div>
        <div className="stat-card">
          <div className="stat-label">Receita já garantida</div>
          <div className="stat-value">{moeda(resumo.receitaPrevista)}</div>
          <div className="stat-delta">agendamentos confirmados à frente</div>
        </div>
      </div>
      {}
      <div className="panel-card">
        <div className="panel-title">Evolução da receita</div>
        <div className="grafico">
          {resumo.serie.map((ponto) => (
            <div className="grafico-coluna" key={ponto.rotulo + ponto.data}>
              <div className="grafico-valor">
                {ponto.valor > 0 && (!serieDensa || ponto.valor === maiorPonto)
                  ? moedaCurta(ponto.valor)
                  : ''}
              </div>
              <div className="grafico-barra-area">
                <div
                  className="grafico-barra"
                  style={{ height: `${(ponto.valor / maiorPonto) * 100}%` }}
                  title={`${ponto.rotulo}: ${moeda(ponto.valor)} (${ponto.quantidade} atendimentos)`}
                />
              </div>
              <div className="grafico-rotulo">{ponto.rotulo}</div>
            </div>
          ))}
        </div>
      </div>
      <div className="grid-2">
        <div className="panel-card">
          <div className="panel-title">Receita por categoria</div>
          {resumo.porCategoria.length === 0 && <div className="vazio">Sem receita registrada no período.</div>}
          {resumo.porCategoria.map((c) => (
            <div className="bar-row" key={c.slug}>
              <div className="bar-label">{c.nome}</div>
              <div className="bar-track">
                <div className="bar-fill" style={{ width: `${c.percentual}%`, background: c.cor }} />
              </div>
              {}
              <div className="bar-valor">
                <span className="bar-valor-reais">{moeda(c.valor)}</span>
                <span className="bar-valor-qtd">{c.quantidade} atend.</span>
              </div>
            </div>
          ))}
        </div>
        <div className="panel-card">
          <div className="panel-title">Indicadores de perda</div>
          <div className="mini-appt">
            <div>
              <div className="who">Taxa de cancelamento</div>
              <div className="what">agendamentos cancelados no período</div>
            </div>
            <div className="when">{resumo.taxaDeCancelamento}%</div>
          </div>
          <div className="mini-appt">
            <div>
              <div className="who">Perda por falta de confirmação</div>
              <div className="what">horários que a cliente esqueceu de confirmar</div>
            </div>
            <div className="when">{moeda(resumo.perdaPorFaltaDeConfirmacao)}</div>
          </div>
          <div className="mini-appt">
            <div>
              <div className="who">Aguardando baixa</div>
              <div className="what">atendimentos realizados sem lançamento</div>
            </div>
            <div className="when">{resumo.atendimentosAFechar}</div>
          </div>
        </div>
      </div>
      <div className="grid-2">
        <div className="panel-card">
          <div className="panel-title">Receita por profissional</div>
          <table className="mini-table">
            <thead>
              <tr><th>Profissional</th><th>Atend.</th><th>Ticket</th><th>Total</th></tr>
            </thead>
            <tbody>
              {resumo.porProfissional.length === 0 && (
                <tr><td colSpan="4" className="tabela-vazia">Sem dados no período.</td></tr>
              )}
              {resumo.porProfissional.map((p) => (
                <tr key={p.profissionalId}>
                  <td>
                    {p.nome}
                    <div className="cel-sub">{p.especialidade}</div>
                  </td>
                  <td>{p.atendimentos}</td>
                  <td>{moeda(p.ticketMedio)}</td>
                  <td className="valor-forte">{moeda(p.total)}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
        <div className="panel-card">
          <div className="panel-title">Procedimentos que mais faturam</div>
          <table className="mini-table">
            <thead>
              <tr><th>Procedimento</th><th>Atend.</th><th>Total</th></tr>
            </thead>
            <tbody>
              {resumo.topProcedimentos.length === 0 && (
                <tr><td colSpan="3" className="tabela-vazia">Sem dados no período.</td></tr>
              )}
              {resumo.topProcedimentos.map((p) => (
                <tr key={p.procedimentoId}>
                  <td>{p.nome}</td>
                  <td>{p.atendimentos}</td>
                  <td className="valor-forte">{moeda(p.total)}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>
      {concluindo && (
        <ConcluirModal
          agendamento={concluindo}
          onFechar={() => setConcluindo(null)}
          onConcluido={async () => { setConcluindo(null); await carregar(); }}
        />
      )}
    </div>
  );
}
