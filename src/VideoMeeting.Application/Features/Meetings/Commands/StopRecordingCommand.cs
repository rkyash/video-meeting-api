using MediatR;
using VideoMeeting.Application.Features.Meetings.DTOs;

namespace VideoMeeting.Application.Features.Meetings.Commands;

public record StopRecordingCommand(
    string RoomCode,
    string RecordingId,
    long UserId
) : IRequest<RecordingDto>;