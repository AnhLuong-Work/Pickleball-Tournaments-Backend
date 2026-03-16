using AppPickleball.Api.Configurations;
using AppPickleball.Api.Middleware;
using AppPickleball.Application;
using AppPickleball.Infrastructure;
using AppPickleball.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Text;

Console.OutputEncoding = Encoding.UTF8;
var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;
var env = builder.Environment;

// Đăng ký Serilog — đọc config từ appsettings.json section "Serilog"
builder.Host.UseSerilog((context, loggerConfig) =>
    loggerConfig.ReadFrom.Configuration(context.Configuration));

builder.Services.AddApiDependencies(config);
builder.Services.AddAllSettings(config);
builder.Services.AddApiControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerModule();
//builder.Services.AddMassTransitConfig(config);
builder.Services.AddAppLocalization();
builder.Services.AddCorsPolicy(config, env);
builder.Services.AddApiVersioningConfig();
builder.Services.AddRateLimiterConfig();

// Add layers
builder.Services.AddApplication();
builder.Services.AddInfrastructure(config);

var app = builder.Build();

// Auto-migrate khi khởi động (áp dụng pending migrations tự động)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppPickleballDbContext>();
    await db.Database.MigrateAsync();
}

// ─── Middleware pipeline ───
// Thứ tự quan trọng: CorrelationId → ExceptionHandler trước hết để catch mọi lỗi

// 1. Correlation ID — đặt đầu tiên để track toàn bộ request lifecycle
app.UseMiddleware<CorrelationIdMiddleware>();

// 2. Exception handler — đặt sớm để bắt lỗi từ mọi middleware phía sau
app.UseMiddleware<ExceptionHandlerMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 3. Rate limiting — chặn trước khi request vào pipeline
app.UseRateLimiter();

app.UseAppLocalization();
app.UseHttpsRedirection();
app.UseCors("AllowWebClients");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
