using Microsoft.AspNetCore.Http;

namespace VideoMeeting.Application.Common.Models;

/// <summary>
///     Helper class for creating standardized API responses
/// </summary>
public static class ApiResponseHelper
{
    /// <summary>
    ///     Creates a successful response with data
    /// </summary>
    public static ApiResponse<T> Success<T>(T data, string message = "Operation completed successfully",
        string? traceId = null)
    {
        return new ApiResponse<T>
        {
            Success = true,
            StatusCode = StatusCodes.Status200OK,
            Message = message,
            Data = data,
            TraceId = traceId ?? Guid.NewGuid().ToString()
        };
    }

    /// <summary>
    ///     Creates a successful response without data
    /// </summary>
    public static ApiResponse Success(string message = "Operation completed successfully", string? traceId = null)
    {
        return new ApiResponse
        {
            Success = true,
            StatusCode = StatusCodes.Status200OK,
            Message = message,
            TraceId = traceId ?? Guid.NewGuid().ToString()
        };
    }

    /// <summary>
    ///     Creates a successful created response (201)
    /// </summary>
    public static ApiResponse<T> Created<T>(T data, string message = "Resource created successfully",
        string? traceId = null)
    {
        return new ApiResponse<T>
        {
            Success = true,
            StatusCode = StatusCodes.Status201Created,
            Message = message,
            Data = data,
            TraceId = traceId ?? Guid.NewGuid().ToString()
        };
    }

    /// <summary>
    ///     Creates a successful no content response (204)
    /// </summary>
    public static ApiResponse NoContent(string message = "Operation completed successfully", string? traceId = null)
    {
        return new ApiResponse
        {
            Success = true,
            StatusCode = StatusCodes.Status204NoContent,
            Message = message,
            TraceId = traceId ?? Guid.NewGuid().ToString()
        };
    }

    /// <summary>
    ///     Creates a bad request error response (400)
    /// </summary>
    public static ApiResponse BadRequest(string message = "Bad request", List<string>? errors = null,
        string? traceId = null)
    {
        return new ApiResponse
        {
            Success = false,
            StatusCode = StatusCodes.Status400BadRequest,
            Message = message,
            Errors = errors,
            TraceId = traceId ?? Guid.NewGuid().ToString()
        };
    }

    /// <summary>
    ///     Creates an unauthorized error response (401)
    /// </summary>
    public static ApiResponse Unauthorized(string message = "Unauthorized access", string? traceId = null)
    {
        return new ApiResponse
        {
            Success = false,
            StatusCode = StatusCodes.Status401Unauthorized,
            Message = message,
            TraceId = traceId ?? Guid.NewGuid().ToString()
        };
    }

    /// <summary>
    ///     Creates a forbidden error response (403)
    /// </summary>
    public static ApiResponse Forbidden(string message = "Access forbidden", string? traceId = null)
    {
        return new ApiResponse
        {
            Success = false,
            StatusCode = StatusCodes.Status403Forbidden,
            Message = message,
            TraceId = traceId ?? Guid.NewGuid().ToString()
        };
    }

    /// <summary>
    ///     Creates a not found error response (404)
    /// </summary>
    public static ApiResponse NotFound(string message = "Resource not found", string? traceId = null)
    {
        return new ApiResponse
        {
            Success = false,
            StatusCode = StatusCodes.Status404NotFound,
            Message = message,
            TraceId = traceId ?? Guid.NewGuid().ToString()
        };
    }

    /// <summary>
    ///     Creates a conflict error response (409)
    /// </summary>
    public static ApiResponse Conflict(string message = "Resource conflict", List<string>? errors = null,
        string? traceId = null)
    {
        return new ApiResponse
        {
            Success = false,
            StatusCode = StatusCodes.Status409Conflict,
            Message = message,
            Errors = errors,
            TraceId = traceId ?? Guid.NewGuid().ToString()
        };
    }

    /// <summary>
    ///     Creates an unprocessable entity error response (422)
    /// </summary>
    public static ApiResponse UnprocessableEntity(string message = "Validation failed", List<string>? errors = null,
        string? traceId = null)
    {
        return new ApiResponse
        {
            Success = false,
            StatusCode = StatusCodes.Status422UnprocessableEntity,
            Message = message,
            Errors = errors,
            TraceId = traceId ?? Guid.NewGuid().ToString()
        };
    }

    /// <summary>
    ///     Creates an internal server error response (500)
    /// </summary>
    public static ApiResponse InternalServerError(string message = "An internal server error occurred",
        string? traceId = null)
    {
        return new ApiResponse
        {
            Success = false,
            StatusCode = StatusCodes.Status500InternalServerError,
            Message = message,
            TraceId = traceId ?? Guid.NewGuid().ToString()
        };
    }

    /// <summary>
    ///     Creates a custom error response
    /// </summary>
    public static ApiResponse Error(int statusCode, string message, List<string>? errors = null, string? traceId = null)
    {
        return new ApiResponse
        {
            Success = false,
            StatusCode = statusCode,
            Message = message,
            Errors = errors,
            TraceId = traceId ?? Guid.NewGuid().ToString()
        };
    }

    /// <summary>
    ///     Creates a paginated response
    /// </summary>
    public static PagedApiResponse<T> Paginated<T>(
        List<T> data,
        int page,
        int pageSize,
        int totalCount,
        string message = "Data retrieved successfully",
        string? traceId = null)
    {
        var response = new PagedApiResponse<T>(data, page, pageSize, totalCount)
        {
            Message = message,
            TraceId = traceId ?? Guid.NewGuid().ToString()
        };

        return response;
    }

    /// <summary>
    ///     Converts an exception to an appropriate API response
    /// </summary>
    public static ApiResponse FromException(Exception exception, string? traceId = null)
    {
        return exception switch
        {
            ArgumentException => BadRequest(exception.Message, null, traceId),
            UnauthorizedAccessException => Unauthorized(exception.Message, traceId),
            KeyNotFoundException => NotFound(exception.Message, traceId),
            InvalidOperationException when exception.Message.Contains("already") => Conflict(exception.Message, null,
                traceId),
            InvalidOperationException => BadRequest(exception.Message, null, traceId),
            _ => InternalServerError("An unexpected error occurred", traceId)
        };
    }
}