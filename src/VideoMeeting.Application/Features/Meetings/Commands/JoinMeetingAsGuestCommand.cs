using MediatR;
using VideoMeeting.Application.Features.Meetings.DTOs;

namespace VideoMeeting.Application.Features.Meetings.Commands;

public record JoinMeetingAsGuestCommand(
    int MeetingId,
    string GuestName,
    string? GuestEmail = null
) : IRequest<ParticipantDto>;