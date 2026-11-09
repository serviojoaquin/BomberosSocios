using BaseApp.Domain.Enums;
using BaseApp.Infrastructure.Identity;

namespace BaseApp.Web.Infrastructure;

public static class PermissionEndpointExtensions
{
    public static TBuilder RequirePermission<TBuilder>(this TBuilder builder, Permissions permission)
        where TBuilder : IEndpointConventionBuilder
    {
        return builder.RequireAuthorization(policy => policy.RequireAssertion(context =>
        {
            var value = context.User.FindFirst(CustomClaimTypes.Permissions)?.Value;

            return int.TryParse(value, out var granted) && ((Permissions)granted & permission) == permission;
        }));
    }
}
