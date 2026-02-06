using Microsoft.Extensions.Primitives;

namespace Api.Config.Correlation;

public class CorrelationIdMiddleware
{
    public const string HeaderName = "X-Correlation-Id";
    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;

    public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // 1. 요청 헤더에서 Correlation ID 추출 (없으면 새로 생성)
        if (!context.Request.Headers.TryGetValue(HeaderName, out StringValues correlationId) || 
            string.IsNullOrWhiteSpace(correlationId))
        {
            correlationId = Guid.NewGuid().ToString();
        }

        // 2. 응답 헤더에 설정 (클라이언트가 추적할 수 있도록 반환)
        context.Response.OnStarting(() =>
        {
            if (!context.Response.Headers.ContainsKey(HeaderName))
            {
                context.Response.Headers.Append(HeaderName, correlationId);
            }
            return Task.CompletedTask;
        });

        // 3. 로깅 스코프 설정 (이후 모든 로그에 Correlation ID가 자동으로 포함됨)
        using (_logger.BeginScope(new Dictionary<string, object> { [HeaderName] = correlationId.ToString() }))
        {
            // 4. 다음 파이프라인으로 전달
            await _next(context);
        }
    }
}