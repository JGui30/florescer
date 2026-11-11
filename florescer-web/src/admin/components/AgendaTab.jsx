import { useState, useEffect, useCallback } from 'react';
import CalendarFilter from './CalendarFilter.jsx';
import ConcluirModal from './ConcluirModal.jsx';
import { agendamentos, profissionais } from '../../services/api.js';

const STATUS_FILTERS = ['', 'Agendado', 'Confirmado', 'Concluído', 'Cancelado'];

const VALOR_DO_STATUS = {
  Agendado: 'Agendado',
  Confirmado: 'Confirmado',
  'Concluído': 'Concluido',
  Cancelado: 'Cancelado',
};

export default function AgendaTab() {
  const [lista, setLista] = useState([]);
  const [equipe, setEquipe] = useState([]);
  const [proFilter, setProFilter] = useState('');
  const [statusFilter, setStatusFilter] = useState('');
  const [dataFiltro, setDataFiltro] = useState(null);
  const [concluindo, setConcluindo] = useState(null);
  const [erro, setErro] = useState('');
  const [carregando, setCarregando] = useState(true);

  useEffect(() => {
    profissionais.listar().then(setEquipe).catch(() => setEquipe([]));
  }, []);

  const carregar = useCallback(async () => {
    setCarregando(true);
    try {
      const hoje = new Date();
      const dataPadrao = `${hoje.getFullYear()}-${String(hoje.getMonth() + 1).padStart(2, '0')}-${String(hoje.getDate()).padStart(2, '0')}`;

      const dados = await agendamentos.listar({
        data: dataFiltro || dataPadrao,
        profissionalId: proFilter || null,
        status: statusFilter ? VALOR_DO_STATUS[statusFilter] : null,
      });

      setLista(dados);
      setErro('');
    } catch (err) {
      setErro(err.message);
    } finally {
      setCarregando(false);
    }
  }, [dataFiltro, proFilter, statusFilter]);

  useEffect(() => { carregar(); }, [carregar]);

  async function executar(acao) {
    try {
      await acao();
      await carregar();
      setErro('');
    } catch (err) {
      setErro(err.message);
    }
  }

  const dataExibida = dataFiltro
    ? new Date(`${dataFiltro}T12:00:00`).toLocaleDateString('pt-BR', { weekday: 'long', day: '2-digit', month: 'long' })
    : new Date().toLocaleDateString('pt-BR', { weekday: 'long', day: '2-digit', month: 'long' });

  return (
    <div className="tab-panel active">
      <div className="topbar">
        <div>
          <div className="page-title">Agenda</div>
          <div className="page-date">{dataExibida}{proFilter ? '' : ' · todas as profissionais'}</div>
        </div>
        <button className="icon-btn" onClick={() => executar(() => agendamentos.revisarAgora())}>
          Revisar agenda agora
        </button>
      </div>

      {erro && <div className="form-error">{erro}</div>}

      <div className="agenda-layout">
        <div className="agenda-calendario">
          <CalendarFilter
            dataSelecionada={dataFiltro}
            onSelecionarData={setDataFiltro}
            profissionalId={proFilter}
          />
        </div>

        <div className="agenda-lista">
          <div className="filter-bar">
            <select className="filter-select" value={proFilter} onChange={(e) => setProFilter(e.target.value)}>
              <option value="">Todas as profissionais</option>
              {equipe.map((p) => (
                <option key={p.id} value={p.id}>{p.nome}</option>
              ))}
            </select>

            {STATUS_FILTERS.map((s) => (
              <div
                key={s || 'todos'}
                className={`chip ${statusFilter === s ? 'active' : ''}`}
                onClick={() => setStatusFilter(s)}
              >
                {s || 'Todos'}
              </div>
            ))}
          </div>

          <table className="appt-table">
            <thead>
              <tr>
                <th>Horário</th><th>Cliente</th><th>Procedimento</th><th>Profissional</th><th>Valor</th><th>Status</th>
              </tr>
            </thead>
            <tbody>
              {carregando && (
                <tr><td colSpan="6" className="tabela-vazia">Carregando...</td></tr>
              )}

              {!carregando && lista.length === 0 && (
                <tr><td colSpan="6" className="tabela-vazia">Nenhum agendamento com esse filtro.</td></tr>
              )}

              {!carregando && lista.map((a) => (
                <tr key={a.id}>
                  <td className="appt-time">{a.hora}</td>
                  <td>
                    {a.clienteNome}
                    {a.clienteTelefone && <div className="cel-sub">{a.clienteTelefone}</div>}
                  </td>
                  <td>{a.procedimentoNome}</td>
                  <td>{a.profissionalNome}</td>
                  <td className="appt-time">
                    {a.valorCobrado != null
                      ? `R$ ${a.valorCobrado.toFixed(2).replace('.', ',')}`
                      : <span className="cel-sub">R$ {a.valorPrevisto.toFixed(2).replace('.', ',')}</span>}
                  </td>
                  <td>
                    <span className={`badge ${a.status}`}>{a.status}</span>
                    {a.motivoCancelamento && <div className="cel-sub">{a.motivoCancelamento}</div>}

                    <div className="acoes-linha">
                      {a.status === 'Agendado' && (
                        <span className="acao-link" onClick={() => executar(() => agendamentos.confirmar(a.id))}>
                          confirmar
                        </span>
                      )}
                      {(a.status === 'Agendado' || a.status === 'Confirmado') && (
                        <>
                          <span className="acao-link destaque" onClick={() => setConcluindo(a)}>
                            dar baixa
                          </span>
                          <span className="acao-link" onClick={() => executar(() => agendamentos.registrarFalta(a.id))}>
                            faltou
                          </span>
                          <span className="acao-link" onClick={() => executar(() => agendamentos.cancelar(a.id))}>
                            cancelar
                          </span>
                        </>
                      )}
                    </div>
                  </td>
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
