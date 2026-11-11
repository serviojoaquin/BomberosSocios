using BaseApp.Application.Cobradores.Commands.CrearUsuarioCobrador;
using BaseApp.Application.Cobradores.Commands.CreateCobrador;
using BaseApp.Application.Cobradores.Commands.DeleteCobrador;
using BaseApp.Application.Cobradores.Commands.UpdateCobrador;
using BaseApp.Application.Cobradores.Queries.GetCobradores;
using BaseApp.Application.Cobradores.Queries.GetCobradorCuotasCobradas;
using BaseApp.Application.Cobradores.Queries.GetCobradorCuotasImpagas;
using BaseApp.Application.Cuotas.Queries.GetCuotasImprimir;
using BaseApp.Domain.Enums;
using Microsoft.AspNetCore.Http.HttpResults;

namespace BaseApp.Web.Endpoints;

public class Cobradores : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.RequireAuthorization();

        groupBuilder.MapGet(GetCobradores).RequirePermission(Permissions.VerCobradores);
        groupBuilder.MapPost(CreateCobrador).RequirePermission(Permissions.CrearCobradores);
        groupBuilder.MapPut(UpdateCobrador, "{id}").RequirePermission(Permissions.EditarCobradores);
        groupBuilder.MapDelete(DeleteCobrador, "{id}").RequirePermission(Permissions.EliminarCobradores);
        groupBuilder.MapPost(CrearUsuarioCobrador, "{id}/usuario").RequirePermission(Permissions.ManageSecurity);
        groupBuilder.MapGet(GetCuotasCobradas, "{id}/cuotas-cobradas/{year}").RequirePermission(Permissions.VerCobradores);
        groupBuilder.MapGet(GetCuotasImpagas, "{id}/cuotas-impagas/{year}").RequirePermission(Permissions.VerCobradores);
        groupBuilder.MapGet(GetCuotasImprimir, "{id}/cuotas-imprimir/{year}").RequirePermission(Permissions.VerCuotas);
    }

    [EndpointSummary("Cuotas cobradas por un cobrador")]
    [EndpointDescription("Devuelve por socio los importes cobrados en cada mes del año, opcionalmente acotado por rango de fecha de cobro.")]
    public static async Task<Ok<CobradorCuotasCobradasVm>> GetCuotasCobradas(ISender sender, int id, short year, DateTime? fechaDeCobroFrom = null, DateTime? fechaDeCobroTo = null)
    {
        var vm = await sender.Send(new GetCobradorCuotasCobradasQuery(id, year, fechaDeCobroFrom, fechaDeCobroTo));

        return TypedResults.Ok(vm);
    }

    [EndpointSummary("Cuotas impagas de los socios de un cobrador")]
    [EndpointDescription("Devuelve por socio los importes impagos de cada mes del año.")]
    public static async Task<Ok<CobradorCuotasImpagasVm>> GetCuotasImpagas(ISender sender, int id, short year)
    {
        var vm = await sender.Send(new GetCobradorCuotasImpagasQuery(id, year));

        return TypedResults.Ok(vm);
    }

    [EndpointSummary("Cuotas de los socios de un cobrador para imprimir")]
    [EndpointDescription("Devuelve hasta 50 socios activos del cobrador con sus 12 cuotas del año para armar los reportes imprimibles.")]
    public static async Task<Ok<CuotasImprimirVm>> GetCuotasImprimir(ISender sender, int id, int year, bool regenerar = false)
    {
        var vm = await sender.Send(new GetCuotasImprimirQuery(id, null, year, regenerar));

        return TypedResults.Ok(vm);
    }

    [EndpointSummary("Listar cobradores")]
    [EndpointDescription("Devuelve todos los cobradores ordenados por apellido y nombre.")]
    public static async Task<Ok<CobradoresVm>> GetCobradores(ISender sender)
    {
        var vm = await sender.Send(new GetCobradoresQuery());

        return TypedResults.Ok(vm);
    }

    [EndpointSummary("Crear cobrador")]
    [EndpointDescription("Crea un cobrador y devuelve su ID.")]
    public static async Task<Created<int>> CreateCobrador(ISender sender, CreateCobradorCommand command)
    {
        var id = await sender.Send(command);

        return TypedResults.Created($"/{nameof(Cobradores)}/{id}", id);
    }

    [EndpointSummary("Modificar cobrador")]
    [EndpointDescription("Modifica el cobrador indicado. El ID de la URL debe coincidir con el del cuerpo.")]
    public static async Task<Results<NoContent, BadRequest>> UpdateCobrador(ISender sender, int id, UpdateCobradorCommand command)
    {
        if (id != command.Id) return TypedResults.BadRequest();

        await sender.Send(command);

        return TypedResults.NoContent();
    }

    [EndpointSummary("Crear usuario del cobrador")]
    [EndpointDescription("Crea el usuario del cobrador con su email y la contraseña inicial indicada, o le asigna el rol Cobrador si ya existe. Devuelve el ID del usuario.")]
    public static async Task<Results<Created<string>, BadRequest>> CrearUsuarioCobrador(ISender sender, int id, CrearUsuarioCobradorCommand command)
    {
        if (id != command.Id) return TypedResults.BadRequest();

        var usuarioId = await sender.Send(command);

        return TypedResults.Created($"/Users/{usuarioId}", usuarioId);
    }

    [EndpointSummary("Eliminar cobrador")]
    [EndpointDescription("Elimina el cobrador indicado.")]
    public static async Task<NoContent> DeleteCobrador(ISender sender, int id)
    {
        await sender.Send(new DeleteCobradorCommand(id));

        return TypedResults.NoContent();
    }
}
