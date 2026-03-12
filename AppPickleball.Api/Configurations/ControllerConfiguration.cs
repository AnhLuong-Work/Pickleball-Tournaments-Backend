using System.Text.Json.Serialization;

namespace AppPickleball.Api.Configurations
{
    public static class ControllerConfiguration
    {
        public static IServiceCollection AddApiControllers(this IServiceCollection services)
        {
            services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(
                        new JsonStringEnumConverter()
                    );
                });

            return services;
        }
    }
}
