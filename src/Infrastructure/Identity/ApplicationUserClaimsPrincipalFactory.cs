using System.Security.Claims;
using BaseApp.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace BaseApp.Infrastructure.Identity;

public class ApplicationUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<ApplicationUser, ApplicationRole>
{
    public ApplicationUserClaimsPrincipalFactory(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        IOptions<IdentityOptions> options)
        : base(userManager, roleManager, options)
    {
    }

    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
    {
        var identity = await base.GenerateClaimsAsync(user);

        var roleNames = await UserManager.GetRolesAsync(user);

        var permissions = await RoleManager.Roles
            .Where(r => roleNames.Contains(r.Name!))
            .Select(r => r.Permissions)
            .ToListAsync();

        var combined = permissions.Aggregate(Permissions.None, (acc, p) => acc | p);

        identity.AddClaim(new Claim(CustomClaimTypes.Permissions, ((int)combined).ToString()));

        return identity;
    }
}
