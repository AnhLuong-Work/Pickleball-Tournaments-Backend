using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AppPickleball.Api.Filters
{
    public class AcceptLanguageHeaderOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            operation.Parameters ??= new List<OpenApiParameter>();

            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "Accept-Language",
                In = ParameterLocation.Header,
                Required = false,
                Description = "Ngôn ngữ phản hồi (ví dụ: vi hoặc en)",
                Schema = new OpenApiSchema
                {
                    Type = "string",
                    Enum = [new OpenApiString("vi"), new OpenApiString("en")],
                    Default = new OpenApiString("vi")
                }
            });
        }
    }
}
