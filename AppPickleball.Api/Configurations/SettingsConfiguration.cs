using AppPickleball.Application.Common.Settings;

namespace AppPickleball.Api.Configurations
{
    public static class SettingsConfiguration
    {
        public static IServiceCollection AddAllSettings(this IServiceCollection services, IConfiguration config)
        {
            services.Configure<EmailSettings>(config.GetSection("EmailSettings"));
            services.Configure<AuthSettings>(config.GetSection("AuthSettings"));
            services.Configure<RabbitMQSettings>(config.GetSection("RabbitMQ"));
            services.Configure<GoogleAuthSettings>(config.GetSection("GoogleAuth"));

            return services;
        }
    }
}
