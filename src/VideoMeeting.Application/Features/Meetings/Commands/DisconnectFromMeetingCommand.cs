using MediatR;

namespace VideoMeeting.Application.Features.Meetings.Commands;

public record DisconnectFromMeetingCommand(
    string RoomCode,
    long UserId
) : IRequest<bool>;