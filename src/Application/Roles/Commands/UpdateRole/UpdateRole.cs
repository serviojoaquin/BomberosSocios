using BaseApp.Application.Common.Interfaces;
using BaseApp.Application.Common.Models;
using BaseApp.Application.Common.Security;
using BaseApp.Domain.Enums;

namespace BaseApp.Application.Roles.Commands.UpdateRole;

[Authorize(Permissions = Domain.Enums.Permissions.ManageSecurity)]
public record UpdateRoleCommand : IRequest
{
    public string Id { get; init; } = string.Empty;

    public string? Name { get; init; }

    public int Permissions { get; init; }
}

public class UpdateRoleCommandHandler : IRequestHandler<UpdateRoleCommand>
{
    private readonly IIdentityService _identityService;

    public UpdateRoleCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task Handle(UpdateRoleCommand request, CancellationToken cancellationToken)
    {
        var result = await _identityService.UpdateRoleAsync(request.Id, request.Name!, (Permissions)request.Permissions);

        result.ThrowIfFailed();
    }
}
