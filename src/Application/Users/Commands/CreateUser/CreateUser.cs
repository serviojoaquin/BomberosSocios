using BaseApp.Application.Common.Interfaces;
using BaseApp.Application.Common.Models;
using BaseApp.Application.Common.Security;
using BaseApp.Domain.Enums;

namespace BaseApp.Application.Users.Commands.CreateUser;

[Authorize(Permissions = Permissions.ManageSecurity)]
public record CreateUserCommand : IRequest<string>
{
    public string? Email { get; init; }

    public string? Password { get; init; }

    public IReadOnlyCollection<string> Roles { get; init; } = [];
}

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(v => v.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(256);

        RuleFor(v => v.Password)
            .NotEmpty()
            .MinimumLength(6);
    }
}

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, string>
{
    private readonly IIdentityService _identityService;

    public CreateUserCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<string> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var (result, userId) = await _identityService.CreateUserAsync(request.Email!, request.Password!);

        result.ThrowIfFailed();

        if (request.Roles.Count > 0)
        {
            var rolesResult = await _identityService.UpdateUserRolesAsync(userId, request.Roles);

            rolesResult.ThrowIfFailed();
        }

        return userId;
    }
}
