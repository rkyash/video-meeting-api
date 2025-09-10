using MediatR;
using VideoMeeting.Application.Features.Meetings.DTOs;

namespace VideoMeeting.Application.Features.Meetings.Commands;

public record StartRecordingCommand(
    string RoomCode,
    long UserId,
    string? SessionId = null
) : IRequest<RecordingDto>;