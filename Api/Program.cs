using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Amazon.S3;
using Scalar.AspNetCore;
using Microsoft.AspNetCore.HttpOverrides;
using StackExchange.Redis;
using Api.Config.Error;
using Api.Config.Monitoring;
using OpenTelemetry.Metrics;
using Api.Config.Validation;
using Microsoft.AspNetCore.Authorization;
using Api.Infra;
using Api.Infra.Base;
using Microsoft.OpenApi.Models;
using Api.Config.Authorization;
using Singleton;
using Share.Models;
using Api.Config.Context;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    ContentRootPath = Directory.GetCurrentDirectory()
});

// --- 0. Configuration ---
builder.Configuration.Sources.Clear();
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

// --- 1. Singleton Infrastructure (공통 인프라 주입) ---
// 이제 Redis, JWT, RedisCache 등록 로직은 Singleton 내부에서 처리됩니다.
builder.Services.AddSingletonInfra(builder.Configuration);

// --- 2. API Specific Infrastructure ---
builder.Services.AddHealthChecks();
builder.Services.AddResponseCompression(options => { options.EnableForHttps = true; });
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUserContext, UserContext>();

// CORS 설정
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
            policy.WithOrigins(allowedOrigins).AllowAnyMethod().AllowAnyHeader().AllowCredentials();
        }
    });
});

// --- 3. Controller & JSON 세팅 ---
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ValidationExceptionFilter>();
    options.Conventions.Add(new ApiPrefixConvention("api"));
})
.AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
    // internal set 직렬화 수정 델리게이트 유지
    options.JsonSerializerOptions.TypeInfoResolver = new DefaultJsonTypeInfoResolver
    {
        Modifiers = { info => {
            if (info.Kind != JsonTypeInfoKind.Object) return;
            foreach (var property in info.Properties)
            {
                if (property.Set == null)
                {
                    var propInfo = info.Type.GetProperty(property.Name);
                    if (propInfo != null && propInfo.GetSetMethod(true) != null) property.Set = propInfo.SetValue;
                }
            }
        }}
    };
});

builder.Services.Configure<ApiBehaviorOptions>(options => options.SuppressModelStateInvalidFilter = true);

// --- 4. 권한 처리 (Custom Policies) ---
builder.Services.AddAuthorization();
builder.Services.AddSingleton<IAuthorizationPolicyProvider, AuthPolicyProvider>();
builder.Services.AddScoped<IAuthorizationHandler, AuthHandler>();

// --- 5. OpenAPI & Scalar ---
var targetPort = builder.Configuration["PORT"] ?? "5000";
builder.Services.AddOpenApi(options =>
{
    options.AddOperationTransformer<ErrorResponseTransformer>();
    options.AddOperationTransformer<AuthDescriptionTransformer>();
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info.Title = "System API";
        document.Info.Version = "v1";
        document.Components ??= new OpenApiComponents();
        document.Servers = [new() { Url = $"http://localhost:{targetPort}" }];

        if (!document.Components.Schemas.ContainsKey(nameof(ErrorSTO)))
        {
            document.Components.Schemas.Add(nameof(ErrorSTO), new OpenApiSchema
            {
                Type = "object",
                Properties = new Dictionary<string, OpenApiSchema>
                {
                    ["code"] = new OpenApiSchema { Type = "string" },
                    ["trace_id"] = new OpenApiSchema { Type = "string" },
                    ["message"] = new OpenApiSchema { Type = "string" }
                }
            });
        }
        if (!document.Components.SecuritySchemes.ContainsKey("Bearer"))
        {
            document.Components.SecuritySchemes.Add("Bearer", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header
            });
        }
        return Task.CompletedTask;
    });
});

// --- 6. Database (API Only) ---
builder.Services.AddDbContextFactory<AppDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "Host=localhost;Database=dummy;";
    options.UseNpgsql(connectionString).UseSnakeCaseNamingConvention();
});

// --- 7. AWS S3 (API Only) ---
var awsSection = builder.Configuration.GetSection("AWS");
builder.Services.Configure<BaseAWSS3Options>(awsSection);
var awsOptions = builder.Configuration.GetAWSOptions();
if (!string.IsNullOrEmpty(awsSection["AccessKey"]))
    awsOptions.Credentials = new Amazon.Runtime.BasicAWSCredentials(awsSection["AccessKey"], awsSection["SecretKey"]);
awsOptions.Region = Amazon.RegionEndpoint.GetBySystemName(awsSection["Region"] ?? "ap-northeast-2");
builder.Services.AddDefaultAWSOptions(awsOptions);
builder.Services.AddAWSService<IAmazonS3>();

// --- 8. SignalR 발신 기능 (Redis Backplane 연동) ---
var redisSection = builder.Configuration.GetSection("REDIS");
var redisSignal = redisSection["REDIS_SIGNAL_NAME"] ?? "Undefined";
builder.Services.AddSignalR()
    .AddStackExchangeRedis(options =>
    {
        options.Configuration = ConfigurationOptions.Parse(redisSection["Configuration"] ?? "localhost:6379");
        options.Configuration.ChannelPrefix = new RedisChannel(redisSignal, RedisChannel.PatternMode.Literal);
    });

// --- 9. Monitoring ---
builder.Services.AddSingleton<ApiMetrics>();
builder.Services.AddOpenTelemetry().WithMetrics(m => m.AddMeter("Performance").AddAspNetCoreInstrumentation().AddPrometheusExporter());

// --- Loggin Level ---
builder.Logging.AddFilter("Microsoft.AspNetCore", LogLevel.Warning);
builder.Logging.AddFilter("Microsoft.AspNetCore.Routing", LogLevel.Warning);
builder.Logging.AddFilter("Microsoft.AspNetCore.Cors", LogLevel.Warning);
builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning);
// 필요한 경우 아래 라인도 추가하여 더 정숙하게 만듭니다.
builder.Logging.AddFilter("Microsoft.AspNetCore.Hosting.Diagnostics", LogLevel.Warning);
builder.Logging.AddFilter("Microsoft.AspNetCore.StaticFiles", LogLevel.Warning);

// --- 서비스 등록 마무리 ---
Api.App.ServiceRegistration.AddApplicationServices(builder.Services, builder.Configuration);

var app = builder.Build();

// --- 10. Pipeline ---
app.UseForwardedHeaders(new ForwardedHeadersOptions { ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto });
app.UseCors();
if (!app.Environment.IsDevelopment())
{
    app.UseResponseCompression();
}
app.UseCorrelationId();
app.UseApiMetrics();
app.UseGlobalExceptionHandling();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(o => o.WithTitle("System API").WithTheme(ScalarTheme.Moon));
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapPrometheusScrapingEndpoint();
app.MapHealthChecks("/health");

app.Run();