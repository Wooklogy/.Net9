using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Share.Enums;
using Share.Constants;
using Api.Infra;
using Singleton.Services.Redis.Interfaces;

namespace Api.Config.Authorization;

public sealed class AuthHandler(IDbContextFactory<AppDbContext> dbFactory, IRedisService redis) : AuthorizationHandler<AuthRequirement>
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory = dbFactory;
    private readonly IRedisService _redis = redis;

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, AuthRequirement requirement)
    {
        if (context.User.Identity?.IsAuthenticated != true) return;

        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId)) return;

        var cacheKey = UserCacheKey.UserCache(userId);

        // 1. Redis에서 Role과 Permissions 한 번에 가져오기 (HGETALL)
        var userCache = await _redis.HashGetAllAsync(cacheKey);

        EnumRole? currentRole = null;
        List<EnumPermission> currentPermissions = [];

        if (userCache.Count > 0)
        {
            // 캐시 적중 (Cache Hit)
            currentRole = Enum.Parse<EnumRole>(userCache[UserCacheKey.Role]);
            currentPermissions = userCache[UserCacheKey.Permissions]
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(p => Enum.Parse<EnumPermission>(p))
                .ToList();
        }
        else
        {
            // 캐시 미스 (Cache Miss) -> DB 조회 후 Redis 적재
            using var db = await _dbFactory.CreateDbContextAsync();
            var user = await db.Users.AsNoTracking()
                .Where(u => u.Id == userId && u.DeletedAt == null)
                .Select(u => new { u.Role }).FirstOrDefaultAsync();

            if (user == null) return;
            currentRole = user.Role;

            currentPermissions = await db.Permissions.AsNoTracking()
                .Where(p => p.TargetIsId == userId && p.DeletedAt == null)
                .Select(p => p.Permission).ToListAsync();

            // Redis에 Hash로 적재 (HSET)
            await _redis.HashSetAsync(cacheKey, UserCacheKey.Role, currentRole.Value.ToString());
            await _redis.HashSetAsync(cacheKey, UserCacheKey.Permissions, string.Join(',', currentPermissions));
        }

        // 2. Role 체크
        if (requirement.Roles.Any() && !requirement.Roles.Contains(currentRole.Value)) return;

        // 3. Permission 체크
        if (requirement.Permissions.Any() && !requirement.Permissions.All(p => currentPermissions.Contains(p))) return;

        var appIdentity = new ClaimsIdentity();
        appIdentity.AddClaim(new Claim(ClaimTypes.Role, currentRole.ToString()!));
        appIdentity.AddClaim(new Claim(ClaimTypes.NameIdentifier, userId.ToString()!));


        foreach (var permission in currentPermissions)
        {
            appIdentity.AddClaim(new Claim("Permission", permission.ToString()));
        }

        // 현재 유저의 Principal에 새로운 Identity 추가
        context.User.AddIdentity(appIdentity);

        context.Succeed(requirement);
    }
}