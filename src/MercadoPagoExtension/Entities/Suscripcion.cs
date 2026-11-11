using System;

namespace MercadoPagoExtension.Entities;

public class Suscripcion
{
    public string Id { get; set; }

    public int Version { get; set; }

    public long Application_id { get; set; }

    public long Collector_id { get; set; }

    public string Preapproval_plan_id { get; set; }

    public string Reason { get; set; }

    public string External_reference { get; set; }

    public string Back_url { get; set; }

    public string Init_point { get; set; }

    public AutoRecurring Auto_recurring { get; set; }

    public int First_invoice_offset { get; set; }

    public int Payer_id { get; set; }

    public string Payer_first_name { get; set; }

    public string Payer_last_name { get; set; }

    public int Card_id { get; set; }

    public string Payment_method_id { get; set; }

    public DateTime Next_payment_date { get; set; }

    public DateTime Date_created { get; set; }

    public DateTime Last_modified { get; set; }

    public Summarized Summarized { get; set; }

    public string Status { get; set; }
}


