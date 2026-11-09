using BaseApp.Application.Common.Models;
using BaseApp.Domain.Enums;

namespace BaseApp.Application.Common.Interfaces;

public interface IIdentityService
{
    Task<string?> GetUserNameAsync(string userId);

    Task<bool> IsInRoleAsync(string userId, string role);

    Task<bool> AuthorizeAsync(string userId, string policyName);

    Task<(Result Result, string UserId)> CreateUserAsync(string userName, string password);

    Task<Result> DeleteUserAsync(string userId);

    Task<IReadOnlyCollection<RoleDto>> GetRolesAsync();

    Task<(Result Result, string RoleId)> CreateRoleAsync(string name, Permissions permissions);

    Task<Result> UpdateRoleAsync(string roleId, string name, Permissions permissions);

    Task<Result> DeleteRoleAsync(string roleId);

    Task<IReadOnlyCollection<UserDto>> GetUsersAsync();

    Task<Result> UpdateUserRolesAsync(string userId, IReadOnlyCollection<string> roles);
}
