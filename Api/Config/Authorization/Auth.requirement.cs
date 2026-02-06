using Microsoft.AspNetCore.Authorization;
using Share.Enums; // EnumRole, EnumPermission 위치

namespace Api.Config.Authorization;

/// <summary>
/// API 접근을 위해 필요한 Role과 Permission의 조합을 정의합니다.
/// </summary>
public sealed class AuthRequirement(
    IEnumerable<EnumRole> roles,
    IEnumerable<EnumPermission> permissions
    ) : IAuthorizationRequirement
{
    // 읽기 전용으로 설정하여 Handler에서 안전하게 참조 가능
    public IReadOnlySet<EnumRole> Roles { get; } = roles.ToHashSet();
    public IReadOnlySet<EnumPermission> Permissions { get; } = permissions.ToHashSet();
}