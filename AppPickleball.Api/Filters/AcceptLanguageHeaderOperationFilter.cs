using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AppPickleball.Api.Filters
{
    public class AcceptLanguageHeaderOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            //if (operation.Parameters == null)
            //    operation.Parameters = new List<OpenApiParameter>();
            //#pragma warning disable IDE0028 // Collection initialization can be simplified
            operation.Parameters ??= new List<OpenApiParameter>();
            //#pragma warning restore IDE0028

            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "Accept-Language",
                In = ParameterLocation.Header,
                Required = false,
                Description = "Ngôn ngữ phản hồi (ví dụ: vi hoặc en)",
                Schema = new OpenApiSchema
                {
                    Type = "string",
                    //Enum = new List<IOpenApiAny>
                    //{
                    //    new OpenApiString("vi"),
                    //    new OpenApiString("en")
                    //},
                    // tương đương với cách viết trên nhưng dùng cú pháp khởi tạo danh sách rút gọn
                    Enum =
                    [
                        new OpenApiString("vi"),
                        new OpenApiString("en")
                    ],
                    Default = new OpenApiString("vi")
                }
            });
        }
    }
}
