using MercadoPagoExtension.Entities;
using MercadoPagoExtension.Contracts.Subscripciones;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace MercadoPagoExtension.Clients;

public class SuscripcionClient
{
    //public static async Task<Subscripcion> ObtenerSubscripcionAsync(string token, string id)
    //{
    //    var path = $"https://api.mercadopago.com/pos/{id}";
    //    Subscripcion subscripcion = null;
    //    using (var client = new HttpClient())
    //    {
    //        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    //        HttpResponseMessage response = await client.GetAsync(path);
    //        if (response.IsSuccessStatusCode)
    //        {
    //            subscripcion = await response.Content.ReadAsAsync<Subscripcion>();
    //        }
    //    }

    //    return subscripcion;
    //}

    public static async Task<ObtenerSuscripcionesResponse> BuscarEnSubscripcionesAsync(string token, int limit)
    {
        string path = $"https://api.mercadopago.com/preapproval/search?limit={limit}";
      
        ObtenerSuscripcionesResponse obtenerCajasResponse = null;
        using (var client = new HttpClient())
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            HttpResponseMessage response = await client.GetAsync(path);
            if (response.IsSuccessStatusCode)
            {
                obtenerCajasResponse = await response.Content.ReadAsAsync<ObtenerSuscripcionesResponse>();
            }
        }

        return obtenerCajasResponse;
    }
    public static async Task<Suscripcion> CrearSuscripcionAsync(string token, CrearSuscripcionRequest request)
    {
        var path = $"https://api.mercadopago.com/preapproval";
        Suscripcion subscripcion = null;
        using (var client = new HttpClient())
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            HttpResponseMessage response = await client.PostAsJsonAsync(path, request);
            if (response.IsSuccessStatusCode)
            {
                //TODO hay qeu manejar las distintas respuestas y mostrar error en pantallla.
                subscripcion = await response.Content.ReadAsAsync<Suscripcion>();
            }
        }
        return subscripcion;
    }
}


