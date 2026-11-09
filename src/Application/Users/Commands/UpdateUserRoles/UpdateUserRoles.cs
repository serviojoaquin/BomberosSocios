using BaseApp.Application.Common.Interfaces;
using BaseApp.Application.Common.Models;
using BaseApp.Application.Common.Security;
using BaseApp.Domain.Enums;

namespace BaseApp.Application.Users.Commands.UpdateUserRoles;

[Authorize(Permissions = Permissions.ManageSecurity)]
public record UpdateUserRolesCommand : IRequest
{
    public string Id { get; init; } = string.Empty;

    public IReadOnlyCollection<string> Roles { get; init; } = [];
}

public class UpdateUserRolesCommandValidator : AbstractValidator<UpdateUserRolesCommand>
{
    public UpdateUserRolesCommandValidator()
    {
        RuleFor(v => v.Id)
            .NotEmpty();
    }
}

public class UpdateUserRolesCommandHandler : IRequestHandler<UpdateUserRolesCommand>
{
    private readonly IIdentityService _identityService;

    public UpdateUserRolesCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task Handle(UpdateUserRolesCommand request, CancellationToken cancellationToken)
    {
        var result = await _identityService.UpdateUserRolesAsync(request.Id, request.Roles);

        result.ThrowIfFailed();
    }
}
