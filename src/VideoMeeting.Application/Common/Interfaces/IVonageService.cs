using VideoMeeting.Application.Common.Models;
using VideoMeeting.Application.Features.Meetings.DTOs;
using Vonage.Video.Archives;

namespace VideoMeeting.Application.Common.Interfaces;

public interface IVonageService
{
    Task<ApiResponse<string>>  CreateSession();
    ApiResponse<string> GenerateToken(string sessionId, string role = "publisher");

    Task<ApiResponse<Archive>> StartRecordingAsync(string SessionId,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<object>> StopRecordingAsync(string archiveId, CancellationToken cancellationToken = default);
    Task<ApiResponse<object>> SignalHostDisconnection(string sessionId, CancellationToken cancellationToken = default);
    // Task<ApiResponse<RecordingInfoDto>> GetRecordingInfoAsync(string recordingId, CancellationToken cancellationToken = default);
}