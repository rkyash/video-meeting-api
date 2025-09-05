using MediatR;
using VideoMeeting.Application.Features.Meetings.DTOs;

namespace VideoMeeting.Application.Features.Meetings.Commands;

public record StartRecordingCommand(
    int MeetingId,
    int UserId,
    string? RecordingName = null
) : IRequest<RecordingDto>;