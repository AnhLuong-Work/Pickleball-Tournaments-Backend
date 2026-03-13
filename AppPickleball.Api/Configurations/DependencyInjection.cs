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
            services.AddScoped<ICurrentUserService, CurrentUserService>();

            return services;
        }
    }

}
