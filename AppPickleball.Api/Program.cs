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

// Đăng ký Serilog — đọc config từ appsettings.json section "Serilog"
builder.Host.UseSerilog((context, loggerConfig) =>
    loggerConfig.ReadFrom.Configuration(context.Configuration));

builder.Services.AddApiDependencies(config);
builder.Services.AddJwtSettings(config);
builder.Services.AddAllSettings(config);
builder.Services.AddApiControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerModule();
//builder.Services.AddMassTransitConfig(config);
builder.Services.AddAppLocalization();
builder.Services.AddCorsPolicy();
builder.Services.AddHealthChecks(config);
builder.Services.AddApiVersioningConfig();

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

// Configure the HTTP request pipeline.
// Correlation ID — phải đặt đầu tiên để track toàn bộ request lifecycle
app.UseMiddleware<CorrelationIdMiddleware>();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseAppLocalization();
app.UseHttpsRedirection();
app.UseCors("AllowWebClients");
app.UseMiddleware<ExceptionHandlerMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

