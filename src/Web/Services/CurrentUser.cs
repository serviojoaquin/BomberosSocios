using System.Security.Claims;
using BaseApp.Application.Common.Interfaces;
using BaseApp.Domain.Enums;
using BaseApp.Infrastructure.Identity;

namespace BaseApp.Web.Services;

public class CurrentUser : IUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? Id => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

    public List<string>? Roles => _httpContextAccessor.HttpContext?.User?.FindAll(ClaimTypes.Role).Select(x => x.Value).ToList();

    public Permissions Permissions
    {
        get
        {
            var value = _httpContextAccessor.HttpContext?.User?.FindFirstValue(CustomClaimTypes.Permissions);

            return int.TryParse(value, out var permissions) ? (Permissions)permissions : Permissions.None;
        }
    }
}
