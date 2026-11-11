import { get, post, put, remover, enviarArquivo, salvarSessao, limparSessao } from './http.js';

export const auth = {
  async entrar(email, senha) {
    const dados = await post('/api/auth/login', { email, senha });
    salvarSessao(dados.token, dados.usuario);
    return dados.usuario;
  },

  async entrarComoAdmin(email, senha) {
    const dados = await post('/api/auth/login-admin', { email, senha });
    salvarSessao(dados.token, dados.usuario);
    return dados.usuario;
  },

  async criarConta(nome, email, senha) {
    const dados = await post('/api/auth/registrar', { nome, email, senha });
    salvarSessao(dados.token, dados.usuario);
    return dados.usuario;
  },

  sair() {
    limparSessao();
  },

  meuPerfil: () => get('/api/auth/eu'),

  atualizarPerfil: (nome, email, telefone) => put('/api/auth/eu', { nome, email, telefone }),

  alterarSenha: (senhaAtual, novaSenha) => post('/api/auth/alterar-senha', { senhaAtual, novaSenha }),

  solicitarRecuperacao: (email) => post('/api/auth/recuperar', { email }),

  validarCodigo: (email, codigo) => post('/api/auth/recuperar/validar', { email, codigo }),

  redefinirSenha: (email, codigo, novaSenha) =>
    post('/api/auth/recuperar/redefinir', { email, codigo, novaSenha }),
};

export const catalogo = {
  categorias: () => get('/api/catalogo/categorias'),

  todasAsCategorias: () => get('/api/catalogo/categorias?incluirInativos=true'),

  procedimentos: () => get('/api/catalogo/procedimentos?incluirInativos=true'),

  criarCategoria: (dados) => post('/api/catalogo/categorias', dados),

  atualizarCategoria: (id, dados) => put(`/api/catalogo/categorias/${id}`, dados),

  desativarCategoria: (id) => remover(`/api/catalogo/categorias/${id}`),

  criarProcedimento: (dados) => post('/api/catalogo/procedimentos', dados),

  atualizarProcedimento: (id, dados) => put(`/api/catalogo/procedimentos/${id}`, dados),

  desativarProcedimento: (id) => remover(`/api/catalogo/procedimentos/${id}`),

  enviarImagemDaCategoria: (id, arquivo) =>
    enviarArquivo(`/api/catalogo/categorias/${id}/imagem`, arquivo),

  removerImagemDaCategoria: (id) => remover(`/api/catalogo/categorias/${id}/imagem`),
};

export const profissionais = {
  listar: () => get('/api/profissionais'),

  ativas: () => get('/api/profissionais?apenasAtivas=true'),

  queRealizam: (procedimentoId) =>
    get(`/api/profissionais?apenasAtivas=true&procedimentoId=${procedimentoId}`),

  disponibilidade: (profissionalId, procedimentoId, dias = 14, de = null) => {
    const inicio = de ? `&de=${de}` : '';
    return get(
      `/api/profissionais/${profissionalId}/disponibilidade?procedimentoId=${procedimentoId}&dias=${dias}${inicio}`
    );
  },

  criar: (dados) => post('/api/profissionais', dados),

  atualizar: (id, dados) => put(`/api/profissionais/${id}`, dados),

  desativar: (id) => remover(`/api/profissionais/${id}`),
};

export const agendamentos = {
  meus: () => get('/api/agendamentos/meus'),

  previa: (procedimentoId, inicio) =>
    get(`/api/agendamentos/previa?procedimentoId=${procedimentoId}&inicio=${inicio}`),

  criar: (procedimentoId, profissionalId, inicio, observacoes) =>
    post('/api/agendamentos', { procedimentoId, profissionalId, inicio, observacoes }),

  confirmar: (id) => post(`/api/agendamentos/${id}/confirmar`),

  cancelar: (id) => post(`/api/agendamentos/${id}/cancelar`),

  reagendar: (id, novoInicio, novoProfissionalId = null) =>
    post(`/api/agendamentos/${id}/reagendar`, { novoInicio, novoProfissionalId }),

  concluir: (id, valorCobrado) => post(`/api/agendamentos/${id}/concluir`, { valorCobrado }),

  registrarFalta: (id) => post(`/api/agendamentos/${id}/falta`),

  listar: ({ data, de, ate, profissionalId, status } = {}) => {
    const filtros = [];
    if (data) filtros.push(`data=${data}`);
    if (de) filtros.push(`de=${de}`);
    if (ate) filtros.push(`ate=${ate}`);
    if (profissionalId) filtros.push(`profissionalId=${profissionalId}`);
    if (status) filtros.push(`status=${status}`);

    return get(`/api/agendamentos${filtros.length ? '?' + filtros.join('&') : ''}`);
  },

  diasComMovimento: (de, ate, profissionalId = null) => {
    const filtro = profissionalId ? `&profissionalId=${profissionalId}` : '';
    return get(`/api/agenda/dias-com-movimento?de=${de}&ate=${ate}${filtro}`);
  },

  periodoDaAgenda: () => get('/api/agenda/periodo'),

  revisarAgora: () => post('/api/agenda/revisar'),
};

export const painel = {
  visaoGeral: (data = null) => get(`/api/painel/visao-geral${data ? `?data=${data}` : ''}`),

  notificacoes: (limite = 50) => get(`/api/painel/notificacoes?limite=${limite}`),
};

export const financeiro = {
  resumo: (periodo = 'mes', referencia = null) =>
    get(`/api/financeiro/resumo?periodo=${periodo}${referencia ? `&referencia=${referencia}` : ''}`),

  resumoPorIntervalo: (de, ate) =>
    get(`/api/financeiro/resumo?periodo=personalizado&de=${de}&ate=${ate}`),

  aFechar: () => get('/api/financeiro/a-fechar'),

  lancamentos: (de = null, ate = null) => {
    const filtros = [];
    if (de) filtros.push(`de=${de}`);
    if (ate) filtros.push(`ate=${ate}`);
    return get(`/api/financeiro/lancamentos${filtros.length ? '?' + filtros.join('&') : ''}`);
  },
};

export const landing = {
  publica: () => get('/api/landing'),

  completa: () => get('/api/landing?incluirInativos=true'),

  atualizarEstabelecimento: (dados) => put('/api/landing/estabelecimento', dados),

  atualizarSecao: (id, dados) => put(`/api/landing/secoes/${id}`, dados),

  enviarImagem: (arquivo, secaoId = null, legenda = null) => {
    const filtros = [];
    if (secaoId) filtros.push(`secaoId=${secaoId}`);
    if (legenda) filtros.push(`legenda=${encodeURIComponent(legenda)}`);
    return enviarArquivo(`/api/landing/midias${filtros.length ? '?' + filtros.join('&') : ''}`, arquivo);
  },

  removerImagem: (id) => remover(`/api/landing/midias/${id}`),

  criarFaq: (dados) => post('/api/landing/faqs', dados),

  atualizarFaq: (id, dados) => put(`/api/landing/faqs/${id}`, dados),

  removerFaq: (id) => remover(`/api/landing/faqs/${id}`),
};
