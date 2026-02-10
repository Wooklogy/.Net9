// Api.App.Common.Interfaces (예시 경로)
using System.Security.Claims;
using Share.Enums;

namespace Api.Config.Context;

public interface IUserContext
{
    Guid UserId { get; }
    EnumRole Role { get; }
    List<EnumPermission> Permissions { get; }
    bool HasPermission(EnumPermission permission);
}

public class UserContext(IHttpContextAccessor accessor) : IUserContext
{
    private ClaimsPrincipal? User => accessor.HttpContext?.User;

    public Guid UserId => User?.FindFirstValue(ClaimTypes.NameIdentifier) is string id ? Guid.Parse(id) : Guid.Empty;

    public EnumRole Role => User?.FindFirstValue(ClaimTypes.Role) is string role ? Enum.Parse<EnumRole>(role) : EnumRole.User;

    public List<EnumPermission> Permissions => User?.FindAll("Permission")
        .Select(c => Enum.Parse<EnumPermission>(c.Value))
        .ToList() ?? [];

    public bool HasPermission(EnumPermission permission) => Permissions.Contains(permission);
}