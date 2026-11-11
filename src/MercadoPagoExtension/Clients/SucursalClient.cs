using MercadoPagoExtension.Contracts.Cajas;
using MercadoPagoExtension.Contracts.Sucursales;
using MercadoPagoExtension.Entities;
using System.Net.Http.Headers;

namespace MercadoPagoExtension.Clients;

public class SucursalClient
{
    public static async Task<ObtenerSucursalesResponse> BuscarEnSucursalesAsync(string token, string user_id, string? external_id = null)
    {
        string path = $"https://api.mercadopago.com/users/{user_id}/stores/search";
        if (!string.IsNullOrEmpty(external_id))
        {
            path = $"{path}?external_id={external_id}";
        }
        ObtenerSucursalesResponse obtenerCajasResponse = new();
        using (var client = new HttpClient())
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await client.GetAsync(path);
            if (response.IsSuccessStatusCode)
            {
                obtenerCajasResponse = await response.Content.ReadAsAsync<ObtenerSucursalesResponse>();
            }
        }

        return obtenerCajasResponse;
    }

    public static async Task<Sucursal> ActualizarSucursalAsync(string token, ActualizarSucursalRequest request)
    {
        var path = $"https://api.mercadopago.com/users/{request.User_id}/stores/{request.Store_id}";
        Sucursal sucursal = new();
        using (var client = new HttpClient())
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await client.PutAsJsonAsync(path, new
            {
                external_id = request.External_id
            });
            if (response.IsSuccessStatusCode)
            {
                sucursal = await response.Content.ReadAsAsync<Sucursal>();
            }
        }
        return sucursal;
    }
}


