using System;

namespace MercadoPagoExtension.Entities;

public class Caja
{
    public int Id { get; set; }

    public  Qr Qr { get; set; }

    public DateTime Date_created { get; set; }

    public DateTime Date_last_updated { get; set; }

    public string Uuid { get; set; }

    public int User_id { get; set; }

    public string Name { get; set; }

    public bool Fixed_amount { get; set; }

    public string External_id { get; set; }

    public string Site { get; set; }

    public string Qr_code { get; set; }

    public string Status { get; set; }
    
    public string? Url { get; set; }

    public string Store_id { get; set; }

    public string External_store_id { get; set; }

    public int Category { get; set; }

}