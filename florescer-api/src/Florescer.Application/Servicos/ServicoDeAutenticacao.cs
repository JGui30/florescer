using System.Security.Cryptography;
using Florescer.Application.Contratos;
using Florescer.Application.Dtos;
using Florescer.Application.Persistencia;
using Florescer.Domain.Entidades;
using Florescer.Domain.Enums;
using Florescer.Domain.Regras;
using Microsoft.EntityFrameworkCore;

namespace Florescer.Application.Servicos;

public class ServicoDeAutenticacao
{
    private const int MinutosDeValidadeDoCodigo = 15;

    private const int TentativasPermitidas = 5;

    private readonly IBancoDeDados _banco;
    private readonly IServicoDeSenha _senhas;
    private readonly IGeradorDeToken _tokens;
    private readonly IRelogio _relogio;
    private readonly ServicoDeNotificacao _notificacoes;

    public ServicoDeAutenticacao(
        IBancoDeDados banco,
        IServicoDeSenha senhas,
        IGeradorDeToken tokens,
        IRelogio relogio,
        ServicoDeNotificacao notificacoes)
    {
        _banco = banco;
        _senhas = senhas;
        _tokens = tokens;
        _relogio = relogio;
        _notificacoes = notificacoes;
    }

    public async Task<LoginResposta> RegistrarAsync(RegistroRequisicao dados, CancellationToken ct = default)
    {
        var email = NormalizarEmail(dados.Email);

        if (string.IsNullOrWhiteSpace(dados.Nome))
            throw new RegraDeNegocioException("Informe seu nome completo.");

        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            throw new RegraDeNegocioException("Informe um e-mail válido.");

        if (dados.Senha.Length < 6)
            throw new RegraDeNegocioException("A senha deve ter pelo menos 6 caracteres.");

        if (await _banco.Usuarios.AnyAsync(u => u.Email == email, ct))
            throw new RegraDeNegocioException("Já existe uma conta cadastrada com este e-mail.");

        var usuario = new Usuario
        {
            Nome = dados.Nome.Trim(),
            Email = email,
            SenhaHash = _senhas.GerarHash(dados.Senha),
            Perfil = PerfilUsuario.Cliente,
            Ativo = true,
            CriadoEm = _relogio.Agora
        };

        _banco.Usuarios.Add(usuario);
        await _banco.SalvarAsync(ct);

        return MontarResposta(usuario);
    }

    public async Task<LoginResposta> EntrarAsync(LoginRequisicao dados, CancellationToken ct = default)
    {
        var email = NormalizarEmail(dados.Email);

        var usuario = await _banco.Usuarios.FirstOrDefaultAsync(u => u.Email == email, ct);

        if (usuario is null || !_senhas.Conferir(dados.Senha, usuario.SenhaHash))
            throw new RegraDeNegocioException("E-mail ou senha incorretos.");

        if (!usuario.Ativo)
            throw new RegraDeNegocioException("Esta conta está desativada. Procure a recepção.");

        return MontarResposta(usuario);
    }

    public async Task<LoginResposta> EntrarComoAdministradorAsync(
        LoginRequisicao dados, CancellationToken ct = default)
    {
        var resposta = await EntrarAsync(dados, ct);

        if (resposta.Usuario.Perfil != nameof(PerfilUsuario.Administrador))
            throw new RegraDeNegocioException("Esta conta não tem acesso ao painel administrativo.");

        return resposta;
    }

    public async Task<UsuarioResposta> ObterPerfilAsync(int usuarioId, CancellationToken ct = default)
    {
        var usuario = await BuscarUsuarioAsync(usuarioId, ct);
        return Converter(usuario);
    }

    public async Task<UsuarioResposta> AtualizarPerfilAsync(
        int usuarioId, AtualizarPerfilRequisicao dados, CancellationToken ct = default)
    {
        var usuario = await BuscarUsuarioAsync(usuarioId, ct);
        var email = NormalizarEmail(dados.Email);

        if (string.IsNullOrWhiteSpace(dados.Nome))
            throw new RegraDeNegocioException("Informe seu nome completo.");

        if (await _banco.Usuarios.AnyAsync(u => u.Email == email && u.Id != usuarioId, ct))
            throw new RegraDeNegocioException("Este e-mail já está em uso por outra conta.");

        usuario.Nome = dados.Nome.Trim();
        usuario.Email = email;
        usuario.Telefone = dados.Telefone?.Trim();

        await _banco.SalvarAsync(ct);
        return Converter(usuario);
    }

    public async Task AlterarSenhaAsync(
        int usuarioId, AlterarSenhaRequisicao dados, CancellationToken ct = default)
    {
        var usuario = await BuscarUsuarioAsync(usuarioId, ct);

        if (!_senhas.Conferir(dados.SenhaAtual, usuario.SenhaHash))
            throw new RegraDeNegocioException("A senha atual está incorreta.");

        if (dados.NovaSenha.Length < 6)
            throw new RegraDeNegocioException("A nova senha deve ter pelo menos 6 caracteres.");

        usuario.SenhaHash = _senhas.GerarHash(dados.NovaSenha);
        await _banco.SalvarAsync(ct);
    }

