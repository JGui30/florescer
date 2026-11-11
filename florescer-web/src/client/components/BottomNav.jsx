export default function BottomNav({ activeView, onChange }) {
  return (
    <div className="bottom-nav">
      <div className={`bn-item ${activeView === 'booking' ? 'active' : ''}`} onClick={() => onChange('booking')}>
        <span className="bn-icon">✦</span>Agendar
      </div>
      <div className={`bn-item ${activeView === 'appointments' ? 'active' : ''}`} onClick={() => onChange('appointments')}>
        <span className="bn-icon">▤</span>Agendamentos
      </div>
    </div>
  );
}
