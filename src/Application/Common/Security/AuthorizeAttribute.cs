using BaseApp.Domain.Enums;

namespace BaseApp.Application.Common.Security;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public class AuthorizeAttribute : Attribute
{
    public AuthorizeAttribute() { }

    public string Roles { get; set; } = string.Empty;

    public string Policy { get; set; } = string.Empty;

    public Permissions Permissions { get; set; } = Permissions.None;
}
