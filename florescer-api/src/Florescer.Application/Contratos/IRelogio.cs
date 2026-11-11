namespace Florescer.Application.Contratos;

public interface IRelogio
{
    DateTime Agora { get; }

    DateTime Hoje { get; }
}
