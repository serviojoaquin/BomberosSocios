using BaseApp.Application.Common.Interfaces;
using BaseApp.Application.Common.Models;
using BaseApp.Application.Common.Security;
using BaseApp.Domain.Enums;

namespace BaseApp.Application.Users.Commands.ResetUserPassword;

[Authorize(Permissions = Permissions.ManageSecurity)]
public record ResetUserPasswordCommand : IRequest
{
    public string Id { get; init; } = string.Empty;

    public string? NewPassword { get; init; }
}

public class ResetUserPasswordCommandValidator : AbstractValidator<ResetUserPasswordCommand>
{
    public ResetUserPasswordCommandValidator()
    {
        RuleFor(v => v.Id)
            .NotEmpty();

        RuleFor(v => v.NewPassword)
            .NotEmpty()
            .MinimumLength(6);
    }
}

public class ResetUserPasswordCommandHandler : IRequestHandler<ResetUserPasswordCommand>
{
    private readonly IIdentityService _identityService;

    public ResetUserPasswordCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task Handle(ResetUserPasswordCommand request, CancellationToken cancellationToken)
    {
        var result = await _identityService.SetUserPasswordAsync(request.Id, request.NewPassword!);

        result.ThrowIfFailed();
    }
}
