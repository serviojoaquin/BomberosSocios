namespace MercadoPagoExtension.Contracts.Cajas;

public class ActualizarSucursalRequest
{
    public string User_id { get; set; }
    public string Store_id { get; set; }
    public string External_id { get; set; }
}