namespace Share.Constants;

public static class UserCacheKey
{
    // 유저 캐시 키: user:cache:{userId}
    public static string UserCache(Guid userId) => $"user:cache:{userId}";

    // Hash Fields
    public const string Role = "role";
    public const string Permissions = "permissions";
}