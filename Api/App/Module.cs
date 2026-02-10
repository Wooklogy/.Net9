using Api.App.Auth;
using Api.App.Auth.Interfaces;
using Api.App.File;
using Api.App.File.Interfaces;

namespace Api.App;
public static class ServiceRegistration
{
    public static void AddApplicationServices(
        IServiceCollection services,
        IConfiguration configuration
    )
    {
        // ✅ App / Domain Services

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IAuthRepository, AuthRepository>();

        services.AddScoped<IFileService, FileService>();
        services.AddScoped<IFileRepository, FileRepository>();


        services.AddHttpContextAccessor();
    }
}