import { useState, useEffect, useCallback } from 'react';
import { catalogo } from '../../services/api.js';
import { urlDaImagem } from '../../services/http.js';

export default function VitrineTab() {
  const [categorias, setCategorias] = useState([]);
  const [carregando, setCarregando] = useState(true);
  const [ocupada, setOcupada] = useState(null);
  const [erro, setErro] = useState('');
  const [aviso, setAviso] = useState('');

  const carregar = useCallback(async () => {
    try {
      setCategorias(await catalogo.todasAsCategorias());
      setErro('');
    } catch (err) {
      setErro(err.message);
    } finally {
      setCarregando(false);
    }
  }, []);

  useEffect(() => { carregar(); }, [carregar]);

  function anunciar(texto) {
    setAviso(texto);
    setTimeout(() => setAviso(''), 2500);
  }

  const FORMATOS_ACEITOS = ['jpg', 'jpeg', 'png', 'webp', 'gif'];
  const TAMANHO_MAXIMO = 5 * 1024 * 1024;

  function recusar(arquivo) {
    const extensao = arquivo.name.split('.').pop()?.toLowerCase() || '';

    if (!FORMATOS_ACEITOS.includes(extensao)) {
      return `"${arquivo.name}" não é um formato aceito. Envie uma imagem em JPG, PNG, WEBP ou GIF.`;
    }

    if (arquivo.size > TAMANHO_MAXIMO) {
      const mb = (arquivo.size / 1024 / 1024).toFixed(1).replace('.', ',');
      return `"${arquivo.name}" tem ${mb} MB. O tamanho máximo é 5 MB.`;
    }

    return null;
  }

  async function enviar(categoria, arquivo) {
    if (!arquivo) return;

    const recusa = recusar(arquivo);
    if (recusa) {
      setErro(recusa);
      return;
    }

    setOcupada(categoria.id);
    setErro('');
    try {
      await catalogo.enviarImagemDaCategoria(categoria.id, arquivo);
      anunciar(`Imagem de ${categoria.nome} atualizada.`);
      await carregar();
    } catch (err) {
      setErro(err.message);
    } finally {
      setOcupada(null);
    }
  }

  async function remover(categoria) {
    const confirmado = window.confirm(
      `Remover a imagem de ${categoria.nome}?\n\nA categoria volta a ser exibida com o círculo colorido.`
    );
    if (!confirmado) return;

    setOcupada(categoria.id);
    setErro('');
    try {
      await catalogo.removerImagemDaCategoria(categoria.id);
      anunciar(`Imagem de ${categoria.nome} removida.`);
      await carregar();
    } catch (err) {
      setErro(err.message);
    } finally {
      setOcupada(null);
    }
  }

  if (carregando) {
    return <div className="tab-panel active"><div className="page-date">Carregando categorias...</div></div>;
  }

  const comImagem = categorias.filter((c) => c.imagemUrl).length;

  return (
    <div className="tab-panel active">
      <div className="topbar">
        <div>
          <div className="page-title">Vitrine</div>
          <div className="page-date">
            Imagens das categorias · {comImagem} de {categorias.length} com foto
          </div>
        </div>
      </div>

      {aviso && <div className="aviso-salvo">{aviso}</div>}
      {erro && <div className="form-error">{erro}</div>}

      <div className="cat-img-grid">
        {categorias.map((cat) => {
          const trabalhando = ocupada === cat.id;

          return (
            <div className="cat-img-card" key={cat.id}>
              <div className="cat-img-preview">
                {cat.imagemUrl ? (
                  <img src={urlDaImagem(cat.imagemUrl)} alt={cat.nome} />
                ) : (
                  <div className="cat-img-vazia">
                    <span className="cat-img-circulo" style={{ background: cat.cor }} />
                    <span className="cat-img-sem">Sem imagem</span>
                  </div>
                )}
              </div>

              <div className="cat-img-info">
                <div className="cat-img-nome">{cat.nome}</div>
                <div className="cat-img-sub">{cat.procedimentos.length} procedimentos</div>
              </div>

              <div className="cat-img-acoes">
                <label className={`icon-btn ${trabalhando ? 'ocupado' : ''}`}>
                  <input
                    type="file"
                    accept="image/*"
                    disabled={trabalhando}
                    onChange={(e) => {
                      enviar(cat, e.target.files?.[0]);
                      e.target.value = '';
                    }}
                    style={{ display: 'none' }}
                  />
                  {trabalhando ? 'Enviando...' : cat.imagemUrl ? 'Trocar imagem' : 'Enviar imagem'}
                </label>

                {cat.imagemUrl && (
                  <button className="icon-btn danger" disabled={trabalhando} onClick={() => remover(cat)}>
                    Remover
                  </button>
                )}
              </div>
            </div>
          );
        })}
      </div>
    </div>
  );
}
