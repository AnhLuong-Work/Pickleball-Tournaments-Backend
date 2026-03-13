using AppPickleball.Application.Common.Settings;

namespace AppPickleball.Api.Configurations
{
    public static class JwtConfiguration
    {
        public static IServiceCollection AddJwtSettings(this IServiceCollection services, IConfiguration config)
        {
            services.Configure<JwtSettings>(config.GetSection("Jwt"));
            return services;
        }
    }
}
