using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace AppPickleball.Api.Configurations;

public static class RateLimiterConfiguration
{
    // Tên policy — dùng trong [EnableRateLimiting("...")]
    public const string AuthPolicy = "auth";
    public const string OtpPolicy = "otp";
    public const string GlobalPolicy = "global";

    public static IServiceCollection AddRateLimiterConfig(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            // Global — áp dụng cho tất cả endpoints không có policy riêng
            // 100 request / 60s / IP
            options.AddPolicy(GlobalPolicy, context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 100,
                        Window = TimeSpan.FromSeconds(60),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0
                    }));

            // Auth — login, register, forgot-password
            // 10 request / 60s / IP — chặn brute-force password
            options.AddPolicy(AuthPolicy, context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 10,
                        Window = TimeSpan.FromSeconds(60),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0
                    }));

            // OTP — send-verification, verify-email, reset-password
            // 3 request / 5 phút / IP — chặn OTP spam
            options.AddPolicy(OtpPolicy, context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 3,
                        Window = TimeSpan.FromMinutes(5),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0
                    }));
        });

        return services;
    }
}
