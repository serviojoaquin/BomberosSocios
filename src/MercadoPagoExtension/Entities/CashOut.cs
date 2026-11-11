using System;
using Newtonsoft.Json;

namespace MercadoPagoExtension.Entities;

public class CashOut
{
    [JsonProperty("amount")]

    public decimal Amount { get; set; }
}


