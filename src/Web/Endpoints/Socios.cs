using BaseApp.Application.Socios.Commands.CreateSocio;
using BaseApp.Application.Socios.Commands.DeleteSocio;
using BaseApp.Application.Socios.Commands.UpdateDatosAfiliacion;
using BaseApp.Application.Socios.Commands.UpdateSocio;
using BaseApp.Application.Socios.Queries.GetSocios;
using BaseApp.Application.Cuotas.Queries.GetCuotasImprimir;
using BaseApp.Domain.Enums;
using Microsoft.AspNetCore.Http.HttpResults;

namespace BaseApp.Web.Endpoints;

public class Socios : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.RequireAuthorization();

        groupBuilder.MapGet(GetSocios).RequirePermission(Permissions.VerSocios);
        groupBuilder.MapPost(CreateSocio).RequirePermission(Permissions.CrearSocios);
        groupBuilder.MapPut(UpdateSocio, "{id}").RequirePermission(Permissions.EditarSocios);
        groupBuilder.MapPut(UpdateDatosAfiliacion, "afiliacion").RequirePermission(Permissions.EditarSocios);
        groupBuilder.MapDelete(DeleteSocio, "{id}").RequirePermission(Permissions.EliminarSocios);
        groupBuilder.MapGet(GetSocioCuotasImprimir, "{id}/cuotas-imprimir/{year}").RequirePermission(Permissions.VerCuotas);
    }

    [EndpointSummary("Cuotas de un socio para imprimir")]
    [EndpointDescription("Devuelve el socio con sus 12 cuotas del año para armar el reporte imprimible.")]
    public static async Task<Ok<CuotasImprimirVm>> GetSocioCuotasImprimir(ISender sender, int id, int year, bool regenerar = false)
    {
        var vm = await sender.Send(new GetCuotasImprimirQuery(null, id, year, regenerar));

        return TypedResults.Ok(vm);
    }

    [EndpointSummary("Listar socios")]
    [EndpointDescription("Devuelve los socios visibles para el usuario (un cobrador solo ve los suyos) junto con las categorías y cobradores para los selectores.")]
    public static async Task<Ok<SociosVm>> GetSocios(ISender sender)
    {
        var vm = await sender.Send(new GetSociosQuery());

        return TypedResults.Ok(vm);
    }

    [EndpointSummary("Crear socio")]
    [EndpointDescription("Da de alta un socio y devuelve su ID.")]
    public static async Task<Created<int>> CreateSocio(ISender sender, CreateSocioCommand command)
    {
        var id = await sender.Send(command);

        return TypedResults.Created($"/{nameof(Socios)}/{id}", id);
    }

    [EndpointSummary("Modificar socio")]
    [EndpointDescription("Modifica el socio indicado. El ID de la URL debe coincidir con el del cuerpo.")]
    public static async Task<Results<NoContent, BadRequest>> UpdateSocio(ISender sender, int id, UpdateSocioCommand command)
    {
        if (id != command.Id) return TypedResults.BadRequest();

        await sender.Send(command);

        return TypedResults.NoContent();
    }

    [EndpointSummary("Modificar datos de afiliación en lote")]
    [EndpointDescription("Cambia categoría, cobrador y/o estado de varios socios a la vez.")]
    public static async Task<NoContent> UpdateDatosAfiliacion(ISender sender, UpdateDatosAfiliacionCommand command)
    {
        await sender.Send(command);

        return TypedResults.NoContent();
    }

    [EndpointSummary("Eliminar socio")]
    [EndpointDescription("Elimina el socio indicado.")]
    public static async Task<NoContent> DeleteSocio(ISender sender, int id)
    {
        await sender.Send(new DeleteSocioCommand(id));

        return TypedResults.NoContent();
    }
}
