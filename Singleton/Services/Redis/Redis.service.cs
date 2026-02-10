using Singleton.Services.Redis.Interfaces;
using StackExchange.Redis;
using System.Text.Json;

namespace Singleton.Services.Redis;


public sealed class RedisService(IConnectionMultiplexer redis) : IRedisService
{
    private readonly IDatabase _db = redis.GetDatabase();
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    // --- String Operations ---
    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, When? when = null)
    {
        var json = JsonSerializer.Serialize(value, _jsonOptions);

        // 1. 명시적 형변환 (TimeSpan? -> Expiration)
        // 2. Named Arguments (expiry:, when:) 사용으로 모호성 완전 제거
        await _db.StringSetAsync(
            key,
            json,
            expiry: expiry,
            when: when ?? When.Always
        );
    }
    public async Task<T?> GetAsync<T>(string key)
    {
        var value = await _db.StringGetAsync(key);
        if (value.IsNullOrEmpty) return default;
        return JsonSerializer.Deserialize<T>(value!, _jsonOptions);
    }

    public async Task RemoveAsync(string key)
    {
        await _db.KeyDeleteAsync(key);
    }

    // --- Hash(Map) Operations ---
    public async Task HashSetAsync(string key, string field, string value)
    {
        await _db.HashSetAsync(key, field, value);
    }

    public async Task<string?> HashGetAsync(string key, string field)
    {
        var value = await _db.HashGetAsync(key, field);
        return value.HasValue ? value.ToString() : null;
    }

    public async Task HashDeleteAsync(string key, string field)
    {
        await _db.HashDeleteAsync(key, field);
    }

    public async Task<Dictionary<string, string>> HashGetAllAsync(string key)
    {
        var entries = await _db.HashGetAllAsync(key);
        var result = new Dictionary<string, string>(entries.Length);
        foreach (var entry in entries)
        {
            result.Add(entry.Name.ToString(), entry.Value.ToString());
        }
        return result;
    }
}