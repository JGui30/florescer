const API_URL = import.meta.env.VITE_API_URL || 'http://localhost:5080';

function area() {
  return window.location.pathname.startsWith('/admin') ? 'admin' : 'cliente';
}

function chaveToken() {
  return `florescer.${area()}.token`;
}

function chaveUsuario() {
  return `florescer.${area()}.usuario`;
}

export function salvarSessao(token, usuario) {
  localStorage.setItem(chaveToken(), token);
  localStorage.setItem(chaveUsuario(), JSON.stringify(usuario));
}

export function lerToken() {
  return localStorage.getItem(chaveToken());
}

export function lerUsuario() {
  const bruto = localStorage.getItem(chaveUsuario());
  return bruto ? JSON.parse(bruto) : null;
}

export function limparSessao() {
  localStorage.removeItem(chaveToken());
  localStorage.removeItem(chaveUsuario());

  localStorage.removeItem('florescer.token');
  localStorage.removeItem('florescer.usuario');
}

const ESPERA_MAXIMA = 90_000;

function limiteDeTempo() {
  return typeof AbortSignal !== 'undefined' && AbortSignal.timeout
    ? AbortSignal.timeout(ESPERA_MAXIMA)
    : undefined;
}

async function aInternetRespondeu() {
  if (typeof navigator !== 'undefined' && navigator.onLine === false) return false;

  try {
    await fetch(window.location.origin, { method: 'HEAD', cache: 'no-store' });
    return true;
  } catch {
    return false;
  }
}

async function mensagemDeFalhaDeRede(erro) {
  if (erro?.name === 'TimeoutError') {
    return 'O servidor demorou demais para responder. Verifique a lista e, se nada mudou, tente de novo.';
  }

  if (await aInternetRespondeu()) {
    return 'O servidor não respondeu. Ele pode estar fora do ar ou reiniciando. ' +
           'Tente de novo em alguns instantes.';
  }

  return 'Você parece estar sem conexão com a internet. Verifique e tente de novo.';
}

function mensagemDeFalhaDoServidor(status) {
  if (status >= 500) {
    return 'O servidor está indisponível neste momento. Tente de novo em instantes.';
  }

  return 'Não foi possível concluir a operação.';
}

async function requisitar(caminho, opcoes = {}) {
  const cabecalhos = { 'Content-Type': 'application/json', ...opcoes.headers };

  const token = lerToken();
  if (token) cabecalhos.Authorization = `Bearer ${token}`;

  let resposta;
  try {
    resposta = await fetch(`${API_URL}${caminho}`, {
      ...opcoes,
      headers: cabecalhos,
      signal: limiteDeTempo(),
    });
  } catch (erro) {
    throw new Error(await mensagemDeFalhaDeRede(erro));
  }

  if (resposta.status === 401) {
    limparSessao();
    throw new Error('Sua sessão expirou. Entre novamente.');
  }

  if (resposta.status === 204) return null;

  const texto = await resposta.text();
  let dados = null;
  if (texto) {
    try {
      dados = JSON.parse(texto);
    } catch {
      dados = null;
    }
  }

  if (!resposta.ok) {
    throw new Error(dados?.erro || mensagemDeFalhaDoServidor(resposta.status));
  }

  return dados;
}

export function get(caminho) {
  return requisitar(caminho);
}

export function post(caminho, corpo) {
  return requisitar(caminho, { method: 'POST', body: JSON.stringify(corpo ?? {}) });
}

export function put(caminho, corpo) {
  return requisitar(caminho, { method: 'PUT', body: JSON.stringify(corpo ?? {}) });
}

export function remover(caminho) {
  return requisitar(caminho, { method: 'DELETE' });
}

export async function enviarArquivo(caminho, arquivo) {
  const formulario = new FormData();
  formulario.append('arquivo', arquivo);

  const cabecalhos = {};
  const token = lerToken();
  if (token) cabecalhos.Authorization = `Bearer ${token}`;

  let resposta;
  try {
    resposta = await fetch(`${API_URL}${caminho}`, {
      method: 'POST',
      headers: cabecalhos,
      body: formulario,
      signal: limiteDeTempo(),
    });
  } catch (erro) {
    throw new Error(await mensagemDeFalhaDeRede(erro));
  }

  const texto = await resposta.text();
  let dados = null;
  if (texto) {
    try {
      dados = JSON.parse(texto);
    } catch {
      dados = null;
    }
  }

  if (!resposta.ok) throw new Error(dados?.erro || 'Não foi possível enviar a imagem.');

  return dados;
}

export function urlDaImagem(caminho) {
  if (!caminho) return '';
  return caminho.startsWith('http') ? caminho : `${API_URL}${caminho}`;
}
