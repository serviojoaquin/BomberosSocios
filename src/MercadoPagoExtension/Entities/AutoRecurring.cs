using System;

namespace MercadoPagoExtension.Entities;

public class AutoRecurring
{
    public int Frequency { get; set; }

    public string Frequency_type { get; set; }

    public DateTime Start_date { get; set; }

    public DateTime End_date { get; set; }

    public string Currency_id { get; set; }

    public decimal Transaction_amount { get; set; }

    public FreeTrial Free_trial { get; set; }
}


