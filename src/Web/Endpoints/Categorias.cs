using BaseApp.Application.Categorias.Commands.CreateCategoria;
using BaseApp.Application.Categorias.Commands.DeleteCategoria;
using BaseApp.Application.Categorias.Commands.UpdateCategoria;
using BaseApp.Application.Categorias.Queries.GetCategorias;
using BaseApp.Domain.Enums;
using Microsoft.AspNetCore.Http.HttpResults;

namespace BaseApp.Web.Endpoints;

public class Categorias : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.RequireAuthorization();

        groupBuilder.MapGet(GetCategorias).RequirePermission(Permissions.VerCategorias);
        groupBuilder.MapPost(CreateCategoria).RequirePermission(Permissions.CrearCategorias);
        groupBuilder.MapPut(UpdateCategoria, "{id}").RequirePermission(Permissions.EditarCategorias);
        groupBuilder.MapDelete(DeleteCategoria, "{id}").RequirePermission(Permissions.EliminarCategorias);
    }

    [EndpointSummary("Listar categorías")]
    [EndpointDescription("Devuelve todas las categorías de socios con sus importes mensuales.")]
    public static async Task<Ok<CategoriasVm>> GetCategorias(ISender sender)
    {
        var vm = await sender.Send(new GetCategoriasQuery());

        return TypedResults.Ok(vm);
    }

    [EndpointSummary("Crear categoría")]
    [EndpointDescription("Crea una categoría con sus 12 importes mensuales y devuelve su ID.")]
    public static async Task<Created<int>> CreateCategoria(ISender sender, CreateCategoriaCommand command)
    {
        var id = await sender.Send(command);

        return TypedResults.Created($"/{nameof(Categorias)}/{id}", id);
    }

    [EndpointSummary("Modificar categoría")]
    [EndpointDescription("Modifica la categoría indicada. El ID de la URL debe coincidir con el del cuerpo.")]
    public static async Task<Results<NoContent, BadRequest>> UpdateCategoria(ISender sender, int id, UpdateCategoriaCommand command)
    {
        if (id != command.Id) return TypedResults.BadRequest();

        await sender.Send(command);

        return TypedResults.NoContent();
    }

    [EndpointSummary("Eliminar categoría")]
    [EndpointDescription("Elimina la categoría indicada.")]
    public static async Task<NoContent> DeleteCategoria(ISender sender, int id)
    {
        await sender.Send(new DeleteCategoriaCommand(id));

        return TypedResults.NoContent();
    }
}
