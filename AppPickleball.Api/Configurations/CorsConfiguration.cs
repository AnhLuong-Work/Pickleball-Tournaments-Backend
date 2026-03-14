namespace AppPickleball.Api.Configurations
{
    public static class CorsConfiguration
    {
        public static IServiceCollection AddCorsPolicy(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("AllowWebClients", builder =>
                {
                    if (environment.IsDevelopment())
                    {
                        // Dev: cho phép tất cả origins để tiện test local
                        builder.SetIsOriginAllowed(_ => true)
                            .AllowAnyMethod()
                            .AllowAnyHeader()
                            .AllowCredentials();
                    }
                    else
                    {
                        // Production: chỉ cho phép origins được cấu hình trong appsettings
                        var allowedOrigins = configuration.GetSection("AllowedOrigins").Get<string[]>() ?? [];
                        builder.WithOrigins(allowedOrigins)
                            .AllowAnyMethod()
                            .AllowAnyHeader()
                            .AllowCredentials();
                    }
                });
            });

            return services;
        }
    }
}
