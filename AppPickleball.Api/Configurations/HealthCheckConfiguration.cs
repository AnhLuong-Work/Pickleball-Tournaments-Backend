using AppPickleball.Infrastructure.Persistence;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace AppPickleball.Api.Configurations;

public static class HealthCheckConfiguration
{
    public static IServiceCollection AddHealthChecks(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddHealthChecks()
            // Kiểm tra kết nối PostgreSQL
            .AddNpgSql(
                connectionString: configuration.GetConnectionString("DefaultConnection")!,
                name: "postgresql",
                failureStatus: HealthStatus.Unhealthy,
                tags: ["db", "postgresql"])
            // Kiểm tra DbContext (EF Core)
            .AddDbContextCheck<AppPickleballDbContext>(
                name: "ef-core",
                failureStatus: HealthStatus.Unhealthy,
                tags: ["db", "ef-core"]);

        return services;
    }
}
