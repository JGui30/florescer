using Florescer.Application.Contratos;

namespace Florescer.Infrastructure.Seguranca;

public class RelogioDoSistema : IRelogio
{
    public DateTime Agora => DateTime.Now;

    public DateTime Hoje => DateTime.Today;
}
