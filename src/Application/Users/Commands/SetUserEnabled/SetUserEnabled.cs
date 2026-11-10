using BaseApp.Application.Common.Interfaces;
using BaseApp.Application.Common.Models;
using BaseApp.Application.Common.Security;
using BaseApp.Domain.Enums;

namespace BaseApp.Application.Users.Commands.SetUserEnabled;

[Authorize(Permissions = Permissions.ManageSecurity)]
public record SetUserEnabledCommand : IRequest
{
    public string Id { get; init; } = string.Empty;

    public bool Enabled { get; init; }
}

public class SetUserEnabledCommandValidator : AbstractValidator<SetUserEnabledCommand>
{
    public SetUserEnabledCommandValidator()
    {
        RuleFor(v => v.Id)
            .NotEmpty();
    }
}

public class SetUserEnabledCommandHandler : IRequestHandler<SetUserEnabledCommand>
{
    private readonly IIdentityService _identityService;
    private readonly IUser _user;

    public SetUserEnabledCommandHandler(IIdentityService identityService, IUser user)
    {
        _identityService = identityService;
        _user = user;
    }

    public async Task Handle(SetUserEnabledCommand request, CancellationToken cancellationToken)
    {
        if (!request.Enabled && request.Id == _user.Id)
        {
            Result.Failure(["You cannot disable your own account."]).ThrowIfFailed();
        }

        var result = await _identityService.SetUserEnabledAsync(request.Id, request.Enabled);

        result.ThrowIfFailed();
    }
}
