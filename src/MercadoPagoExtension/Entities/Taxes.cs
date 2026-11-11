using System;
using Newtonsoft.Json;

namespace MercadoPagoExtension.Entities;

public class Taxes
{
    [JsonProperty("value")]
    public decimal Value { get; set; }

    [JsonProperty("type")]
    public string Type { get; set; }
}


