using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Api.Infra;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        // 1. 설정을 수동으로 구성 (파일이 없어도 에러 안 나게 설정)
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddEnvironmentVariables() // 우선순위 1: 환경변수
            .Build();

        // 2. DbContextOptionsBuilder 설정
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        
        // 환경변수에서 연결 문자열을 못 찾으면 더미 문자열이라도 넣어 에러를 방지합니다.
        var connectionString = configuration.GetConnectionString("DefaultConnection") 
                               ?? "Host=localhost;Database=dummy;Username=dummy;Password=dummy";

        optionsBuilder.UseNpgsql(connectionString)
                      .UseSnakeCaseNamingConvention();

        return new AppDbContext(optionsBuilder.Options);
    }
}