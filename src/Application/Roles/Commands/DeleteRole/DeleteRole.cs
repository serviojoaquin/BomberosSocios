using BaseApp.Application.Common.Interfaces;
using BaseApp.Application.Common.Models;
using BaseApp.Application.Common.Security;
using BaseApp.Domain.Enums;

namespace BaseApp.Application.Roles.Commands.DeleteRole;

[Authorize(Permissions = Permissions.ManageSecurity)]
public record DeleteRoleCommand(string Id) : IRequest;

public class DeleteRoleCommandHandler : IRequestHandler<DeleteRoleCommand>
{
    private readonly IIdentityService _identityService;

    public DeleteRoleCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task Handle(DeleteRoleCommand request, CancellationToken cancellationToken)
    {
        var result = await _identityService.DeleteRoleAsync(request.Id);

        result.ThrowIfFailed();
    }
}
