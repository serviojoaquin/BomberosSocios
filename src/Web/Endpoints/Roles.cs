using BaseApp.Application.Roles.Commands.CreateRole;
using BaseApp.Application.Roles.Commands.DeleteRole;
using BaseApp.Application.Roles.Commands.UpdateRole;
using BaseApp.Application.Roles.Queries.GetRoles;
using BaseApp.Domain.Enums;
using Microsoft.AspNetCore.Http.HttpResults;

namespace BaseApp.Web.Endpoints;

public class Roles : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.RequireAuthorization().RequirePermission(Permissions.ManageSecurity);

        groupBuilder.MapGet(GetRoles);
        groupBuilder.MapPost(CreateRole);
        groupBuilder.MapPut(UpdateRole, "{id}");
        groupBuilder.MapDelete(DeleteRole, "{id}");
    }

    [EndpointSummary("Get all Roles")]
    [EndpointDescription("Retrieves all roles with their permissions and the list of available permissions.")]
    public static async Task<Ok<RolesVm>> GetRoles(ISender sender)
    {
        var vm = await sender.Send(new GetRolesQuery());

        return TypedResults.Ok(vm);
    }

    [EndpointSummary("Create a new Role")]
    [EndpointDescription("Creates a new role with the given permissions and returns its ID.")]
    public static async Task<Created<string>> CreateRole(ISender sender, CreateRoleCommand command)
    {
        var id = await sender.Send(command);

        return TypedResults.Created($"/{nameof(Roles)}/{id}", id);
    }

    [EndpointSummary("Update a Role")]
    [EndpointDescription("Updates the name and permissions of the specified role. System roles cannot be renamed and the Administrator role cannot be modified.")]
    public static async Task<Results<NoContent, BadRequest>> UpdateRole(ISender sender, string id, UpdateRoleCommand command)
    {
        if (id != command.Id) return TypedResults.BadRequest();

        await sender.Send(command);

        return TypedResults.NoContent();
    }

    [EndpointSummary("Delete a Role")]
    [EndpointDescription("Deletes the role with the specified ID. System roles and roles assigned to users cannot be deleted.")]
    public static async Task<NoContent> DeleteRole(ISender sender, string id)
    {
        await sender.Send(new DeleteRoleCommand(id));

        return TypedResults.NoContent();
    }
}
