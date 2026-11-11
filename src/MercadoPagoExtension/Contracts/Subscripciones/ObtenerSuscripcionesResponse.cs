using MercadoPagoExtension.Entities;
using System.Collections.Generic;

namespace MercadoPagoExtension.Contracts.Subscripciones;

public class ObtenerSuscripcionesResponse
{
    public Paging Paging { get; set; }

    public IList<Suscripcion> Results { get; set; }
}