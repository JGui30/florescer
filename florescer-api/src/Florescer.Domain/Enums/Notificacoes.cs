namespace Florescer.Domain.Enums;

public enum CanalNotificacao
{
    Email = 1,
    Sms = 2,
    WhatsApp = 3
}

public enum TipoNotificacao
{
    AgendamentoCriado = 1,
    LembreteDeConfirmacao = 2,
    AgendamentoConfirmado = 3,
    LembreteDeAtendimento = 4,
    CanceladoPorFaltaDeConfirmacao = 5,
    AgendamentoCancelado = 6,
    AgendamentoReagendado = 7,
    CodigoDeRecuperacao = 8
}

public enum StatusNotificacao
{
    Pendente = 1,
    Enviada = 2,
    Falhou = 3,
    Simulada = 4
}