    public async Task<SolicitarRecuperacaoResposta> SolicitarRecuperacaoAsync(
        SolicitarRecuperacaoRequisicao dados, CancellationToken ct = default)
    {
        var email = NormalizarEmail(dados.Email);
        var usuario = await _banco.Usuarios.FirstOrDefaultAsync(u => u.Email == email && u.Ativo, ct);

        var mensagemPadrao = "Se houver uma conta com este e-mail, o código de recuperação foi enviado.";

        if (usuario is null)
        {
            return new SolicitarRecuperacaoResposta(
                mensagemPadrao, Textos.Mascarar(email), null);
        }

        var anteriores = await _banco.CodigosRecuperacao
            .Where(c => c.UsuarioId == usuario.Id && c.UsadoEm == null && c.ExpiraEm > _relogio.Agora)
            .ToListAsync(ct);

        foreach (var anterior in anteriores)
            anterior.UsadoEm = _relogio.Agora;

        var codigo = GerarCodigoDeSeisDigitos();

        _banco.CodigosRecuperacao.Add(new CodigoRecuperacao
        {
            UsuarioId = usuario.Id,
            CodigoHash = _senhas.GerarHash(codigo),
            Canal = CanalNotificacao.Email,
            ExpiraEm = _relogio.Agora.AddMinutes(MinutosDeValidadeDoCodigo),
            CriadoEm = _relogio.Agora
        });

        await _banco.SalvarAsync(ct);

        var texto = ServicoDeNotificacao.TextoCodigoDeRecuperacao(
            usuario.Nome, codigo, MinutosDeValidadeDoCodigo);

        var notificacao = await _notificacoes.EnviarAsync(
            usuario, TipoNotificacao.CodigoDeRecuperacao,
            "Código de recuperação — Florescer", texto, null, ct);

        var simulado = notificacao.Status == StatusNotificacao.Simulada;

        return new SolicitarRecuperacaoResposta(
            mensagemPadrao,
            Textos.Mascarar(usuario.Email),
            simulado ? codigo : null);
    }

    public async Task ValidarCodigoAsync(string emailInformado, string codigo, CancellationToken ct = default)
    {
        await BuscarCodigoValidoAsync(emailInformado, codigo, ct);
    }

    public async Task RedefinirSenhaAsync(RedefinirSenhaRequisicao dados, CancellationToken ct = default)
    {
        if (dados.NovaSenha.Length < 6)
            throw new RegraDeNegocioException("A nova senha deve ter pelo menos 6 caracteres.");

        var registro = await BuscarCodigoValidoAsync(dados.Email, dados.Codigo, ct);

        registro.Usuario.SenhaHash = _senhas.GerarHash(dados.NovaSenha);
        registro.UsadoEm = _relogio.Agora;

        await _banco.SalvarAsync(ct);
    }

    private async Task<CodigoRecuperacao> BuscarCodigoValidoAsync(
        string emailInformado, string codigo, CancellationToken ct)
    {
        var email = NormalizarEmail(emailInformado);

        var registro = await _banco.CodigosRecuperacao
            .Include(c => c.Usuario)
            .Where(c => c.Usuario.Email == email && c.UsadoEm == null)
            .OrderByDescending(c => c.CriadoEm)
            .FirstOrDefaultAsync(ct);

        if (registro is null)
            throw new RegraDeNegocioException("Código inválido. Solicite um novo código.");

        if (registro.ExpiraEm < _relogio.Agora)
            throw new RegraDeNegocioException("Este código expirou. Solicite um novo código.");

        if (registro.Tentativas >= TentativasPermitidas)
            throw new RegraDeNegocioException("Número de tentativas excedido. Solicite um novo código.");

        if (!_senhas.Conferir(codigo.Trim(), registro.CodigoHash))
        {
            registro.Tentativas++;
            await _banco.SalvarAsync(ct);
            throw new RegraDeNegocioException("Código incorreto. Confira e tente novamente.");
        }

        return registro;
    }

    private static string GerarCodigoDeSeisDigitos()
        => RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");

    private static string NormalizarEmail(string email)
        => email.Trim().ToLowerInvariant();

    private async Task<Usuario> BuscarUsuarioAsync(int id, CancellationToken ct)
    {
        var usuario = await _banco.Usuarios.FirstOrDefaultAsync(u => u.Id == id, ct);
        return usuario ?? throw new RegraDeNegocioException("Usuário não encontrado.");
    }

    private LoginResposta MontarResposta(Usuario usuario)
    {
        return new LoginResposta(
            _tokens.Gerar(usuario),
            _relogio.Agora.AddHours(_tokens.HorasDeValidade),
            Converter(usuario));
    }

    private static UsuarioResposta Converter(Usuario usuario)
        => new(usuario.Id, usuario.Nome, usuario.Email, usuario.Telefone, usuario.Perfil.ToString());
}
