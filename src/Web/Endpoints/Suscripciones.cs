using BaseApp.Application.Suscripciones;
using BaseApp.Domain.Enums;
using Microsoft.AspNetCore.Http.HttpResults;

namespace BaseApp.Web.Endpoints;

public class Suscripciones : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.RequireAuthorization().RequirePermission(Permissions.VerMercadoPago);

        groupBuilder.MapGet(GetSuscripciones);
        groupBuilder.MapGet(Register, "register");
        groupBuilder.MapPost(Create);
    }

    [EndpointSummary("Listar suscripciones de Mercado Pago")]
    public static async Task<Ok<SuscripcionesVm>> GetSuscripciones(ISender sender)
    {
        var vm = await sender.Send(new GetSuscripcionesQuery());

        return TypedResults.Ok(vm);
    }

    [EndpointSummary("Categorías disponibles para suscribirse")]
    public static async Task<Ok<RegisterSuscripcionVm>> Register(ISender sender)
    {
        var vm = await sender.Send(new RegisterSuscripcionQuery());

        return TypedResults.Ok(vm);
    }

    [EndpointSummary("Crear una suscripción")]
    public static async Task<Ok<CreateSuscripcionResponse>> Create(ISender sender, HttpContext httpContext, CreateSuscripcionCommand command)
    {
        var response = await sender.Send(command with { BaseUrl = $"{httpContext.Request.Scheme}://{httpContext.Request.Host}" });

        return TypedResults.Ok(response);
    }
}
