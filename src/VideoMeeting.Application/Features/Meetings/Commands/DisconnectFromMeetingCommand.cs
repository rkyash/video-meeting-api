using MediatR;

namespace VideoMeeting.Application.Features.Meetings.Commands;

public record DisconnectFromMeetingCommand(
    string RoomCode,
    int UserId
) : IRequest<bool>;