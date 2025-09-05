using System.Diagnostics;
using System.Text.Json;
using VideoMeeting.Application.Common.Models;

namespace VideoMeeting.Api.Middleware;

public class ResponseMiddleware
{
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly ILogger<ResponseMiddleware> _logger;
    private readonly RequestDelegate _next;

    public ResponseMiddleware(RequestDelegate next, ILogger<ResponseMiddleware> logger)
    {
        _next = next;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var originalBodyStream = context.Response.Body;

        try
        {
            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            await _next(context);

            context.Response.Body.Seek(0, SeekOrigin.Begin);
            var response = await new StreamReader(context.Response.Body).ReadToEndAsync();
            context.Response.Body.Seek(0, SeekOrigin.Begin);

            if (ShouldWrapResponse(context))
            {
                var wrappedResponse = WrapResponse(response, context);
                var wrappedJson = JsonSerializer.Serialize(wrappedResponse, _jsonOptions);

                context.Response.ContentType = "application/json";
                context.Response.ContentLength = null;
                context.Response.Body = originalBodyStream;

                await context.Response.WriteAsync(wrappedJson);
            }
            else
            {
                context.Response.Body = originalBodyStream;
                await responseBody.CopyToAsync(originalBodyStream);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred in ResponseMiddleware");
            context.Response.Body = originalBodyStream;
            await HandleExceptionAsync(context, ex);
        }
    }

    private static bool ShouldWrapResponse(HttpContext context)
    {
        return false; // Disable middleware wrapping since we're using extension methods
    }

    private static bool IsAlreadyWrapped(HttpContext context)
    {
        return context.Response.Headers.ContainsKey("X-Response-Wrapped");
    }

    private static ApiResponse<object> WrapResponse(string response, HttpContext context)
    {
        var traceId = Activity.Current?.Id ?? context.TraceIdentifier;
        var statusCode = context.Response.StatusCode;
        var isSuccess = statusCode >= 200 && statusCode < 300;

        object? data = null;
        if (!string.IsNullOrWhiteSpace(response))
            try
            {
                data = JsonSerializer.Deserialize<object>(response);
            }
            catch
            {
                data = response;
            }

        var message = GetMessageForStatusCode(statusCode, isSuccess);

        return new ApiResponse<object>
        {
            Success = isSuccess,
            StatusCode = statusCode,
            Message = message,
            Data = data,
            TraceId = traceId,
            Timestamp = DateTime.UtcNow
        };
    }

    private static string GetMessageForStatusCode(int statusCode, bool isSuccess)
    {
        if (isSuccess)
            return statusCode switch
            {
                200 => "Operation completed successfully",
                201 => "Resource created successfully",
                204 => "Operation completed successfully",
                _ => "Request processed successfully"
            };

        return statusCode switch
        {
            400 => "Bad request",
            401 => "Unauthorized access",
            403 => "Access forbidden",
            404 => "Resource not found",
            409 => "Resource conflict",
            422 => "Validation failed",
            500 => "An internal server error occurred",
            _ => "An error occurred"
        };
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var traceId = Activity.Current?.Id ?? context.TraceIdentifier;

        var response = ApiResponseHelper.FromException(exception, traceId);
        var jsonResponse = JsonSerializer.Serialize(response, _jsonOptions);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = response.StatusCode;

        await context.Response.WriteAsync(jsonResponse);
    }
}