using MercadoPago.Resource.MerchantOrder;
using MercadoPagoExtension.Contracts;
using MercadoPagoExtension.Contracts.Orden;

namespace SociosApp.MercadoPagoExtension.Clients.Interfaces;

public interface IOrderClient
{
    Task CrearOrdenAsync(string token, string user_id, string external_store_id, string external_pos_id, CrearOrdenRequest request);

    Task<ObtenerOrdenesResponse> ObtenerOrdenesAsync(string token, string user_id, string external_pos_id);

    Task<MerchantOrder> ObtenerOrdenAsync(string token, string external_pos_id);
}