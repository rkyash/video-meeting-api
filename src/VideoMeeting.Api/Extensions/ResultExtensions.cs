using System.Diagnostics;
using VideoMeeting.Application.Common.Models;

namespace VideoMeeting.Api.Extensions;

/// <summary>
///     Extension methods for creating standardized API responses in Minimal APIs
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    ///     Creates a standardized success response
    /// </summary>
    public static IResult ToApiResponse<T>(this T data, string message = "Operation completed successfully")
    {
        var response = ApiResponseHelper.Success(data, message, GetTraceId());
        return Results.Json(response, statusCode: response.StatusCode);
    }

    /// <summary>
    ///     Creates a standardized created response
    /// </summary>
    public static IResult ToCreatedResponse<T>(this T data, string message = "Resource created successfully")
    {
        var response = ApiResponseHelper.Created(data, message, GetTraceId());
        return Results.Json(response, statusCode: response.StatusCode);
    }

    /// <summary>
    ///     Creates a standardized no content response
    /// </summary>
    public static IResult ToNoContentResponse(string message = "Operation completed successfully")
    {
        var response = ApiResponseHelper.NoContent(message, GetTraceId());
        return Results.Json(response, statusCode: response.StatusCode);
    }

    /// <summary>
    ///     Creates a standardized bad request response
    /// </summary>
    public static IResult ToBadRequestResponse(string message = "Bad request", List<string>? errors = null)
    {
        var response = ApiResponseHelper.BadRequest(message, errors, GetTraceId());
        return Results.Json(response, statusCode: response.StatusCode);
    }

    /// <summary>
    ///     Creates a standardized unauthorized response
    /// </summary>
    public static IResult ToUnauthorizedResponse(string message = "Unauthorized access")
    {
        var response = ApiResponseHelper.Unauthorized(message, GetTraceId());
        return Results.Json(response, statusCode: response.StatusCode);
    }

    /// <summary>
    ///     Creates a standardized forbidden response
    /// </summary>
    public static IResult ToForbiddenResponse(string message = "Access forbidden")
    {
        var response = ApiResponseHelper.Forbidden(message, GetTraceId());
        return Results.Json(response, statusCode: response.StatusCode);
    }

    /// <summary>
    ///     Creates a standardized not found response
    /// </summary>
    public static IResult ToNotFoundResponse(string message = "Resource not found")
    {
        var response = ApiResponseHelper.NotFound(message, GetTraceId());
        return Results.Json(response, statusCode: response.StatusCode);
    }

    /// <summary>
    ///     Creates a standardized conflict response
    /// </summary>
    public static IResult ToConflictResponse(string message = "Resource conflict", List<string>? errors = null)
    {
        var response = ApiResponseHelper.Conflict(message, errors, GetTraceId());
        return Results.Json(response, statusCode: response.StatusCode);
    }

    /// <summary>
    ///     Creates a standardized validation error response
    /// </summary>
    public static IResult ToValidationErrorResponse(string message = "Validation failed", List<string>? errors = null)
    {
        var response = ApiResponseHelper.UnprocessableEntity(message, errors, GetTraceId());
        return Results.Json(response, statusCode: response.StatusCode);
    }

    /// <summary>
    ///     Creates a standardized internal server error response
    /// </summary>
    public static IResult ToInternalServerErrorResponse(string message = "An internal server error occurred")
    {
        var response = ApiResponseHelper.InternalServerError(message, GetTraceId());
        return Results.Json(response, statusCode: response.StatusCode);
    }

    /// <summary>
    ///     Creates a standardized paginated response
    /// </summary>
    public static IResult ToPagedResponse<T>(this List<T> data, int page, int pageSize, int totalCount,
        string message = "Data retrieved successfully")
    {
        var response = ApiResponseHelper.Paginated(data, page, pageSize, totalCount, message, GetTraceId());
        return Results.Json(response, statusCode: response.StatusCode);
    }

    /// <summary>
    ///     Converts an exception to a standardized API response
    /// </summary>
    public static IResult ToApiResponse(this Exception exception)
    {
        var response = ApiResponseHelper.FromException(exception, GetTraceId());
        return Results.Json(response, statusCode: response.StatusCode);
    }

    /// <summary>
    ///     Creates a custom standardized response
    /// </summary>
    public static IResult ToCustomResponse(int statusCode, bool success, string message, object? data = null,
        List<string>? errors = null)
    {
        var response = new ApiResponse<object>
        {
            Success = success,
            StatusCode = statusCode,
            Message = message,
            Data = data,
            Errors = errors,
            TraceId = GetTraceId()
        };

        return Results.Json(response, statusCode: statusCode);
    }

    /// <summary>
    ///     Gets or generates a trace ID for request tracking
    /// </summary>
    private static string GetTraceId()
    {
        return Activity.Current?.Id ?? Guid.NewGuid().ToString();
    }
}