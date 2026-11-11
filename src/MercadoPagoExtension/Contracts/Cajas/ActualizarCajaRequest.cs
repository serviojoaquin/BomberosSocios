using Newtonsoft.Json;

namespace MercadoPagoExtension.Contracts.Cajas;

public class ActualizarCajaRequest
{
    [JsonProperty("category")]
    public int? Category { get; set; }

    [JsonProperty("external_store_id")]
    public string ExternalStoreId { get; set; }

    [JsonProperty("external_id")]
    public string ExternalId { get; set; }

    [JsonProperty("fixed_amount")]
    public bool FixedAmount { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("store_id")]
    public string StoreId { get; set; }

    [JsonProperty("url")]
    public string Url { get; set; }
}