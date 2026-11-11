using MercadoPagoExtension.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace MercadoPagoExtension.Contracts;

public class CrearOrdenRequest
{
    [JsonProperty("cash_out")]
    public CashOut Cash_out { get; set; } = new();

    [JsonProperty("description")]
    public string? Description { get; set; }

    [JsonProperty("title")]
    public string? Title { get; set; }

    [JsonProperty("expiration_date")]
    public string? ExpirationDate { get; set; }

    [JsonProperty("external_reference")]
    public string? ExternalReference { get; set; }

    [JsonProperty("items")]
    public IList<Items> Items { get; set; } = new List<Items>();

    [JsonProperty("notification_url")]
    public string? NotificationUrl { get; set; }

    [JsonProperty("sponsor")]
    public Sponsor Sponsor { get; set; } = new();

    [JsonProperty("taxes")]
    public IList<Taxes> Taxes { get; set; } = new List<Taxes>();

    [JsonProperty("total_amount")]
    public decimal TotalAmount { get; set; }
}