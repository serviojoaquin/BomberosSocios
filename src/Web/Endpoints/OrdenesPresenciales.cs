using BaseApp.Application.OrdenesPresenciales;
using BaseApp.Domain.Enums;
using MercadoPagoExtension.Contracts.Orden;
using Microsoft.AspNetCore.Http.HttpResults;

namespace BaseApp.Web.Endpoints;

public class OrdenesPresenciales : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.RequireAuthorization();

        groupBuilder.MapPost(Crear).RequirePermission(Permissions.PagosMercadoPago);
        groupBuilder.MapGet(Get).RequirePermission(Permissions.VerMercadoPago);
    }

    [EndpointSummary("Crear una orden presencial (QR)")]
    [EndpointDescription("Crea en Mercado Pago una orden con las cuotas indicadas para cobrarlas con el QR de la caja.")]
    public static async Task<NoContent> Crear(ISender sender, CrearOrdenCommand command)
    {
        await sender.Send(command);

        return TypedResults.NoContent();
    }

    [EndpointSummary("Órdenes de una caja")]
    public static async Task<Ok<ObtenerOrdenesResponse>> Get(ISender sender, string externalPosId)
    {
        var response = await sender.Send(new ObtenerOrdenesQuery(externalPosId));

        return TypedResults.Ok(response);
    }
}
