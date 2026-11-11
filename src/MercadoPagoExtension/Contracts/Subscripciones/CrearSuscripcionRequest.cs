using MercadoPagoExtension.Entities;
using System.Collections.Generic;

namespace MercadoPagoExtension.Contracts.Subscripciones;

public class CrearSuscripcionRequest
{
    public string Back_url { get; set; }
    public string Payer_email { get; set; }
    public string Card_token_id { get; set; }
    public string External_reference { get; set; }
    public string Preapproval_plan_id { get; set; }
    public string Reason { get; set; }
    public string Status { get; set; }
    public AutoRecurring Auto_recurring { get; set; }
}


