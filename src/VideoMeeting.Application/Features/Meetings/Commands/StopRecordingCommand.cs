using MediatR;
using VideoMeeting.Application.Features.Meetings.DTOs;

namespace VideoMeeting.Application.Features.Meetings.Commands;

public record StopRecordingCommand(
    int MeetingId,
    int RecordingId,
    int UserId
) : IRequest<RecordingDto>;