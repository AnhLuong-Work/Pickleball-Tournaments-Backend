namespace AppPickleball.Api.Configurations
{
    public static class CorsConfiguration
    {
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
