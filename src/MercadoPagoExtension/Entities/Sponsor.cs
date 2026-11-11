using System;
using Newtonsoft.Json;

namespace MercadoPagoExtension.Entities;

public class Sponsor
{
    /// <summary>
    /// USER_ID de la cuenta de Mercado Pago del sistema integrador
    /// </summary>
    [JsonProperty("id")]
    public long Id { get; set; }
}


