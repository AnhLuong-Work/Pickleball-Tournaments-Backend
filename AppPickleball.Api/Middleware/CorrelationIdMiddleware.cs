using Shared.Kernel.Wrappers;

namespace AppPickleball.Api.Middleware;

/// <summary>
/// Middleware tạo và truyền Correlation ID cho mỗi request.
/// Hỗ trợ distributed tracing giữa các microservices.
/// Flow: Header → CorrelationIdAccessor → MetaResponse + Serilog LogContext + Response Header
/// </summary>
public class CorrelationIdMiddleware
{
    private const string CorrelationIdHeader = "X-Correlation-Id";
    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Lấy correlation ID từ header (gateway gửi) hoặc tạo mới
        if (!context.Request.Headers.TryGetValue(CorrelationIdHeader, out var correlationId)
            || string.IsNullOrWhiteSpace(correlationId))
        {
            correlationId = Guid.NewGuid().ToString();
        }

        var correlationIdStr = correlationId.ToString();

        // Set vào accessor → MetaResponse tự đọc khi tạo ApiResponse
        CorrelationIdAccessor.CorrelationId = correlationIdStr;

        // Gắn vào HttpContext để handler/service có thể truy cập
        context.Items[CorrelationIdHeader] = correlationIdStr;

        // Thêm vào response header để client/gateway nhận lại
        context.Response.OnStarting(() =>
        {
            context.Response.Headers[CorrelationIdHeader] = correlationIdStr;
            return Task.CompletedTask;
        });

        // Thêm vào Serilog log context
        using (Serilog.Context.LogContext.PushProperty("CorrelationId", correlationIdStr))
        {
            await _next(context);
        }
    }
}

