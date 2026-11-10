using BaseApp.Application.Common.Interfaces;
using BaseApp.Application.Common.Models;
using BaseApp.Domain.Constants;
using BaseApp.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BaseApp.Infrastructure.Identity;

public class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly IUserClaimsPrincipalFactory<ApplicationUser> _userClaimsPrincipalFactory;
    private readonly IAuthorizationService _authorizationService;

    public IdentityService(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        IUserClaimsPrincipalFactory<ApplicationUser> userClaimsPrincipalFactory,
        IAuthorizationService authorizationService)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _userClaimsPrincipalFactory = userClaimsPrincipalFactory;
        _authorizationService = authorizationService;
    }

    public async Task<string?> GetUserNameAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        return user?.UserName;
    }

    public async Task<string?> GetUserIdByEmailAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);

        return user?.Id;
    }

    public async Task<(Result Result, string UserId)> CreateUserAsync(string userName, string password)
    {
        var user = new ApplicationUser
        {
            UserName = userName,
            Email = userName,
        };

        var result = await _userManager.CreateAsync(user, password);

        return (result.ToApplicationResult(), user.Id);
    }

    public async Task<bool> IsInRoleAsync(string userId, string role)
    {
        var user = await _userManager.FindByIdAsync(userId);

        return user != null && await _userManager.IsInRoleAsync(user, role);
    }

    public async Task<bool> AuthorizeAsync(string userId, string policyName)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
        {
            return false;
        }

        var principal = await _userClaimsPrincipalFactory.CreateAsync(user);

        var result = await _authorizationService.AuthorizeAsync(principal, policyName);

        return result.Succeeded;
    }

    public async Task<Result> DeleteUserAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        return user != null ? await DeleteUserAsync(user) : Result.Success();
    }

    public async Task<Result> DeleteUserAsync(ApplicationUser user)
    {
        var result = await _userManager.DeleteAsync(user);

        return result.ToApplicationResult();
    }

    public async Task<IReadOnlyCollection<RoleDto>> GetRolesAsync()
    {
        return await _roleManager.Roles
            .AsNoTracking()
            .OrderBy(r => r.Name)
            .Select(r => new RoleDto
            {
                Id = r.Id,
                Name = r.Name!,
                Permissions = (int)r.Permissions,
                IsSystem = r.IsSystem
            })
            .ToListAsync();
    }

    public async Task<(Result Result, string RoleId)> CreateRoleAsync(string name, Permissions permissions)
    {
        var role = new ApplicationRole(name, permissions);

        var result = await _roleManager.CreateAsync(role);

        return (result.ToApplicationResult(), role.Id);
    }

    public async Task<Result> UpdateRoleAsync(string roleId, string name, Permissions permissions)
    {
        var role = await _roleManager.FindByIdAsync(roleId);

        if (role is null)
        {
            return Result.Failure([$"Role '{roleId}' was not found."]);
        }

        if (role.Name == Roles.Administrator)
        {
            return Result.Failure([$"The '{Roles.Administrator}' role cannot be modified."]);
        }

        if (role.IsSystem && role.Name != name)
        {
            return Result.Failure(["System roles cannot be renamed."]);
        }

        role.Name = name;
        role.Permissions = permissions;

        var result = await _roleManager.UpdateAsync(role);

        return result.ToApplicationResult();
    }

    public async Task<Result> DeleteRoleAsync(string roleId)
    {
        var role = await _roleManager.FindByIdAsync(roleId);

        if (role is null)
        {
            return Result.Success();
        }

        if (role.IsSystem)
        {
            return Result.Failure(["System roles cannot be deleted."]);
        }

        var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name!);

        if (usersInRole.Count > 0)
        {
            return Result.Failure([$"Role '{role.Name}' is assigned to {usersInRole.Count} user(s) and cannot be deleted."]);
        }

        var result = await _roleManager.DeleteAsync(role);

        return result.ToApplicationResult();
    }

    public async Task<IReadOnlyCollection<UserDto>> GetUsersAsync()
    {
        var users = await _userManager.Users
            .AsNoTracking()
            .OrderBy(u => u.UserName)
            .ToListAsync();

        var result = new List<UserDto>(users.Count);

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);

            result.Add(new UserDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                IsEnabled = !(user.LockoutEnabled && user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTimeOffset.UtcNow),
                Roles = roles.OrderBy(r => r).ToList()
            });
        }

        return result;
    }

    public async Task<Result> SetUserPasswordAsync(string userId, string newPassword)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user is null)
        {
            return Result.Failure([$"User '{userId}' was not found."]);
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);

        var result = await _userManager.ResetPasswordAsync(user, token, newPassword);

        return result.ToApplicationResult();
    }

    public async Task<Result> SetUserEnabledAsync(string userId, bool enabled)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user is null)
        {
            return Result.Failure([$"User '{userId}' was not found."]);
        }

        await _userManager.SetLockoutEnabledAsync(user, true);

        var result = await _userManager.SetLockoutEndDateAsync(user, enabled ? null : DateTimeOffset.MaxValue);

        if (result.Succeeded && !enabled)
        {
            await _userManager.UpdateSecurityStampAsync(user);
        }

        return result.ToApplicationResult();
    }

    public async Task<Result> UpdateUserRolesAsync(string userId, IReadOnlyCollection<string> roles)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user is null)
        {
            return Result.Failure([$"User '{userId}' was not found."]);
        }

        var existingRoles = await _roleManager.Roles
            .Select(r => r.Name!)
            .ToListAsync();

        var unknown = roles.Except(existingRoles).ToList();

        if (unknown.Count > 0)
        {
            return Result.Failure([$"Unknown role(s): {string.Join(", ", unknown)}."]);
        }

        var currentRoles = await _userManager.GetRolesAsync(user);

        var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles.Except(roles));

        if (!removeResult.Succeeded)
        {
            return removeResult.ToApplicationResult();
        }

        var addResult = await _userManager.AddToRolesAsync(user, roles.Except(currentRoles));

        return addResult.ToApplicationResult();
    }
}
