using System;

namespace MercadoPagoExtension.Entities;

public class Summarized
{
    public int Quotas { get; set; }

    public int Charged_quantity { get; set; }

    public decimal Charged_amount { get; set; }

    public int Pending_charge_quantity { get; set; }

    public decimal Pending_charge_amount { get; set; }

    public DateTime Last_charged_date { get; set; }

    public decimal Last_charged_amount { get; set; }

    public string Semaphore { get; set; }
}


