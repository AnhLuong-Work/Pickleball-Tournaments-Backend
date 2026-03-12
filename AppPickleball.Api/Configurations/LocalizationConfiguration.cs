using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Options;
using System.Globalization;

namespace AppPickleball.Api.Configurations
{
    public static class LocalizationConfiguration
    {
        public static IServiceCollection AddAppLocalization(this IServiceCollection services)
        {
            services.AddLocalization();

            var supportedCultures = new[] { "vi", "en" };

            services.Configure<RequestLocalizationOptions>(options =>
            {
                options.DefaultRequestCulture = new RequestCulture("vi");
                options.SupportedCultures = supportedCultures
                    .Select(c => new CultureInfo(c))
                    .ToList();
                options.SupportedUICultures = supportedCultures
                    .Select(c => new CultureInfo(c))
                    .ToList();
                options.ApplyCurrentCultureToResponseHeaders = true;
            });

            return services;
        }

        public static IApplicationBuilder UseAppLocalization(this IApplicationBuilder app)
        {
            // Lấy cấu hình localization đã đăng ký
            var options = app.ApplicationServices
                .GetRequiredService<IOptions<RequestLocalizationOptions>>().Value;
            // Áp dụng localization vào pipeline
            app.UseRequestLocalization(options);
            return app;
        }
    }
}
