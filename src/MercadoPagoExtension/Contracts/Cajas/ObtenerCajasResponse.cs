using MercadoPagoExtension.Entities;

namespace MercadoPagoExtension.Contracts.Cajas;

public class ObtenerCajasResponse
{
    public Paging Paging { get; set; }

    public IList<Caja> Results { get; set; }
}