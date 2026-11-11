import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import './landing.css';
import BloomLogo from '../assets/BloomLogo.jsx';
import { landing } from '../services/api.js';
import { urlDaImagem } from '../services/http.js';

export default function LandingPage() {
  const [dados, setDados] = useState(null);
  const [erro, setErro] = useState('');
  const [faqAberta, setFaqAberta] = useState(null);
  const navigate = useNavigate();

  useEffect(() => {
    landing.publica().then(setDados).catch((e) => setErro(e.message));
  }, []);

  if (erro) {
    return (
      <div className="lp-carregando">
        <BloomLogo size="lg" />
        <p>Não foi possível carregar a página. Verifique se o servidor está no ar.</p>
        <p className="lp-erro-detalhe">{erro}</p>
      </div>
    );
  }

  if (!dados) {
    return (
      <div className="lp-carregando">
        <BloomLogo size="lg" />
        <p>Carregando...</p>
      </div>
    );
  }

  const { estabelecimento, secoes, categorias, equipe, faqs, numeros } = dados;
  const secao = (chave) => secoes.find((s) => s.chave === chave);

  const hero = secao('hero');
  const diferenciais = secao('diferenciais');
  const servicos = secao('servicos');
  const secaoEquipe = secao('equipe');
  const galeria = secao('galeria');
  const secaoFaq = secao('faq');
  const cta = secao('cta');

  const imagemDoHero = hero?.midias?.[0];

  const irParaCadastro = () => navigate('/app');

  const linkWhatsApp = estabelecimento.whatsApp
    ? `https://wa.me/${estabelecimento.whatsApp}?text=${encodeURIComponent('Olá! Gostaria de agendar um horário.')}`
    : null;

  return (
    <div className="lp">
      {}
      <header className="lp-nav">
        <div className="lp-nav-interna">
          <div className="lp-marca">
            <BloomLogo size="sm" />
            {estabelecimento.nome}
          </div>
          <nav className="lp-links">
            <a href="#servicos">Serviços</a>
            <a href="#equipe">Equipe</a>
            <a href="#faq">Dúvidas</a>
            <a href="#contato">Contato</a>
          </nav>
          <button className="lp-btn lp-btn-principal pequeno" onClick={irParaCadastro}>
            Entrar / Agendar
          </button>
        </div>
      </header>

      {}
      {hero?.ativa && (
        <section className="lp-hero">
          <div className="lp-hero-conteudo">
            {}
            <div className="lp-selo">
              {numeros.atendimentosRealizados.toLocaleString('pt-BR')} atendimentos realizados
              {' · '}
              {numeros.clientesAtendidas} clientes atendidas
            </div>
            <h1 className="lp-titulo">{hero.titulo}</h1>
            <p className="lp-subtitulo">{hero.subtitulo}</p>
            <div className="lp-hero-acoes">
              <button className="lp-btn lp-btn-principal" onClick={irParaCadastro}>
                {hero.textoBotao || 'Agendar agora'}
              </button>
              <a className="lp-btn lp-btn-secundario" href="#servicos">Ver serviços e preços</a>
            </div>
            {}
            {numeros.horariosLivresNaSemana > 0 && (
              <div className="lp-urgencia">
                <span className="lp-pulso" />
                Restam <strong>{numeros.horariosLivresNaSemana} horários</strong> nos próximos 7 dias
                {numeros.ocupacaoDaSemana > 0 && ` — agenda ${numeros.ocupacaoDaSemana}% ocupada`}
              </div>
            )}
          </div>
          {}
          {imagemDoHero ? (
            <div className="lp-hero-imagem">
              <img src={urlDaImagem(imagemDoHero.url)} alt={imagemDoHero.legenda || estabelecimento.nome} />
            </div>
          ) : (
            <div className="lp-hero-numeros">
              <div className="lp-numero">
                <span className="lp-numero-valor">{numeros.clientesAtendidas}</span>
                <span className="lp-numero-rotulo">clientes atendidas</span>
              </div>
              <div className="lp-numero">
                <span className="lp-numero-valor">{numeros.profissionaisAtivas}</span>
                <span className="lp-numero-rotulo">profissionais especializadas</span>
              </div>
              <div className="lp-numero">
                <span className="lp-numero-valor">{numeros.procedimentosOferecidos}</span>
                <span className="lp-numero-rotulo">procedimentos disponíveis</span>
              </div>
              <div className="lp-numero">
                <span className="lp-numero-valor">{numeros.categoriasOferecidas}</span>
                <span className="lp-numero-rotulo">áreas de especialidade</span>
              </div>
            </div>
          )}
        </section>
      )}
      {}
      {diferenciais?.ativa && (
        <section className="lp-secao lp-diferenciais">
          <h2 className="lp-secao-titulo">{diferenciais.titulo}</h2>
          <p className="lp-secao-sub">{diferenciais.subtitulo}</p>
          <div className="lp-cards">
            <div className="lp-card">
              <div className="lp-card-icone">◷</div>
              <h3>Agendamento em 1 minuto</h3>
              <p>Escolha o serviço, a profissional e o horário direto pelo celular, a qualquer hora do dia.</p>
            </div>
            <div className="lp-card">
              <div className="lp-card-icone">✓</div>
              <h3>Horário garantido</h3>
              <p>Você confirma seu agendamento pelo aplicativo e o horário fica reservado só para você.</p>
            </div>
            <div className="lp-card">
              <div className="lp-card-icone">◆</div>
              <h3>Especialistas por área</h3>
              <p>Cada procedimento é realizado por quem domina a técnica, não por quem estiver livre.</p>
            </div>
            <div className="lp-card">
              <div className="lp-card-icone">❋</div>
              <h3>Ambiente acolhedor</h3>
              <p>Espaço pensado para o seu conforto, com protocolos rigorosos de higienização.</p>
            </div>
          </div>
        </section>
      )}
      {}
      {servicos?.ativa && (
        <section className="lp-secao lp-servicos" id="servicos">
          <h2 className="lp-secao-titulo">{servicos.titulo}</h2>
          <p className="lp-secao-sub">{servicos.subtitulo}</p>
          <div className="lp-categorias">
            {categorias.map((cat) => (
              <div className="lp-categoria" key={cat.id}>
                <div className="lp-categoria-topo" style={{ borderTopColor: cat.cor }}>
                  <h3>{cat.nome}</h3>
                  <span>{cat.procedimentos.length} procedimentos</span>
                </div>
                <ul className="lp-procedimentos">
                  {cat.procedimentos.slice(0, 6).map((p) => (
                    <li key={p.id}>
                      <span className="lp-proc-nome">{p.nome}</span>
                      <span className="lp-proc-info">
                        <span className="lp-proc-dur">{p.duracaoMinutos} min</span>
                        <span className="lp-proc-preco">R$ {p.preco.toFixed(0)}</span>
                      </span>
                    </li>
                  ))}
                  {cat.procedimentos.length > 6 && (
                    <li className="lp-proc-mais">+ {cat.procedimentos.length - 6} outros procedimentos</li>
                  )}
                </ul>
              </div>
            ))}
          </div>
          <div className="lp-centro">
            <button className="lp-btn lp-btn-principal" onClick={irParaCadastro}>Escolher meu procedimento</button>
          </div>
        </section>
      )}
      {}
      {secaoEquipe?.ativa && (
        <section className="lp-secao lp-equipe" id="equipe">
          <h2 className="lp-secao-titulo">{secaoEquipe.titulo}</h2>
          <p className="lp-secao-sub">{secaoEquipe.subtitulo}</p>
          <div className="lp-equipe-grid">
            {equipe.map((p) => (
              <div className="lp-profissional" key={p.id}>
                {p.fotoUrl
                  ? <img className="lp-pro-foto" src={urlDaImagem(p.fotoUrl)} alt={p.nome} />
                  : <div className="lp-pro-inicial">{p.nome.charAt(0)}</div>}
                <div className="lp-pro-nome">{p.nome}</div>
                <div className="lp-pro-esp">{p.especialidade}</div>
              </div>
            ))}
          </div>
        </section>
      )}
      {}
      {galeria?.ativa && galeria.midias.length > 0 && (
        <section className="lp-secao lp-galeria">
          <h2 className="lp-secao-titulo">{galeria.titulo}</h2>
          <p className="lp-secao-sub">{galeria.subtitulo}</p>
          <div className="lp-galeria-grid">
            {galeria.midias.map((m) => (
              <figure className="lp-galeria-item" key={m.id}>
                <img src={urlDaImagem(m.url)} alt={m.legenda || 'Trabalho realizado'} />
                {m.legenda && <figcaption>{m.legenda}</figcaption>}
              </figure>
            ))}
          </div>
        </section>
      )}
      {}
      {secaoFaq?.ativa && faqs.length > 0 && (
        <section className="lp-secao lp-faq" id="faq">
          <h2 className="lp-secao-titulo">{secaoFaq.titulo}</h2>
          <p className="lp-secao-sub">{secaoFaq.subtitulo}</p>
          <div className="lp-faq-lista">
            {faqs.map((f) => (
              <div className={`lp-faq-item ${faqAberta === f.id ? 'aberta' : ''}`} key={f.id}>
                <button className="lp-faq-pergunta" onClick={() => setFaqAberta(faqAberta === f.id ? null : f.id)}>
                  {f.pergunta}
                  <span className="lp-faq-sinal">{faqAberta === f.id ? '−' : '+'}</span>
                </button>
                {faqAberta === f.id && <div className="lp-faq-resposta">{f.resposta}</div>}
              </div>
            ))}
          </div>
        </section>
      )}
      {}
      {cta?.ativa && (
        <section className="lp-cta">
          <h2>{cta.titulo}</h2>
          <p>{cta.subtitulo}</p>
          <button className="lp-btn lp-btn-claro" onClick={irParaCadastro}>
            {cta.textoBotao || 'Criar minha conta'}
          </button>
          <div className="lp-cta-nota">Criar conta é gratuito e leva menos de um minuto.</div>
        </section>
      )}
      {}
      <footer className="lp-rodape" id="contato">
        <div className="lp-rodape-grid">
          <div>
            <div className="lp-marca clara">
              <BloomLogo size="sm" dark />
              {estabelecimento.nome}
            </div>
            {estabelecimento.slogan && <p className="lp-rodape-slogan">{estabelecimento.slogan}</p>}
          </div>
          <div>
            <h4>Contato</h4>
            {estabelecimento.telefone && <p>{estabelecimento.telefone}</p>}
            {estabelecimento.email && <p>{estabelecimento.email}</p>}
            {estabelecimento.endereco && <p>{estabelecimento.endereco}</p>}
          </div>
          <div>
            <h4>Funcionamento</h4>
            <p>{estabelecimento.horarioFuncionamento}</p>
            <div className="lp-redes">
              {estabelecimento.instagramUrl && (
                <a href={estabelecimento.instagramUrl} target="_blank" rel="noreferrer">Instagram</a>
              )}
              {estabelecimento.facebookUrl && (
                <a href={estabelecimento.facebookUrl} target="_blank" rel="noreferrer">Facebook</a>
              )}
            </div>
          </div>
        </div>
        <div className="lp-rodape-base">
          <span>© {new Date().getFullYear()} {estabelecimento.nome}. Todos os direitos reservados.</span>
          <a href="/admin">Acesso administrativo</a>
        </div>
      </footer>
      {}
      {linkWhatsApp && (
        <a className="lp-whatsapp" href={linkWhatsApp} target="_blank" rel="noreferrer" title="Falar no WhatsApp">
          <span>Falar no WhatsApp</span>
        </a>
      )}
    </div>
  );
}
