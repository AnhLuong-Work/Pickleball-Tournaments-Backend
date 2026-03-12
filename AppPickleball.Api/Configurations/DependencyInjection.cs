using AppPickleball.Api.Services;
using AppPickleball.Application.Common.Services;
using AppPickleball.Application.Common.Settings;

namespace AppPickleball.Api.Configurations
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApiDependencies(this IServiceCollection services, IConfiguration config)
        {
            // Đăng ký các service tầng API
            services.AddHttpContextAccessor();
            // Configure Options Pattern - All Settings
            services.Configure<JwtSettings>(config.GetSection("Jwt"));
            services.Configure<EmailSettings>(config.GetSection("EmailSettings"));
            services.Configure<AuthSettings>(config.GetSection("AuthSettings"));
            services.Configure<RabbitMQSettings>(config.GetSection("RabbitMQ"));
            services.Configure<GoogleAuthSettings>(config.GetSection("GoogleAuth"));
            services.AddScoped<ICurrentUserService, CurrentUserService>();

            return services;
        }
        public static IServiceCollection AddCorsPolicy(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("AllowWebClients", builder =>
                {
                    builder.SetIsOriginAllowed(origin => true) // Allow all origins for testing
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials(); // Bắt buộc cho cookie-based auth
                });
            });

            return services;
        }
    }

}
