using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Share.Models;

namespace Api.Config.Validation;

public class ValidationExceptionFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid)
        {
            // 1. TraceId 추출 최적화: 헤더 우선, 없으면 ASP.NET 기본 ID 사용
            var traceId = context.HttpContext.Request.Headers["X-Correlation-Id"].FirstOrDefault() 
                          ?? context.HttpContext.TraceIdentifier;

            // 2. 에러 딕셔너리 추출 (형님의 로직 유지 및 최적화)
            var errors = context.ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value!.Errors
                        .Select(e => string.IsNullOrWhiteSpace(e.ErrorMessage)
                            ? "Invalid value"
                            : e.ErrorMessage)
                        .ToArray()
                );

            // 3. 공통 에러 규격(ErrorSTO)에 맞춰 응답 생성
            var response = new ErrorSTO(
                code: "ERR-400",
                trace_id: traceId,
                message: "Input validation failed"
            );

            // TODO: 추후 클라이언트 요구에 따라 response 모델에 errors 필드를 추가하여 
            // 구체적으로 어떤 필드가 틀렸는지(예: Symbol, Price 등) 반환하도록 확장 가능합니다.

            context.Result = new BadRequestObjectResult(response);
        }
    }

    public void OnActionExecuted(ActionExecutedContext context) { }
}