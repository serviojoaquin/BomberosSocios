using BaseApp.Application.Cajas;
using BaseApp.Domain.Enums;
using Microsoft.AspNetCore.Http.HttpResults;

namespace BaseApp.Web.Endpoints;

public class Cajas : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.RequireAuthorization();

        groupBuilder.MapGet(GetCajas).RequirePermission(Permissions.VerMercadoPago);
        groupBuilder.MapGet(GetCurrentCaja, "current").RequirePermission(Permissions.PagosMercadoPago);
        groupBuilder.MapPost(CreateCajaPrincipal, "principal").RequirePermission(Permissions.AdministrarMercadoPago);
        groupBuilder.MapDelete(DeleteCaja, "{id}").RequirePermission(Permissions.AdministrarMercadoPago);
    }

    [EndpointSummary("Listar cajas de Mercado Pago")]
    public static async Task<Ok<CajasVm>> GetCajas(ISender sender)
    {
        var vm = await sender.Send(new GetCajasQuery());

        return TypedResults.Ok(vm);
    }

    [EndpointSummary("Caja actual")]
    [EndpointDescription("Devuelve la caja del cobrador logueado o la caja principal.")]
    public static async Task<Ok<CajaVm>> GetCurrentCaja(ISender sender)
    {
        var vm = await sender.Send(new GetCurrentCajaQuery());

        return TypedResults.Ok(vm);
    }

    [EndpointSummary("Crear la caja principal")]
    public static async Task<Ok<CajaDto?>> CreateCajaPrincipal(ISender sender)
    {
        var caja = await sender.Send(new CrearCajaPrincipalCommand());

        return TypedResults.Ok<CajaDto?>(caja);
    }

    [EndpointSummary("Eliminar una caja")]
    public static async Task<NoContent> DeleteCaja(ISender sender, int id)
    {
        await sender.Send(new DeleteCajaCommand(id));

        return TypedResults.NoContent();
    }
}
