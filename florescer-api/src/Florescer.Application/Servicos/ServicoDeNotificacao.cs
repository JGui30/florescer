using Florescer.Application.Contratos;
using Florescer.Application.Persistencia;
using Florescer.Domain.Entidades;
using Florescer.Domain.Enums;
using Florescer.Domain.Regras;

namespace Florescer.Application.Servicos;

public class ServicoDeNotificacao
{
    private readonly IBancoDeDados _banco;
    private readonly ICanalDeNotificacao _canal;
    private readonly IRelogio _relogio;

    public ServicoDeNotificacao(
        IBancoDeDados banco,
        ICanalDeNotificacao canal,
        IRelogio relogio)
    {
        _banco = banco;
        _canal = canal;
        _relogio = relogio;
    }

    public async Task<Notificacao> EnviarAsync(
        Usuario destinatario,
        TipoNotificacao tipo,
        string assunto,
        string mensagem,
        Agendamento? agendamento = null,
        CancellationToken cancelamento = default)
    {
        var destino = destinatario.Email;

        var textoRegistrado = tipo == TipoNotificacao.CodigoDeRecuperacao
            ? OcultarCodigo(mensagem)
            : mensagem;

        var registro = new Notificacao
        {
            UsuarioId = destinatario.Id,
            AgendamentoId = agendamento?.Id,
            Canal = _canal.Canal,
            Tipo = tipo,
            Destino = destino,
            Assunto = assunto,
            Mensagem = textoRegistrado,
            Status = StatusNotificacao.Pendente,
            CriadaEm = _relogio.Agora
        };

        var resultado = await _canal.EnviarAsync(
            new MensagemParaEnvio(destino, assunto, mensagem), cancelamento);

        registro.Status = resultado.Status;
        registro.Erro = resultado.Erro;
        if (resultado.Sucesso)
            registro.EnviadaEm = _relogio.Agora;

        _banco.Notificacoes.Add(registro);
        await _banco.SalvarAsync(cancelamento);

        return registro;
    }

    private static string OcultarCodigo(string texto)
        => System.Text.RegularExpressions.Regex.Replace(texto, "[0-9]{6}", "******");

    public static string TextoAgendamentoCriado(Agendamento agendamento, string procedimento, string profissional)
    {
        var quando = $"{Textos.DataPorExtenso(agendamento.Inicio)} às {Textos.Hora(agendamento.Inicio)}";

        if (agendamento.Status == StatusAgendamento.Confirmado)
        {
            return $"Olá, {agendamento.Cliente.Nome}!\n\n" +
                   $"Seu agendamento de {procedimento} com {profissional} está confirmado para {quando}.\n\n" +
                   "Como faltam menos de 2 horas para o atendimento, a própria reserva já valeu como confirmação.\n\n" +
                   "Até logo!\nEquipe Florescer";
        }

        return $"Olá, {agendamento.Cliente.Nome}!\n\n" +
               $"Recebemos seu agendamento de {procedimento} com {profissional} para {quando}.\n\n" +
               RegrasDeConfirmacao.MensagemDoPrazo(agendamento.PrazoConfirmacao) + "\n\n" +
               "É rápido: abra o aplicativo, vá em \"Meus agendamentos\" e toque em Confirmar.\n\n" +
               "Até breve!\nEquipe Florescer";
    }

    public static string TextoLembreteDeConfirmacao(Agendamento agendamento, string procedimento)
    {
        var quando = $"{Textos.DataCurta(agendamento.Inicio)} às {Textos.Hora(agendamento.Inicio)}";

        return $"Olá, {agendamento.Cliente.Nome}!\n\n" +
               $"Seu agendamento de {procedimento} em {quando} ainda não foi confirmado.\n\n" +
               $"Você tem até {agendamento.PrazoConfirmacao:dd/MM 'às' HH:mm} para confirmar. " +
               "Sem a confirmação, o horário será liberado para outra cliente.\n\n" +
               "Equipe Florescer";
    }

    public static string TextoAgendamentoConfirmado(Agendamento agendamento, string procedimento, string profissional)
    {
        var quando = $"{Textos.DataPorExtenso(agendamento.Inicio)} às {Textos.Hora(agendamento.Inicio)}";

        return $"Tudo certo, {agendamento.Cliente.Nome}!\n\n" +
               $"Seu {procedimento} com {profissional} está confirmado para {quando}.\n\n" +
               "Pedimos que chegue com 10 minutos de antecedência.\n\n" +
               "Até lá!\nEquipe Florescer";
    }

    public static string TextoLembreteDeAtendimento(Agendamento agendamento, string procedimento, string profissional)
    {
        return $"Olá, {agendamento.Cliente.Nome}!\n\n" +
               $"Passando para lembrar do seu {procedimento} com {profissional} " +
               $"amanhã, {Textos.DataCurta(agendamento.Inicio)}, às {Textos.Hora(agendamento.Inicio)}.\n\n" +
               "Se precisar remarcar, faça isso pelo aplicativo com pelo menos 2 horas de antecedência.\n\n" +
               "Até amanhã!\nEquipe Florescer";
    }

    public static string TextoCanceladoPorFaltaDeConfirmacao(Agendamento agendamento, string procedimento)
    {
        var quando = $"{Textos.DataCurta(agendamento.Inicio)} às {Textos.Hora(agendamento.Inicio)}";

        return $"Olá, {agendamento.Cliente.Nome}.\n\n" +
               $"Seu agendamento de {procedimento} em {quando} foi cancelado porque não recebemos " +
               "a confirmação dentro do prazo, e o horário foi liberado para outra cliente.\n\n" +
               "Sentimos muito! Você pode escolher um novo horário a qualquer momento pelo aplicativo.\n\n" +
               "Equipe Florescer";
    }

    public static string TextoCancelado(Agendamento agendamento, string procedimento)
    {
        var quando = $"{Textos.DataCurta(agendamento.Inicio)} às {Textos.Hora(agendamento.Inicio)}";

        return $"Olá, {agendamento.Cliente.Nome}.\n\n" +
               $"Seu agendamento de {procedimento} em {quando} foi cancelado.\n\n" +
               "Quando quiser, é só escolher um novo horário pelo aplicativo.\n\n" +
               "Equipe Florescer";
    }

    public static string TextoReagendado(Agendamento agendamento, string procedimento)
    {
        var quando = $"{Textos.DataPorExtenso(agendamento.Inicio)} às {Textos.Hora(agendamento.Inicio)}";

        return $"Olá, {agendamento.Cliente.Nome}!\n\n" +
               $"Seu agendamento de {procedimento} foi remarcado para {quando}.\n\n" +
               RegrasDeConfirmacao.MensagemDoPrazo(agendamento.PrazoConfirmacao) + "\n\n" +
               "Equipe Florescer";
    }

    public static string TextoCodigoDeRecuperacao(string nome, string codigo, int minutosDeValidade)
    {
        return $"Olá, {nome}!\n\n" +
               $"Seu código para redefinir a senha é: {codigo}\n\n" +
               $"Ele vale por {minutosDeValidade} minutos e só pode ser usado uma vez.\n\n" +
               "Se não foi você quem pediu, ignore esta mensagem — sua senha continua a mesma.\n\n" +
               "Equipe Florescer";
    }
}
