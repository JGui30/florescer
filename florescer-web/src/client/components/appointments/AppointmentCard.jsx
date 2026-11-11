export default function AppointmentCard({ appt, onConfirm, onCancel, onReschedule, ocupado }) {
  return (
    <div className="appt-card">
      <div className="appt-card-top">
        <div>
          <div className="appt-proc">
            <span className="appt-cat-dot" style={{ background: corDaCategoria(appt.categoriaSlug) }} />
            {appt.procedimentoNome}
          </div>
          <div className="appt-meta">{appt.categoriaNome} · {appt.profissionalNome}</div>
        </div>
        <span className={`status-pill ${appt.status}`}>{appt.status}</span>
      </div>

      <div className="appt-when">{appt.dataLabel} às {appt.hora}</div>

      {}
      {appt.precisaConfirmar && !appt.prazoVencido && (
        <div className="aviso-prazo compacto">{appt.avisoDeConfirmacao}</div>
      )}
      {appt.prazoVencido && (
        <div className="aviso-vencido">O prazo de confirmação venceu. Remarque para escolher um novo horário.</div>
      )}
      {appt.motivoCancelamento && (
        <div className="appt-motivo">{appt.motivoCancelamento}</div>
      )}
      {appt.status !== 'Concluído' && (
        <div className="appt-actions">
          {appt.precisaConfirmar && !appt.prazoVencido && (
            <button className="act-confirm" onClick={onConfirm} disabled={ocupado}>Confirmar</button>
          )}
          {appt.podeReagendar && (
            <button className="act-reschedule" onClick={onReschedule} disabled={ocupado}>Remarcar</button>
          )}
          {appt.podeCancelar && (
            <button className="act-cancel" onClick={onCancel} disabled={ocupado}>Cancelar</button>
          )}
        </div>
      )}
    </div>
  );
}

const CORES = {
  unhas: '#E8306E',
  cabelo: '#B5540E',
  spa: '#128F5D',
  estetica: '#E8A93C',
};
function corDaCategoria(slug) {
  return CORES[slug] || 'var(--wine)';
}
