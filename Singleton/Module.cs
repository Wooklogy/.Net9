using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Singleton.Services.Jwt;
using Singleton.Services.Jwt.Interfaces;
using Singleton.Services.Redis;
using Singleton.Services.Redis.Interfaces;
using StackExchange.Redis;

namespace Singleton;

public static class SingletonRegistration
{
    public static IServiceCollection AddSingletonInfra(this IServiceCollection services, IConfiguration config)
    {
        // 1. Redis Connection Setup
        var redisConnString = config["REDIS:Configuration"] ?? "localhost:6379";
        services.AddSingleton<IConnectionMultiplexer>(sp => ConnectionMultiplexer.Connect(redisConnString));
        services.AddSingleton<IRedisService, RedisService>();

        // 2. Redis Distributed Cache (IDistributedCache 사용용 - API 캐시 로직 호환)
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnString;
            options.InstanceName = config["REDIS:NAME"] ?? "Undefined";
        });

        // 3. JWT Service
        services.AddSingleton<IJwtTokenService, JwtTokenService>();

        return services;
    }
}