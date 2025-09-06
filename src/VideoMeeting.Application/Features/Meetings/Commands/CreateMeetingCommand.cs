using MediatR;
using VideoMeeting.Application.Features.Meetings.DTOs;

namespace VideoMeeting.Application.Features.Meetings.Commands;

public record CreateMeetingCommand(
    string Title,
    string? Description,
    DateTime ScheduledAt,
    bool IsScreenSharingEnabled = true,
    int MaxParticipants = 15,
    long CreatedById = 0,
    string? RoomCode = null
) : IRequest<MeetingResponseDto>;