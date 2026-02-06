using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;
using Share.Models;

namespace Api.Config.Error;

public sealed class ErrorResponseTransformer : IOpenApiOperationTransformer
{
    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        // 1. 해당 액션에 붙은 ProducesErrorCodesAttribute를 가져옵니다.
        var attr = context.Description.ActionDescriptor.EndpointMetadata
            .OfType<ProducesErrorCodesAttribute>()
            .FirstOrDefault();

        if (attr == null) return Task.CompletedTask;

        
        foreach (var code in attr.StatusCodes)
        {
            var key = code.ToString();
            if (!operation.Responses.ContainsKey(key))
            {
                operation.Responses.Add(key, new OpenApiResponse
                {
                    Description = GetDescription(code),
                    Content = new Dictionary<string, OpenApiMediaType>
                    {
                        ["application/json"] = new OpenApiMediaType
                        {
                            // 3. 스키마를 직접 정의하거나 Reference를 사용합니다.
                            // 가장 확실한 방법은 Reference를 사용하는 것입니다.
                            Schema = new OpenApiSchema
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.Schema,
                                    Id = nameof(ErrorSTO)
                                }
                            }
                        }
                    }
                });
            }
        }
        return Task.CompletedTask;
    }

    private static string GetDescription(int code) => code switch
    {
        400 => "Bad Request",
        401 => "Unauthorized",
        403 => "Forbidden",
        404 => "Not Found",
        409 => "Conflict",
        500 => "Internal Server Error",
        _ => "Error"
    };
}