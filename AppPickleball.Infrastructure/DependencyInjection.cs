using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using AppPickleball.Application.Common.Interfaces;
using AppPickleball.Application.Common.Services;
using AppPickleball.Application.Common.Settings;
using AppPickleball.Application.Features.Auth.Interfaces;
using AppPickleball.Application.Features.Matches.Interfaces;
using AppPickleball.Application.Features.Participants.Interfaces;
using AppPickleball.Application.Features.Teams.Interfaces;
using AppPickleball.Application.Features.Tournaments.Interfaces;
using AppPickleball.Application.Features.Users.Interfaces;
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

            // Specific repositories
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
            services.AddScoped<IUserAuthProviderRepository, UserAuthProviderRepository>();
            services.AddScoped<ITournamentRepository, TournamentRepository>();
            services.AddScoped<IParticipantRepository, ParticipantRepository>();
            services.AddScoped<ITeamRepository, TeamRepository>();
            services.AddScoped<IGroupRepository, GroupRepository>();
            services.AddScoped<IMatchRepository, MatchRepository>();
            services.AddScoped<IFollowRepository, FollowRepository>();

            // Services
            services.AddScoped<IPasswordHasher, PasswordHasher>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IJwtService, JwtService>();
            services.AddScoped<IGoogleAuthService, GoogleAuthService>();
            services.AddHttpClient<IFacebookAuthService, FacebookAuthService>();

            // Settings — tập trung tại Infrastructure để tránh đăng ký phân tán
            services.Configure<JwtSettings>(configuration.GetSection("Jwt"));
            services.Configure<GoogleAuthSettings>(configuration.GetSection("GoogleAuth"));
            services.Configure<FacebookAuthSettings>(configuration.GetSection("FacebookAuth"));

            // JWT Authentication
            var jwtSettings = configuration.GetSection("Jwt").Get<JwtSettings>()!;
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
                    ClockSkew = TimeSpan.Zero // Không có grace period, token hết hạn là hết ngay
                };

                // Hỗ trợ SignalR: lấy token từ query string
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                            context.Token = accessToken;
                        return Task.CompletedTask;
                    }
                };
            });

            services.AddAuthorization();

            return services;
        }
    }
}
