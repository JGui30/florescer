const STEPS = ['Categoria', 'Procedimento', 'Profissional', 'Data', 'Confirmar'];

export default function RibbonProgress({ currentStep }) {
  return (
    <div className="ribbon-wrap">
      <div className="ribbon">
        {STEPS.map((label, i) => {
          const cls = i < currentStep ? 'done' : i === currentStep ? 'current' : '';
          const node = i < currentStep ? '✓' : i + 1;
          return (
            <div className={`ribbon-step ${cls}`} key={label}>
              <div className="ribbon-node">{node}</div>
              <div className="ribbon-label">{label}</div>
            </div>
          );
        })}
      </div>
    </div>
  );
}
