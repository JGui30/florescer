import { urlDaImagem } from '../../../services/http.js';

export default function CategoryStep({ categorias, onSelect }) {
  return (
    <div className="step-panel active">
      <h2 className="step-title">O que você deseja fazer hoje?</h2>
      <p className="step-sub">Escolha uma categoria para começar</p>
      <div className="cat-grid">
        {categorias.map((cat) => (
          <div
            className={`cat-card ${cat.imagemUrl ? 'com-foto' : ''}`}
            key={cat.id}
            onClick={() => onSelect(cat)}
          >
            {cat.imagemUrl ? (
              <div className="cat-foto">
                {}
                <img src={urlDaImagem(cat.imagemUrl)} alt={cat.nome} />
              </div>
            ) : (
              <div className="cat-swatch" style={{ background: cat.cor }} />
            )}
            <div className="cat-texto">
              <div className="cat-name">{cat.nome}</div>
              <div className="cat-count">{cat.procedimentos.length} procedimentos</div>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}
