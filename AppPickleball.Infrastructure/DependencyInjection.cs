using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using AppPickleball.Application.Common.Interfaces;
using AppPickleball.Application.Common.Services;
using AppPickleball.Infrastructure.Persistence;
using AppPickleball.Infrastructure.Persistence.Repositories;
using AppPickleball.Infrastructure.Services;

namespace AppPickleball.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Database
            services.AddDbContext<AppPickleballDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"),
                npgsql => npgsql.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)
                ));

            services.AddScoped<IBaseDbContext>(provider =>
                provider.GetRequiredService<AppPickleballDbContext>());

            // Unit of Work + Generic Repository
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped(typeof(ISoftDeletableRepository<>), typeof(SoftDeletableRepository<>));

            // Services
            services.AddScoped<IPasswordHasher, PasswordHasher>();
            services.AddScoped<IEmailService, EmailService>();


            return services;
        }
    }
}
