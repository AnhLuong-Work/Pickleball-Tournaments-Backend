using AppPickleball.Application.Common.Exceptions;
using Shared.Kernel.Wrappers;
using System.Net;
using System.Text.Json;

namespace AppPickleball.Api.Middleware
{
    public class ExceptionHandlerMiddleware
    {
        // Cache tránh tạo mới mỗi request (JsonSerializerOptions khởi tạo tốn kém)
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

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
                var response = context.Response;
                response.ContentType = "application/json";

                ApiResponse<string> apiResponse;

                switch (error)
                {
                    case ValidationException e:
                        _logger.LogWarning(e, "Validation error: {Message}", e.Message);
                        response.StatusCode = (int)HttpStatusCode.BadRequest;
                        var firstErrorMessage = e.Errors.Values.SelectMany(v => v).FirstOrDefault();
                        apiResponse = ApiResponse<string>.FailureResponse(
                            firstErrorMessage ?? "Validation failed.",
                            statusCode: (int)HttpStatusCode.BadRequest,
                            errorCodes: new List<string> { "VALIDATION_ERROR" });
                        break;

                    case FluentValidation.ValidationException e:
                        _logger.LogWarning(e, "FluentValidation error: {Message}", e.Message);
                        response.StatusCode = (int)HttpStatusCode.BadRequest;
                        var firstFluentError = e.Errors.FirstOrDefault()?.ErrorMessage;
                        apiResponse = ApiResponse<string>.FailureResponse(
                            firstFluentError ?? "Validation failed.",
                            statusCode: (int)HttpStatusCode.BadRequest,
                            errorCodes: new List<string> { "VALIDATION_ERROR" });
                        break;

                    case NotFoundException e:
                        _logger.LogWarning(e, "Not found: {Message}", e.Message);
                        response.StatusCode = (int)HttpStatusCode.NotFound;
                        apiResponse = ApiResponse<string>.FailureResponse(
                            e.Message,
                            errorCode: "NOT_FOUND",
                            statusCode: (int)HttpStatusCode.NotFound);
                        break;

                    case UnauthorizedException e:
                        _logger.LogWarning(e, "Unauthorized: {Message}", e.Message);
                        response.StatusCode = (int)HttpStatusCode.Unauthorized;
                        apiResponse = ApiResponse<string>.FailureResponse(
                            e.Message,
                            errorCode: "UNAUTHORIZED",
                            statusCode: (int)HttpStatusCode.Unauthorized);
                        break;

                    case DomainException e:
                        _logger.LogWarning(e, "Domain error: {Message}", e.Message);
                        response.StatusCode = (int)HttpStatusCode.BadRequest;
                        apiResponse = ApiResponse<string>.FailureResponse(
                            e.Message,
                            errorCode: "DOMAIN_ERROR",
                            statusCode: (int)HttpStatusCode.BadRequest);
                        break;

                    case BadHttpRequestException e:
                        _logger.LogWarning(e, "Bad request: {Message}", e.Message);
                        response.StatusCode = (int)HttpStatusCode.BadRequest;
                        apiResponse = ApiResponse<string>.FailureResponse(
                            e.Message,
                            errorCode: "BAD_REQUEST",
                            statusCode: (int)HttpStatusCode.BadRequest);
                        break;

                    default:
                        _logger.LogError(error, "Unhandled exception: {Message}", error.Message);
                        response.StatusCode = (int)HttpStatusCode.InternalServerError;
                        apiResponse = ApiResponse<string>.FailureResponse(
                            "An unexpected error occurred.",
                            errorCode: "INTERNAL_SERVER_ERROR",
                            statusCode: (int)HttpStatusCode.InternalServerError);
                        break;
                }

                var jsonResponse = JsonSerializer.Serialize(apiResponse, _jsonOptions);
                await response.WriteAsync(jsonResponse);
            }
        }
    }
}
