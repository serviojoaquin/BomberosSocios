using System.ComponentModel;
using System.Reflection;
using BaseApp.Application.Common.Interfaces;
using BaseApp.Application.Common.Models;
using BaseApp.Application.Common.Security;
using BaseApp.Domain.Enums;

namespace BaseApp.Application.Roles.Queries.GetRoles;

public class PermissionDto
{
    public int Id { get; init; }

    public string Name { get; init; } = string.Empty;
}

public class RolesVm
{
    public IReadOnlyCollection<RoleDto> Roles { get; init; } = [];

    public IReadOnlyCollection<PermissionDto> AvailablePermissions { get; init; } = [];
}

[Authorize(Permissions = Permissions.ManageSecurity)]
public record GetRolesQuery : IRequest<RolesVm>;

public class GetRolesQueryHandler : IRequestHandler<GetRolesQuery, RolesVm>
{
    private readonly IIdentityService _identityService;

    public GetRolesQueryHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<RolesVm> Handle(GetRolesQuery request, CancellationToken cancellationToken)
    {
        return new RolesVm
        {
            Roles = await _identityService.GetRolesAsync(),
            AvailablePermissions = Enum.GetValues<Permissions>()
                .Where(p => p != Permissions.None && p != Permissions.All)
                .Select(p => new PermissionDto { Id = (int)p, Name = GetDescription(p) })
                .ToList()
        };
    }

    private static string GetDescription(Permissions permission)
    {
        var name = permission.ToString();

        return typeof(Permissions).GetField(name)?
            .GetCustomAttribute<DescriptionAttribute>()?.Description ?? name;
    }
}
