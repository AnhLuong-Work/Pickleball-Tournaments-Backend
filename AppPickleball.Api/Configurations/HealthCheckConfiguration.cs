using AppPickleball.Infrastructure.Persistence;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace AppPickleball.Api.Configurations;

public static class HealthCheckConfiguration
{
    public static IServiceCollection AddHealthChecksConfig(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddHealthChecks()
            .AddNpgSql(
                connectionString: configuration.GetConnectionString("DefaultConnection")!,
                name: "postgresql",
                failureStatus: HealthStatus.Unhealthy,
                tags: ["db", "postgresql"])
            .AddDbContextCheck<AppPickleballDbContext>(
                name: "ef-core",
                failureStatus: HealthStatus.Unhealthy,
                tags: ["db", "ef-core"]);

        // HealthChecks UI — dashboard tại /health-ui
        services.AddHealthChecksUI(options =>
        {
            options.SetEvaluationTimeInSeconds(30); // Poll mỗi 30s
            options.AddHealthCheckEndpoint("AppPickleball API", "/health");
        })
        .AddInMemoryStorage();

        return services;
    }

    public static IApplicationBuilder UseHealthChecksEndpoints(this IApplicationBuilder app)
    {
        app.UseHealthChecks("/health", new HealthCheckOptions
        {
            // Trả JSON chi tiết thay vì plain text
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

        app.UseHealthChecksUI(options =>
        {
            options.UIPath = "/health-ui";
            options.ApiPath = "/health-api";
        });

        return app;
    }
}
