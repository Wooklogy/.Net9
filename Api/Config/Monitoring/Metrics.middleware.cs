using System.Diagnostics;

namespace Api.Config.Monitoring;

public class MetricsMiddleware(RequestDelegate next, ApiMetrics metrics)
{
    private readonly RequestDelegate _next = next;
    private readonly ApiMetrics _metrics = metrics;

    public async Task InvokeAsync(HttpContext context)
    {
        long start = Stopwatch.GetTimestamp();

        try
        {
            await _next(context);
        }
        finally
        {
            var elapsedMs = Stopwatch.GetElapsedTime(start).TotalMilliseconds;

            // 수치 데이터만 기록 (문자열 보간이나 I/O 작업 없음)
            _metrics.RecordRequest(
                context.Request.Method,
                context.Request.Path.Value ?? "unknown",
                context.Response.StatusCode,
                elapsedMs
            );
        }
    }
}