using BaseApp.Application.Common.Interfaces;
using BaseApp.Application.Common.Models;
using BaseApp.Application.Common.Security;
using BaseApp.Domain.Enums;

namespace BaseApp.Application.Roles.Commands.CreateRole;

[Authorize(Permissions = Domain.Enums.Permissions.ManageSecurity)]
public record CreateRoleCommand : IRequest<string>
{
    public string? Name { get; init; }

    public int Permissions { get; init; }
}

public class CreateRoleCommandHandler : IRequestHandler<CreateRoleCommand, string>
{
    private readonly IIdentityService _identityService;

    public CreateRoleCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<string> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        var (result, roleId) = await _identityService.CreateRoleAsync(request.Name!, (Permissions)request.Permissions);

        result.ThrowIfFailed();

        return roleId;
    }
}
