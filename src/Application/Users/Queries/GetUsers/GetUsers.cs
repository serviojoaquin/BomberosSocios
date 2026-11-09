using BaseApp.Application.Common.Interfaces;
using BaseApp.Application.Common.Models;
using BaseApp.Application.Common.Security;
using BaseApp.Domain.Enums;

namespace BaseApp.Application.Users.Queries.GetUsers;

public class UsersVm
{
    public IReadOnlyCollection<UserDto> Users { get; init; } = [];

    public IReadOnlyCollection<string> AvailableRoles { get; init; } = [];
}

[Authorize(Permissions = Permissions.ManageSecurity)]
public record GetUsersQuery : IRequest<UsersVm>;

public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, UsersVm>
{
    private readonly IIdentityService _identityService;

    public GetUsersQueryHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<UsersVm> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var roles = await _identityService.GetRolesAsync();

        return new UsersVm
        {
            Users = await _identityService.GetUsersAsync(),
            AvailableRoles = roles.Select(r => r.Name).ToList()
        };
    }
}
