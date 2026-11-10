using BaseApp.Application.Cobradores.Commands.CrearUsuarioCobrador;
using BaseApp.Application.Cobradores.Commands.CreateCobrador;
using BaseApp.Application.Cobradores.Commands.DeleteCobrador;
using BaseApp.Application.Cobradores.Commands.UpdateCobrador;
using BaseApp.Application.Cobradores.Queries.GetCobradores;
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
