namespace MercadoPagoExtension.Contracts.Cajas;

public class CrearCajaRequest
{
    public int? Category { get; set; }
    public string? External_id { get; set; }
    public string? External_store_id { get; set; }
    public string? Store_id { get; set; }
    public bool Fixed_amount { get; set; }
    public string? Name { get; set; }
    public string? Url { get; set; }
}