using StackExchange.Redis;

namespace Singleton.Services.Redis.Interfaces;

public interface IRedisService
{
    // String Operations (JSON 직렬화 지원)
    Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, When? when = null);
    Task<T?> GetAsync<T>(string key);
    Task RemoveAsync(string key);

    // ✅ Hash(Map) Operations (필드 단위 접근)
    Task HashSetAsync(string key, string field, string value);
    Task<string?> HashGetAsync(string key, string field);
    Task HashDeleteAsync(string key, string field);
    Task<Dictionary<string, string>> HashGetAllAsync(string key);
}
