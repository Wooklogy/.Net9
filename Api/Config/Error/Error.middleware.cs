using Share.Models; // 반드시 추가되어야 합니다.

namespace Api.Config.Error;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger = logger;

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleAsync(context, ex);
        }
    }

    private async Task HandleAsync(HttpContext context, Exception ex)
    {
        // 로직별 커스텀 예외 분기
        var (statusCode, errorCode, level) = ex switch
        {
            BadRequestException => (400, "ERR-400", LogLevel.Warning),
            UnauthorizedAccessException => (401, "ERR-401", LogLevel.Warning),
            ForbiddenException => (403, "ERR-403", LogLevel.Warning),
            NotFoundException => (404, "ERR-404", LogLevel.Warning),
            ConflictException => (409, "ERR-409", LogLevel.Warning),
            _ => (500, "ERR-500", LogLevel.Error)
        };

        var traceId = context.Request.Headers["X-Correlation-Id"].FirstOrDefault()
                      ?? context.TraceIdentifier;

        _logger.Log(level, ex, "{ErrorCode} | TraceId: {TraceId} | {Message}", errorCode, traceId, ex.Message);

        if (context.Response.HasStarted) return;

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var response = new ErrorSTO(errorCode, traceId, statusCode == 500 ? "Unexpected error occurred" : ex.Message);
        await context.Response.WriteAsJsonAsync(response);
    }
}
