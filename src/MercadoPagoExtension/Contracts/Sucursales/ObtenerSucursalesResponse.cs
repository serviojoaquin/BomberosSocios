using MercadoPagoExtension.Entities;

namespace MercadoPagoExtension.Contracts.Sucursales;

public class ObtenerSucursalesResponse
{
    public Paging Paging { get; set; } = new();

    public IList<Sucursal> Results { get; set; } = new List<Sucursal>();
}