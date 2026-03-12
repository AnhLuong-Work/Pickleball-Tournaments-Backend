using Asp.Versioning;

namespace AppPickleball.Api.Configurations;

/// <summary>
/// Cấu hình API Versioning — hỗ trợ URL segment versioning (/api/v1/...).
/// Khi thêm version mới, tạo controller mới với [ApiVersion("2.0")].
/// </summary>
public static class ApiVersioningConfiguration
{
    public static IServiceCollection AddApiVersioningConfig(this IServiceCollection services)
    {
        services.AddApiVersioning(options =>
        {
            // Mặc định version 1.0 nếu client không chỉ định
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;

            // Cho phép đọc version từ URL segment, header, hoặc query string
            options.ReportApiVersions = true;
            options.ApiVersionReader = ApiVersionReader.Combine(
                new UrlSegmentApiVersionReader(),
                new HeaderApiVersionReader("X-Api-Version")
            );
        })
        .AddApiExplorer(options =>
        {
            // Format version trong URL: v1, v2, v3...
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });

        return services;
    }
}
