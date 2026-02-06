using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace Api.Config.Authorization;

public sealed class AuthDescriptionTransformer : IOpenApiOperationTransformer
{
    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        var actionDescriptor = context.Description.ActionDescriptor as ControllerActionDescriptor;
        if (actionDescriptor == null) return Task.CompletedTask;

        // 1. 리플렉션으로 특성 추출
        var authAttr = actionDescriptor.MethodInfo.GetCustomAttribute<AuthorizePermissionAttribute>() 
                      ?? actionDescriptor.ControllerTypeInfo.GetCustomAttribute<AuthorizePermissionAttribute>();

        if (authAttr == null)
        {
            operation.Security.Clear();
            return Task.CompletedTask;
        }

        // 2. 권한 정보 마크다운 작성
        var authInfo = new StringBuilder();
        authInfo.AppendLine("\n\n---");
        authInfo.AppendLine("### 🔒 **Security Requirements**");
        
        if (authAttr.Role?.Any() == true)
            authInfo.AppendLine($"- **Allowed Roles**: `{string.Join("`, `", authAttr.Role)}` ");
            
        if (authAttr.Permissions?.Any() == true)
            authInfo.AppendLine($"- **Permissions**: `{string.Join("`, `", authAttr.Permissions)}` ");

        // ✅ 급소: Scalar가 EndpointDescription을 우선시하므로, 강제로 합쳐버립니다.
        operation.Description = (operation.Description ?? "") + authInfo.ToString();
        
        // 추가로 Summary 옆에도 자물쇠 표시를 넣어 시각적 효과를 극대화합니다.
        operation.Summary = $"[🔒] {operation.Summary}";

        // 3. 보안 스키마 연결 확인
        operation.Security = new List<OpenApiSecurityRequirement>
        {
            new() { [new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } }] = Array.Empty<string>() }
        };

        return Task.CompletedTask;
    }
}