using AppPickleball.Api.Filters;
using Microsoft.OpenApi.Models;

namespace AppPickleball.Api.Configurations
{
    public static class SwaggerConfiguration
    {
        public static IServiceCollection AddSwaggerModule(this IServiceCollection services)
        {
            services.AddEndpointsApiExplorer();

            services.AddSwaggerGen(c =>
            {
                // Định nghĩa tài liệu Swagger với version
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "AppPickleball API",
                    Version = "v1",
                    Description = "Pickleball Tournament Management — Auth, Tournaments, Matches, Community"
                });

                // JWT Bearer Auth
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    Description = "Nhập JWT token. Ví dụ: Bearer {token}"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new List<string>()
                    }
                });
                // Bộ lọc thêm header Accept-Language
                c.OperationFilter<AcceptLanguageHeaderOperationFilter>();
                // Cho phép sử dụng chú thích
                c.EnableAnnotations();
                // Hiện thị enum inline
                c.UseInlineDefinitionsForEnums();
            });

            return services;
        }
    }
}
