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
                    Title = "AppPickleball.API",
                    Version = "v1",
                    Description = "Operator Service — Workspace Management, Subscriptions, Orders"
                });

                // 🏢 Header cho Workspace
                c.AddSecurityDefinition("X-Workspace-Id", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Name = "X-Workspace-Id",
                    Type = SecuritySchemeType.ApiKey,
                    Description = "Workspace Id header (bắt buộc cho mỗi request)"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "X-Workspace-Id"
                            },
                            In = ParameterLocation.Header,
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
