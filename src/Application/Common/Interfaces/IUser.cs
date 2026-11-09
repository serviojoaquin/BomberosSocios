using BaseApp.Domain.Enums;

namespace BaseApp.Application.Common.Interfaces;

public interface IUser
{
    string? Id { get; }
    List<string>? Roles { get; }
    Permissions Permissions { get; }
}
