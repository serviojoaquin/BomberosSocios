namespace MercadoPagoExtension.Entities;

public class Location
{
    public string Address_line { get; set; }

    public string Reference { get; set; }

    public int Latitud { get; set; }

    public int Longitud { get; set; }
}