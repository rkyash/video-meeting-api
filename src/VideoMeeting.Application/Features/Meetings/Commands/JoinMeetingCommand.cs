using MediatR;
using VideoMeeting.Application.Features.Meetings.DTOs;

namespace VideoMeeting.Application.Features.Meetings.Commands;

public record JoinMeetingCommand(
    string RoomCode,
    long UserId,
    string UserRole,
    string? UserName = null
) : IRequest<ParticipantDto>;