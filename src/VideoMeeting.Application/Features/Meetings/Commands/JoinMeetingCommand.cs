using MediatR;
using VideoMeeting.Application.Features.Meetings.DTOs;

namespace VideoMeeting.Application.Features.Meetings.Commands;

public record JoinMeetingCommand(
    int MeetingId,
    int UserId
) : IRequest<ParticipantDto>;