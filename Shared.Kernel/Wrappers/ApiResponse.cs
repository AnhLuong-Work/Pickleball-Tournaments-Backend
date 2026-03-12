namespace Shared.Kernel.Wrappers;

// API response chuẩn
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public int StatusCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public MetaResponse Meta { get; set; } = new();
    public List<string>? ErrorCodes { get; set; }
    public T? Data { get; set; }

    public static ApiResponse<T> SuccessResponse(T data, string message = "Success", int statusCode = 200)
    {
        return new ApiResponse<T>
        {
            Success = true,
            StatusCode = statusCode,
            Message = message,
            Data = data,
            Meta = new MetaResponse()
        };
    }

    public static ApiResponse<T> FailureResponse(
        string message,
        int statusCode = 400,
        List<string>? errorCodes = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            StatusCode = statusCode,
            Message = message,
            ErrorCodes = errorCodes,
            Data = default,
            Meta = new MetaResponse()
        };
    }

    public static ApiResponse<T> FailureResponse(
        string message,
        string errorCode,
        int statusCode = 400)
    {
        return new ApiResponse<T>
        {
            Success = false,
            StatusCode = statusCode,
            Message = message,
            ErrorCodes = new List<string> { errorCode },
            Data = default,
            Meta = new MetaResponse()
        };
    }
}

// API response không có data
public class ApiResponse : ApiResponse<object>
{
    public static ApiResponse SuccessResponse(string message = "Success", int statusCode = 200)
    {
        return new ApiResponse
        {
            Success = true,
            StatusCode = statusCode,
            Message = message,
            Data = null,
            Meta = new MetaResponse()
        };
    }

    public static new ApiResponse FailureResponse(
        string message,
        int statusCode = 400,
        List<string>? errorCodes = null)
    {
        return new ApiResponse
        {
            Success = false,
            StatusCode = statusCode,
            Message = message,
            ErrorCodes = errorCodes,
            Data = null,
            Meta = new MetaResponse()
        };
    }
}
