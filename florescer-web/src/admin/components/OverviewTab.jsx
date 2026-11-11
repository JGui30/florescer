import { useState, useEffect } from 'react';
import { painel } from '../../services/api.js';

function formatarMoeda(valor) {
  return valor.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL', maximumFractionDigits: 0 });
}

function comparar(atual, anterior, sufixo = '') {
  const diferenca = atual - anterior;

  if (diferenca === 0) return { texto: 'igual a ontem', classe: '' };

  const seta = diferenca > 0 ? '↑' : '↓';
  const valor = Math.abs(Number(diferenca.toFixed(1)));

  return { texto: `${seta} ${valor}${sufixo} vs. ontem`, classe: diferenca > 0 ? 'up' : 'down' };
}

function compararEmDinheiro(atual, anterior) {
  if (anterior === 0 && atual === 0) return { texto: 'igual a ontem', classe: '' };
  if (anterior === 0) return { texto: 'primeiro faturamento do período', classe: 'up' };

  const variacao = ((atual - anterior) / anterior) * 100;

  if (Math.round(variacao) === 0) return { texto: 'estável em relação a ontem', classe: '' };

  const seta = variacao > 0 ? '↑' : '↓';
  return { texto: `${seta} ${Math.abs(variacao).toFixed(0)}% vs. ontem`, classe: variacao > 0 ? 'up' : 'down' };
}

export default function OverviewTab() {
  const [dados, setDados] = useState(null);
  const [erro, setErro] = useState('');

  useEffect(() => {
    painel.visaoGeral().then(setDados).catch((e) => setErro(e.message));
  }, []);

  if (erro) return <div className="tab-panel active"><div className="form-error">{erro}</div></div>;
  if (!dados) return <div className="tab-panel active"><div className="page-date">Carregando indicadores...</div></div>;

  const hoje = new Date(dados.data).toLocaleDateString('pt-BR', {
    weekday: 'long', day: '2-digit', month: 'long',
  });

  const agendamentos = comparar(dados.agendamentosHoje, dados.agendamentosOntem);
  const ocupacao = comparar(dados.taxaOcupacao, dados.taxaOcupacaoOntem, '%');
  const faturamento = compararEmDinheiro(dados.faturamentoHoje, dados.faturamentoOntem);
  const cancelamentos = comparar(dados.cancelamentosHoje, dados.cancelamentosOntem);

  return (
    <div className="tab-panel active">
      <div className="topbar">
        <div>
          <div className="page-title">Visão geral</div>
          <div className="page-date">{hoje}</div>
        </div>
      </div>

      <div className="stat-grid">
        <div className="stat-card">
          <div className="stat-label">Agendamentos hoje</div>
          <div className="stat-value">{dados.agendamentosHoje}</div>
          <div className={`stat-delta ${agendamentos.classe}`}>{agendamentos.texto}</div>
        </div>
        <div className="stat-card">
          <div className="stat-label">Taxa de ocupação</div>
          <div className="stat-value">{dados.taxaOcupacao}%</div>
          <div className={`stat-delta ${ocupacao.classe}`}>{ocupacao.texto}</div>
        </div>
        <div className="stat-card">
          <div className="stat-label">Faturamento do dia</div>
          <div className="stat-value">{formatarMoeda(dados.faturamentoHoje)}</div>
          <div className={`stat-delta ${faturamento.classe}`}>{faturamento.texto}</div>
        </div>
        <div className="stat-card">
          <div className="stat-label">Cancelamentos</div>
          <div className="stat-value">{dados.cancelamentosHoje}</div>
          {}
          <div className={`stat-delta ${cancelamentos.classe === 'up' ? 'down' : 'up'}`}>
            {cancelamentos.texto}
          </div>
        </div>
      </div>

      {}
      {(dados.aguardandoConfirmacao > 0 || dados.atendimentosAFechar > 0) && (
        <div className="alert-row">
          {dados.aguardandoConfirmacao > 0 && (
            <div className="alert-card">
              <span className="alert-num">{dados.aguardandoConfirmacao}</span>
              agendamento(s) aguardando confirmação da cliente
            </div>
          )}
          {dados.atendimentosAFechar > 0 && (
            <div className="alert-card destaque">
              <span className="alert-num">{dados.atendimentosAFechar}</span>
              atendimento(s) já realizados aguardando baixa no financeiro
            </div>
          )}
        </div>
      )}

      <div className="grid-2">
        <div className="panel-card">
          <div className="panel-title">Agendamentos por categoria (hoje)</div>
          {dados.porCategoria.every((c) => c.quantidade === 0) && (
            <div className="vazio">Nenhum agendamento hoje ainda.</div>
          )}
          {dados.porCategoria.map((s) => (
            <div className="bar-row" key={s.slug}>
              <div className="bar-label">{s.nome}</div>
              <div className="bar-track">
                <div className="bar-fill" style={{ width: `${s.percentual}%`, background: s.cor }} />
              </div>
              <div className="bar-count">{s.quantidade}</div>
            </div>
          ))}
        </div>

        <div className="panel-card">
          <div className="panel-title">Próximos atendimentos</div>
          {dados.proximosAtendimentos.length === 0 && (
            <div className="vazio">Nenhum atendimento à frente na agenda.</div>
          )}
          {dados.proximosAtendimentos.map((a) => (
            <div className="mini-appt" key={a.id}>
              <div>
                <div className="who">{a.clienteNome}</div>
                <div className="what">{a.procedimentoNome} · {a.profissionalNome}</div>
              </div>
              <div className="when">
                {a.dataLabel}
                <span className="mini-hora">{a.hora}</span>
              </div>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
}
