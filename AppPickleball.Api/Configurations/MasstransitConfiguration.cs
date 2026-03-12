using AppPickleball.Application.Common.Settings;
using MassTransit;
using Microsoft.Extensions.Options;

namespace AppPickleball.Api.Configurations
{
    public static class MasstransitConfiguration
    {
        public static IServiceCollection AddMassTransitConfig(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddMassTransit(x =>
            {
                x.UsingRabbitMq((context, cfg) =>
                {
                    // Get RabbitMQ settings from DI
                    var rabbitMqSettings = context.GetRequiredService<IOptions<RabbitMQSettings>>().Value;

                    if (string.IsNullOrEmpty(rabbitMqSettings.Host))
                    {
                        throw new InvalidOperationException("RabbitMQ Host is not configured in appsettings.json.");
                    }

                    cfg.Host(rabbitMqSettings.Host, rabbitMqSettings.VHost, h =>
                    {
                        if (!string.IsNullOrEmpty(rabbitMqSettings.Username))
                        {
                            h.Username(rabbitMqSettings.Username);
                        }
                        if (!string.IsNullOrEmpty(rabbitMqSettings.Password))
                        {
                            h.Password(rabbitMqSettings.Password);
                        }
                    });
                });
            });

            return services;
        }
    }

}
