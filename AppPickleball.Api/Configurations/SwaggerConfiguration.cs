using AppPickleball.Api.Filters;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

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
                // Thêm header Accept-Language vào mọi endpoint
                c.OperationFilter<AcceptLanguageHeaderOperationFilter>();
                // Xóa lock icon khỏi endpoint [AllowAnonymous]
                c.OperationFilter<SwaggerAllowAnonymousFilter>();
                c.EnableAnnotations();
                c.UseInlineDefinitionsForEnums();
            });

            return services;
        }
    }
}
