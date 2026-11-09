namespace BaseApp.Application.Common.Models;

public class UserDto
{
    public string Id { get; init; } = string.Empty;

    public string? UserName { get; init; }

    public string? Email { get; init; }

    public IReadOnlyCollection<string> Roles { get; init; } = [];
}
