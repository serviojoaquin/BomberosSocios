using BaseApp.Application.Common.Interfaces;
using MercadoPago.Client.MerchantOrder;
using MercadoPago.Config;
using MercadoPago.Resource.MerchantOrder;
using MercadoPagoExtension.Clients;
using MercadoPagoExtension.Contracts;
using MercadoPagoExtension.Contracts.Cajas;
using MercadoPagoExtension.Contracts.Orden;
using MercadoPagoExtension.Contracts.Subscripciones;
using MercadoPagoExtension.Contracts.Sucursales;
using MercadoPagoExtension.Entities;
using SociosApp.MercadoPagoExtension.Clients.Interfaces;
using SociosApp.MercadoPagoExtension.Services.Interfaces;

namespace BaseApp.Infrastructure.Services;

public class MercadoPagoService : IMercadoPagoService
{
    private readonly IOrderClient _orderClient;
    private readonly IMpConfigurationManager _mpConfigurationManager;

    public MercadoPagoService(IOrderClient orderClient, IMpConfigurationManager mpConfigurationManager)
    {
        _orderClient = orderClient;
        _mpConfigurationManager = mpConfigurationManager;
    }

    public async Task<ObtenerSucursalesResponse> BuscarEnSucursalesAsync(string? external_id = null)
    {
        return await SucursalClient.BuscarEnSucursalesAsync(_mpConfigurationManager.Token, _mpConfigurationManager.UserId, external_id);
    }

    public async Task<Sucursal> ActualizarSucursalAsync(ActualizarSucursalRequest request)
    {
        request.Store_id = _mpConfigurationManager.StoreId;
        request.User_id = _mpConfigurationManager.UserId;
        return await SucursalClient.ActualizarSucursalAsync(_mpConfigurationManager.Token, request);
    }

    public async Task<Caja> ObtenerCajaAsync(string id)
    {
        return await CajaClient.ObtenerCajaAsync(_mpConfigurationManager.Token, id);
    }

    public async Task<Caja> ActualizarCajaAsync(int id, ActualizarCajaRequest request)
    {
        request.StoreId = _mpConfigurationManager.StoreId;
        request.ExternalStoreId = _mpConfigurationManager.StoreExternalId;
        return await CajaClient.ActualizarCajaAsync(_mpConfigurationManager.Token, id, request);
    }

    public async Task<Caja> CrearCajaAsync(CrearCajaRequest request)
    {
        request.Store_id = _mpConfigurationManager.StoreId;
        request.External_store_id = _mpConfigurationManager.StoreExternalId;
        return await CajaClient.CrearCajaAsync(_mpConfigurationManager.Token, request);
    }

    public async Task<bool> DeleteCajaAsync(int id)
    {
        return await CajaClient.DeleteCajaAsync(_mpConfigurationManager.Token, id);
    }

    public async Task<ObtenerCajasResponse> BuscarEnCajasAsync(string? external_id, string? external_store_id = null, string? category = null)
    {
        return await CajaClient.BuscarEnCajasAsync(_mpConfigurationManager.Token, external_id, external_store_id, _mpConfigurationManager.StoreId, category);
    }

    public async Task CrearOrdenAsync(string external_pos_id, CrearOrdenRequest request)
    {
        request.Sponsor.Id = Convert.ToInt64(_mpConfigurationManager.SponsorId);
        await _orderClient.CrearOrdenAsync(_mpConfigurationManager.Token, _mpConfigurationManager.UserId, _mpConfigurationManager.StoreExternalId, external_pos_id, request);
    }

    public async Task<MerchantOrder> ObtenerOrdenAsync(int merchant_order_id)
    {
        MercadoPagoConfig.AccessToken = _mpConfigurationManager.Token;
        var orderClient = new MerchantOrderClient();
        return await orderClient.GetAsync(merchant_order_id);
    }

    public async Task<MerchantOrder> ObtenerOrdenAsync(string merchant_order_id)
    {
        return await _orderClient.ObtenerOrdenAsync(_mpConfigurationManager.Token, merchant_order_id);
    }

    public async Task<ObtenerOrdenesResponse> ObtenerOrdenesAsync(string externalPosId)
    {
        return await _orderClient.ObtenerOrdenesAsync(_mpConfigurationManager.Token, _mpConfigurationManager.UserId, externalPosId);
    }

    public async Task<ObtenerSuscripcionesResponse> BuscarEnSubscripcionesAsync(int limit)
    {
        return await SuscripcionClient.BuscarEnSubscripcionesAsync(_mpConfigurationManager.Token, limit);
    }

    public async Task<Suscripcion> CrearSuscripcionAsync(CrearSuscripcionRequest request)
    {
        return await SuscripcionClient.CrearSuscripcionAsync(_mpConfigurationManager.Token, request);
    }
}
