using AppPickleball.Application.Common.Exceptions;
using Shared.Kernel.Wrappers;
using System.Net;
using System.Text.Json;

namespace AppPickleball.Api.Middleware
{
    public class ExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlerMiddleware> _logger;

        public ExceptionHandlerMiddleware(RequestDelegate next, ILogger<ExceptionHandlerMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception error)
            {
                _logger.LogError(error, "Một lỗi không xác định đã xảy ra: {ErrorMessage}", error.Message);
                var response = context.Response;
                response.ContentType = "application/json";

                // Xác định statusCode và errorCode theo loại exception
                ApiResponse<string> apiResponse;

                switch (error)
                {
                    case ValidationException e:
                        response.StatusCode = (int)HttpStatusCode.BadRequest;
                        var firstErrorMessage = e.Errors.Values.SelectMany(v => v).FirstOrDefault();
                        apiResponse = ApiResponse<string>.FailureResponse(
                            firstErrorMessage ?? "Validation failed.",
                            statusCode: (int)HttpStatusCode.BadRequest,
                            errorCodes: new List<string> { "VALIDATION_ERROR" });
                        break;
                    case FluentValidation.ValidationException e:
                        response.StatusCode = (int)HttpStatusCode.BadRequest;
                        var firstFluentError = e.Errors.FirstOrDefault()?.ErrorMessage;
                        apiResponse = ApiResponse<string>.FailureResponse(firstFluentError ?? "Validation failed.", 
                            statusCode: (int)HttpStatusCode.BadRequest,
                            errorCodes: new List<string> { "VALIDATION_ERROR" });
                        break;
                    case NotFoundException e:
                        response.StatusCode = (int)HttpStatusCode.NotFound;
                        apiResponse = ApiResponse<string>.FailureResponse(
                            e.Message,
                            errorCode: "NOT_FOUND",
                            statusCode: (int)HttpStatusCode.NotFound);
                        break;
                    case BadHttpRequestException e:
                        response.StatusCode = (int)HttpStatusCode.BadRequest;
                        apiResponse = ApiResponse<string>.FailureResponse(
                            e.Message,
                            errorCode: "BAD_REQUEST",
                            statusCode: (int)HttpStatusCode.BadRequest);
                        break;
                    case DomainException exception:
                        response.StatusCode = (int)HttpStatusCode.BadRequest;
                        apiResponse = ApiResponse<string>.FailureResponse(
                            exception.Message,
                            errorCode: "DOMAIN_ERROR",
                            statusCode: (int)HttpStatusCode.BadRequest);
                        break;
                    default:
                        response.StatusCode = (int)HttpStatusCode.InternalServerError;
                        apiResponse = ApiResponse<string>.FailureResponse(
                            "An unexpected error occurred.",
                            errorCode: "INTERNAL_SERVER_ERROR",
                            statusCode: (int)HttpStatusCode.InternalServerError);
                        break;
                }

                var jsonResponse = JsonSerializer.Serialize(apiResponse, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                await response.WriteAsync(jsonResponse);
            }
        }
    }

}
