using System.Text.Json.Serialization;

namespace VideoMeeting.Application.Common.Models;

/// <summary>
///     Standard API response wrapper for all endpoints
/// </summary>
/// <typeparam name="T">The type of data being returned</typeparam>
public class ApiResponse<T>
{
    /// <summary>
    ///     Indicates if the request was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    ///     HTTP status code
    /// </summary>
    public int StatusCode { get; set; }

    /// <summary>
    ///     Human-readable message about the response
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    ///     The actual data payload (null for error responses)
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public T? Data { get; set; }

    /// <summary>
    ///     List of validation errors or detailed error information
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? Errors { get; set; } = null;

    /// <summary>
    ///     Additional metadata about the response
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ResponseMetadata? Metadata { get; set; }

    /// <summary>
    ///     Timestamp when the response was generated
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    ///     Unique identifier for request tracing
    /// </summary>
    public string TraceId { get; set; } = string.Empty;
}

/// <summary>
///     Non-generic version for responses without data
/// </summary>
public class ApiResponse : ApiResponse<object>
{
}

/// <summary>
///     Additional metadata that can be included in responses
/// </summary>
public class ResponseMetadata
{
    /// <summary>
    ///     Current page number (for paginated responses)
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Page { get; set; }

    /// <summary>
    ///     Number of items per page
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? PageSize { get; set; }

    /// <summary>
    ///     Total number of items available
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? TotalCount { get; set; }

    /// <summary>
    ///     Total number of pages
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? TotalPages { get; set; }

    /// <summary>
    ///     Indicates if there are more pages available
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? HasNextPage { get; set; }

    /// <summary>
    ///     Indicates if there are previous pages available
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? HasPreviousPage { get; set; }

    /// <summary>
    ///     Additional custom metadata
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, object>? Additional { get; set; }
}

/// <summary>
///     Standard paginated response wrapper
/// </summary>
/// <typeparam name="T">The type of items in the collection</typeparam>
public class PagedApiResponse<T> : ApiResponse<List<T>>
{
    public PagedApiResponse()
    {
        Metadata = new ResponseMetadata();
    }

    public PagedApiResponse(List<T> data, int page, int pageSize, int totalCount) : this()
    {
        Success = true;
        StatusCode = 200;
        Message = "Data retrieved successfully";
        Data = data;

        Metadata!.Page = page;
        Metadata.PageSize = pageSize;
        Metadata.TotalCount = totalCount;
        Metadata.TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);
        Metadata.HasNextPage = page < Metadata.TotalPages;
        Metadata.HasPreviousPage = page > 1;
    }
}