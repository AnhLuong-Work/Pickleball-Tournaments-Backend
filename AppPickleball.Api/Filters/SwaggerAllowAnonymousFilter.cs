using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AppPickleball.Api.Filters;

/// <summary>
/// Xóa security requirement (lock icon) khỏi các endpoint có [AllowAnonymous].
/// Giúp Swagger UI hiển thị đúng: endpoint public không cần token.
/// </summary>
public class SwaggerAllowAnonymousFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var hasAllowAnonymous = context.MethodInfo
            .GetCustomAttributes(true)
            .OfType<AllowAnonymousAttribute>()
            .Any()
            || (context.MethodInfo.DeclaringType?
                .GetCustomAttributes(true)
                .OfType<AllowAnonymousAttribute>()
                .Any() ?? false);

        if (hasAllowAnonymous)
        {
            operation.Security.Clear();
        }
    }
}
