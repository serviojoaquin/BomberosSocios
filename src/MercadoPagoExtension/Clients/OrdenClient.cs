using MercadoPago.Resource.MerchantOrder;
using MercadoPagoExtension.Contracts;
using MercadoPagoExtension.Contracts.Orden;
using SociosApp.MercadoPagoExtension.Clients.Interfaces;
using SociosApp.MercadoPagoExtension.Services.Interfaces;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace MercadoPagoExtension.Clients;

public class OrdenClient : IOrderClient
{
    private readonly IMpConfigurationManager _mpConfigurationManager;
    public OrdenClient(IMpConfigurationManager mpConfigurationManager)
    {
        _mpConfigurationManager = mpConfigurationManager;
    }

    public async Task CrearOrdenAsync(string token, string user_id, string external_store_id, string external_pos_id, CrearOrdenRequest request)
    {
        var path = $"{_mpConfigurationManager.MercadoPagoUri}/instore/qr/seller/collectors/{user_id}/stores/{external_store_id}/pos/{external_pos_id}/orders";
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        HttpResponseMessage response = await client.PutAsJsonAsync(path, request);
        if (response.IsSuccessStatusCode)
        {
            var asd = await response.Content.ReadAsStringAsync();
        }
    }

    public async Task<ObtenerOrdenesResponse> ObtenerOrdenesAsync(string token, string user_id, string external_pos_id)
    {
        var path = $"{_mpConfigurationManager.MercadoPagoUri}/instore/qr/seller/collectors/{user_id}/pos/{external_pos_id}/orders";
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        ObtenerOrdenesResponse response = new();

        HttpResponseMessage httpResponse = await client.GetAsync(path);
        if (httpResponse.IsSuccessStatusCode)
        {
            var asd = await httpResponse.Content.ReadAsStringAsync();
            response = await httpResponse.Content.ReadAsAsync<ObtenerOrdenesResponse>();
        }

        return response;
    }

    public async Task<MerchantOrder> ObtenerOrdenAsync(string token, string external_pos_id)
    {
        var path = $"{_mpConfigurationManager.MercadoPagoUri}/merchant_orders?external_reference={external_pos_id}";
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        MerchantOrder response = new();

        HttpResponseMessage httpResponse = await client.GetAsync(path);
        if (httpResponse.IsSuccessStatusCode)
        {
            var asd = await httpResponse.Content.ReadAsStringAsync();
            response = await httpResponse.Content.ReadAsAsync<MerchantOrder>();
        }

        return response;
    }
}


