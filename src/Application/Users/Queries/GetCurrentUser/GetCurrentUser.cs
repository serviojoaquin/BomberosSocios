using BaseApp.Application.Common.Interfaces;
using BaseApp.Application.Common.Security;

namespace BaseApp.Application.Users.Queries.GetCurrentUser;

public class CurrentUserVm
{
    public string Id { get; init; } = string.Empty;

    public string? UserName { get; init; }

    public IReadOnlyCollection<string> Roles { get; init; } = [];

    public int Permissions { get; init; }
}

[Authorize]
public record GetCurrentUserQuery : IRequest<CurrentUserVm>;

public class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, CurrentUserVm>
{
    private readonly IUser _user;
    private readonly IIdentityService _identityService;

    public GetCurrentUserQueryHandler(IUser user, IIdentityService identityService)
    {
        _user = user;
        _identityService = identityService;
    }

    public async Task<CurrentUserVm> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        var userId = _user.Id!;

        return new CurrentUserVm
        {
            Id = userId,
            UserName = await _identityService.GetUserNameAsync(userId),
            Roles = _user.Roles ?? [],
            Permissions = (int)_user.Permissions
        };
    }
}
