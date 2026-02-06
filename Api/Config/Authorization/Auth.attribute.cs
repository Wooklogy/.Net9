using Microsoft.AspNetCore.Authorization;
using Share.Enums; // EnumRole, EnumPermission 위치

namespace Api.Config.Authorization;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class AuthorizePermissionAttribute : AuthorizeAttribute
{
    public EnumRole[] Role { get; }
    public EnumPermission[] Permissions { get; }

    // Role-only
    public AuthorizePermissionAttribute(params EnumRole[] roles)
    {
        Role = roles;
        Permissions = [];
        Policy = BuildPolicy();
    }

    // Role + Permission 조합
    public AuthorizePermissionAttribute(EnumRole[] roles, EnumPermission[] permissions)
    {
        Role = roles;
        Permissions = permissions;
        Policy = BuildPolicy();
    }

    private string BuildPolicy()
    {
        // 2만 명 트래픽 대응: 문자열 결합 최적화
        return $"PERM::{string.Join(',', Role)}|{string.Join(',', Permissions)}";
    }
}