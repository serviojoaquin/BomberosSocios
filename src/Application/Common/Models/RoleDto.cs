namespace BaseApp.Application.Common.Models;

public class RoleDto
{
    public string Id { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public int Permissions { get; init; }

    public bool IsSystem { get; init; }
}
