using MercadoPago.Resource.MerchantOrder;
using MercadoPagoExtension.Contracts;
using MercadoPagoExtension.Contracts.Cajas;
using MercadoPagoExtension.Contracts.Orden;
using MercadoPagoExtension.Contracts.Subscripciones;
using MercadoPagoExtension.Contracts.Sucursales;
using MercadoPagoExtension.Entities;

namespace BaseApp.Application.Common.Interfaces;

public interface IMercadoPagoService
{
    Task<ObtenerSucursalesResponse> BuscarEnSucursalesAsync(string? external_id = null);

    Task<Sucursal> ActualizarSucursalAsync(ActualizarSucursalRequest request);

    Task<Caja> ObtenerCajaAsync(string id);

    Task<Caja> ActualizarCajaAsync(int id, ActualizarCajaRequest request);

    Task<Caja> CrearCajaAsync(CrearCajaRequest request);

    Task<bool> DeleteCajaAsync(int id);

    Task<ObtenerCajasResponse> BuscarEnCajasAsync(string? external_id, string? external_store_id = null, string? category = null);

    Task CrearOrdenAsync(string external_pos_id, CrearOrdenRequest request);

    Task<MerchantOrder> ObtenerOrdenAsync(int merchant_order_id);

    Task<MerchantOrder> ObtenerOrdenAsync(string merchant_order_id);

    Task<ObtenerOrdenesResponse> ObtenerOrdenesAsync(string externalPosId);

    Task<ObtenerSuscripcionesResponse> BuscarEnSubscripcionesAsync(int limit);

    Task<Suscripcion> CrearSuscripcionAsync(CrearSuscripcionRequest request);
}
