using Microsoft.AspNetCore.SignalR;
using StackExchange.Redis;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Singleton;
using Hub.App;
using Hub.Config;

var builder = WebApplication.CreateBuilder(args);

// --- 0. Logging Configuration (노이즈 제거) ---
builder.Logging.AddFilter("Microsoft.AspNetCore", LogLevel.Warning);
builder.Logging.AddFilter("Microsoft.AspNetCore.Routing", LogLevel.Warning);
builder.Logging.AddFilter("Microsoft.AspNetCore.Cors", LogLevel.Warning);
builder.Logging.AddFilter("Microsoft.AspNetCore.Hosting.Diagnostics", LogLevel.Warning);

// --- 1. Singleton Infrastructure 장착 ---
// Redis, JWT(IJwtTokenService), DistributedCache 등을 한 번에 세팅
builder.Services.AddSingletonInfra(builder.Configuration);

// --- 2. Hub Specific Infrastructure ---
builder.Services.AddHttpContextAccessor();

// CORS 설정 (API와 정렬)
var rawOrigins = builder.Configuration["ALLOWED_ORIGINS"] ?? "";
var allowedOrigins = rawOrigins.Split(',', StringSplitOptions.RemoveEmptyEntries)
                               .Select(o => o.Trim())
                               .ToArray();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
        }
        else
        {
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials(); // SignalR 필수 설정
        }
    });
});

// --- 3. JWT 상세 설정 (SignalR 쿼리 스트링 대응) ---
// Singleton에서 기본 인증은 잡았으나, WebSocket 특유의 'access_token' 파싱은 Hub에서 추가 설정
builder.Services.Configure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
{
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

// --- 4. SignalR & Redis Backplane ---
var redisSection = builder.Configuration.GetSection("REDIS");
var redisSignal = redisSection["REDIS_SIGNAL_NAME"] ?? "Undefined";

builder.Services.AddSignalR()
    .AddStackExchangeRedis(options =>
    {
        options.Configuration = ConfigurationOptions.Parse(redisSection["Configuration"] ?? "localhost:6379");
        options.Configuration.ChannelPrefix = new RedisChannel(redisSignal, RedisChannel.PatternMode.Literal);
    });

// 유저 식별자 프로바이더 등록 (Singleton 프로젝트에 위치한 공용 부품)
builder.Services.AddSingleton<IUserIdProvider, UuidUserIdProvider>();

var app = builder.Build();

// --- 5. Middleware Pipeline ---
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.UseCors();

// 개발 환경에서 dotnet watch 스크립트 주입 충돌 방지
if (!app.Environment.IsDevelopment())
{
    // Hub는 보통 응답 압축의 실익이 적으나, 필요 시 운영에서만 켭니다.
    // app.UseResponseCompression(); 
}

app.UseAuthentication();
app.UseAuthorization();
app.MapSystemHubs();
app.Run();