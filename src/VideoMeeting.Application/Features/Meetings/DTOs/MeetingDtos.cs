using VideoMeeting.Domain.Enums;

namespace VideoMeeting.Application.Features.Meetings.DTOs;

public record MeetingResponseDto(
    long Id,
    string Title,
    string? Description,
    string SessionId,
    string RoomCode,
    DateTime ScheduledAt,
    DateTime? StartedAt,
    DateTime? EndedAt,
    DateTime CreatedAt,
    long? CreatedById,
    string CreatedByName,
    bool IsRecordingEnabled,
    bool IsScreenSharingEnabled,
    int MaxParticipants,
    MeetingStatus Status,
    int ParticipantCount
);

public record MeetingListDto(
    long Id,
    string Title,
    string? Description,
    DateTime ScheduledAt,
    DateTime? StartedAt,
    DateTime? EndedAt,
    string CreatedByName,
    MeetingStatus Status,
    int ParticipantCount,
    bool IsRecordingEnabled
);

public record ParticipantDto(
    long Id,
    long MeetingId,
    long? UserId,
    string? UserName,
    string? GuestName,
    string? GuestEmail,
    DateTime JoinedAt,
    DateTime? LeftAt,
    ParticipantRole Role,
    bool IsMuted,
    bool IsVideoEnabled,
    bool IsScreenSharing,
    string? SessionId = null,
    string? Token = null,
    string? ApiKey = null
);

public record RecordingDto(
    long Id,
    long MeetingId,
    string SessionId,
    string RecordingId,
    string FileName,
    string? FileUrl,
    long FileSizeBytes,
    int DurationSeconds,
    DateTime StartedAt,
    DateTime? CompletedAt,
    RecordingStatus Status
);

public record RecordingInfoDto(
    string Id,
    RecordingStatus Status,
    int Duration,
    long Size,
    string? Url,
    DateTime CreatedAt,
    DateTime UpdatedAt
);