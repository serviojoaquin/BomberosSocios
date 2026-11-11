using BaseApp.Application.OrdenesPresenciales;
using Microsoft.AspNetCore.Http.HttpResults;

namespace BaseApp.Web.Endpoints;

public class Notifications : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapPost(Receive).AllowAnonymous();
    }

    [EndpointSummary("Webhook de Mercado Pago")]
    [EndpointDescription("Recibe las notificaciones de Mercado Pago y actualiza las cuotas según el estado de la orden.")]
    public static async Task<Ok> Receive(ISender sender, ILogger<Notifications> logger, string topic, int id)
    {
        logger.LogInformation(">>> MercadoPago: llegó notificación con topic {Topic} e id {Id}", topic, id);

        if (topic == "merchant_order")
        {
            await sender.Send(new ManageOrdenNotificationCommand(id));
        }

        return TypedResults.Ok();
    }
}
