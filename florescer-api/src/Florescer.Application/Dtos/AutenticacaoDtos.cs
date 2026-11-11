namespace Florescer.Application.Dtos;

public record RegistroRequisicao(
    string Nome,
    string Email,
    string Senha);

public record LoginRequisicao(
    string Email,
    string Senha);

public record UsuarioResposta(
    int Id,
    string Nome,
    string Email,
    string? Telefone,
    string Perfil);

public record LoginResposta(
    string Token,
    DateTime ExpiraEm,
    UsuarioResposta Usuario);

public record AtualizarPerfilRequisicao(
    string Nome,
    string Email,
    string? Telefone);

public record AlterarSenhaRequisicao(
    string SenhaAtual,
    string NovaSenha);

public record SolicitarRecuperacaoRequisicao(string Email);

public record SolicitarRecuperacaoResposta(
    string Mensagem,
    string DestinoMascarado,
    string? CodigoParaDemonstracao);

public record RedefinirSenhaRequisicao(
    string Email,
    string Codigo,
    string NovaSenha);
