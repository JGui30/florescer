export default function ProcedureStep({ categoria, onSelect, onBack }) {
  return (
    <div className="step-panel active">
      <h2 className="step-title">Escolha o procedimento</h2>
      <p className="step-sub">Categoria: {categoria.nome}</p>
      <div>
        {categoria.procedimentos.map((p) => (
          <div className="list-item" key={p.id} onClick={() => onSelect(p)}>
            <div className="li-main">
              <div className="li-title">{p.nome}</div>
              <div className="li-meta">{p.duracaoMinutos} min</div>
            </div>
            <div className="li-price">R$ {p.preco.toFixed(2).replace('.', ',')}</div>
          </div>
        ))}
      </div>
      <div className="nav-row">
        <button className="btn-ghost" onClick={onBack}>← Voltar</button>
      </div>
    </div>
  );
}
