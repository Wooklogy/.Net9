using Api.Config.Monitoring;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

public static class MiddlewareExtension
{
    public static IApplicationBuilder UseCorrelationId(
        this IApplicationBuilder app)
    {
        return app.UseMiddleware<Api.Config.Correlation.CorrelationIdMiddleware>();
    }

    // 성능 측정을 위한 메트릭 미들웨어 추가
    public static IApplicationBuilder UseApiMetrics(this IApplicationBuilder app)
    {
        return app.UseMiddleware<MetricsMiddleware>();
    }

    public static IApplicationBuilder UseGlobalExceptionHandling(
        this IApplicationBuilder app)
    {
        return app.UseMiddleware<Api.Config.Error.ExceptionHandlingMiddleware>();
    }
}


public sealed class ApiPrefixConvention : IApplicationModelConvention
{
    private readonly AttributeRouteModel _routePrefix;

    public ApiPrefixConvention(string prefix)
    {
        _routePrefix = new AttributeRouteModel(
            new Microsoft.AspNetCore.Mvc.RouteAttribute(prefix)
        );
    }

    public void Apply(ApplicationModel application)
    {
        foreach (var controller in application.Controllers)
        {
            foreach (var selector in controller.Selectors)
            {
                if (selector.AttributeRouteModel != null)
                {
                    selector.AttributeRouteModel =
                        AttributeRouteModel.CombineAttributeRouteModel(
                            _routePrefix,
                            selector.AttributeRouteModel
                        );
                }
                else
                {
                    selector.AttributeRouteModel = _routePrefix;
                }
            }
        }
    }
}
