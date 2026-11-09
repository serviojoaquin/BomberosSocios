using BaseApp.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace BaseApp.Infrastructure.Identity;

public class ApplicationRole : IdentityRole
{
    public ApplicationRole() { }

    public ApplicationRole(string roleName, Permissions permissions, bool isSystem = false) : base(roleName)
    {
        Permissions = permissions;
        IsSystem = isSystem;
    }

    public Permissions Permissions { get; set; }

    public bool IsSystem { get; set; }
}
