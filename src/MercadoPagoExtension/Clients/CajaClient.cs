using MercadoPagoExtension.Contracts.Subscripciones;
using MercadoPagoExtension.Entities;
using MercadoPagoExtension.Contracts.Cajas;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace MercadoPagoExtension.Clients;

//TODO Create Interface and inyect!

public class CajaClient
{
    public static async Task<Caja> ObtenerCajaAsync(string token, string id)
    {
        var path = $"https://api.mercadopago.com/pos/{id}";
        Caja caja = new();
        using (var client = new HttpClient())
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await client.GetAsync(path);
            if (response.IsSuccessStatusCode)
            {
                caja = await response.Content.ReadAsAsync<Caja>();
            }
        }

        return caja;
    }

    public static async Task<ObtenerCajasResponse> BuscarEnCajasAsync(string token, string? external_id, string? external_store_id, string? store_id, string? category)
    {
        string path = $"https://api.mercadopago.com/pos?";
        if (!string.IsNullOrEmpty(external_id))
        {
            path = path[^1] == '?' ? $"{path}" : $"{path}&";
            path = $"{path}external_id={external_id}";
        }
        if (!string.IsNullOrEmpty(external_store_id))
        {
            path = path[^1] == '?' ? $"{path}" : $"{path}&";
            path = $"{path}external_store_id={external_store_id}";
        }
        if (!string.IsNullOrEmpty(store_id))
        {
            path = path[^1] == '?' ? $"{path}" : $"{path}&";
            path = $"{path}store_id={store_id}";
        }
        if (!string.IsNullOrEmpty(category))
        {
            path = path[^1] == '?' ? $"{path}" : $"{path}&";
            path = $"{path}category={category}";
        }
        ObtenerCajasResponse obtenerCajasResponse = new();
        using (var client = new HttpClient())
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await client.GetAsync(path);
            if (response.IsSuccessStatusCode)
            {
                obtenerCajasResponse = await response.Content.ReadAsAsync<ObtenerCajasResponse>();
            }
        }

        return obtenerCajasResponse;
    }

    public static async Task<Caja> ActualizarCajaAsync(string token, int id, ActualizarCajaRequest request)
    {
        var path = $"https://api.mercadopago.com/pos/{id}";
        Caja caja = new();
        using (var client = new HttpClient())
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await client.PutAsJsonAsync(path, new
            {
                external_id = request.ExternalId,
                store_id = request.StoreId,
                fixed_amount = false,
                category = request.Category,
                external_store_id = request.ExternalStoreId,
            });
            if (response.IsSuccessStatusCode)
            {
                caja = await response.Content.ReadAsAsync<Caja>();
            }
        }
        return caja;
    }

    public static async Task<bool> DeleteCajaAsync(string token, int id)
    {
        var path = $"https://api.mercadopago.com/pos/{id}";
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.DeleteAsync(path);
        return response.IsSuccessStatusCode;
    }

    public static async Task<Caja> CrearCajaAsync(string token, CrearCajaRequest request)
    {
        var path = $"https://api.mercadopago.com/pos";
        Caja caja = new();
        using (var client = new HttpClient())
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await client.PostAsJsonAsync(path, new
            {
                name = request.Name,
                fixed_amount = false,
                store_id = request.Store_id,
                external_store_id = request.External_store_id,
                external_id = request.External_id,
                category = request.Category,
            });

            if (response.IsSuccessStatusCode)
            {
                caja = await response.Content.ReadAsAsync<Caja>();
            }
        }
        return caja;
    }
}