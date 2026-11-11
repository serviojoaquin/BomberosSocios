namespace MercadoPagoExtension.Entities;

public class Sucursal
{
    public string Id { get; set; }

    public string Name { get; set; }

    public string External_id { get; set; }

    public DateTime Date_created { get; set; }

    public BusinessHours Business_hours { get; set; }

    public Location Localtion { get; set; }
}