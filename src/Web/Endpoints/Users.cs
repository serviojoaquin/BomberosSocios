using BaseApp.Application.Users.Commands.UpdateUserRoles;
using BaseApp.Application.Users.Queries.GetCurrentUser;
using BaseApp.Application.Users.Queries.GetUsers;
using BaseApp.Domain.Enums;
using BaseApp.Infrastructure.Identity;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BaseApp.Web.Endpoints;

public class Users : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapIdentityApi<ApplicationUser>();

        groupBuilder.MapPost(Logout, "logout").RequireAuthorization();
        groupBuilder.MapGet(GetCurrentUser, "me").RequireAuthorization();
        groupBuilder.MapGet(GetUsers).RequirePermission(Permissions.ManageSecurity);
        groupBuilder.MapPut(UpdateUserRoles, "{id}/roles").RequirePermission(Permissions.ManageSecurity);
    }

    [EndpointSummary("Get current user")]
    [EndpointDescription("Returns the authenticated user with roles and permissions.")]
    public static async Task<Ok<CurrentUserVm>> GetCurrentUser(ISender sender)
    {
        var vm = await sender.Send(new GetCurrentUserQuery());

        return TypedResults.Ok(vm);
    }

    [EndpointSummary("Get all Users")]
    [EndpointDescription("Retrieves all users with their roles and the list of available roles.")]
    public static async Task<Ok<UsersVm>> GetUsers(ISender sender)
    {
        var vm = await sender.Send(new GetUsersQuery());

        return TypedResults.Ok(vm);
    }

    [EndpointSummary("Update the Roles of a User")]
    [EndpointDescription("Replaces the roles assigned to the specified user. Changes apply on the next login.")]
    public static async Task<Results<NoContent, BadRequest>> UpdateUserRoles(ISender sender, string id, UpdateUserRolesCommand command)
    {
        if (id != command.Id) return TypedResults.BadRequest();

        await sender.Send(command);

        return TypedResults.NoContent();
    }

    [EndpointSummary("Log out")]
    [EndpointDescription("Logs out the current user by clearing the authentication cookie.")]
    public static async Task<Results<Ok, UnauthorizedHttpResult>> Logout(SignInManager<ApplicationUser> signInManager, [FromBody] object empty)
    {
        if (empty != null)
        {
            await signInManager.SignOutAsync();
            return TypedResults.Ok();
        }

        return TypedResults.Unauthorized();
    }
}
