using MediatR;
using Microsoft.EntityFrameworkCore;
using VideoMeeting.Application.Common.Interfaces;
using VideoMeeting.Application.Features.Meetings.Commands;
using VideoMeeting.Application.Features.Meetings.DTOs;
using VideoMeeting.Domain.Entities;
using VideoMeeting.Domain.Enums;

namespace VideoMeeting.Application.Features.Meetings.Handlers;

public class JoinMeetingAsGuestHandler : IRequestHandler<JoinMeetingAsGuestCommand, ParticipantDto>
{
    private readonly IApplicationDbContext _context;

    public JoinMeetingAsGuestHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ParticipantDto> Handle(JoinMeetingAsGuestCommand request, CancellationToken cancellationToken)
    {
        var meeting = await _context.Meetings
            .Include(m => m.Participants)
            .FirstOrDefaultAsync(m => m.Id == request.MeetingId, cancellationToken);

        if (meeting == null)
            throw new KeyNotFoundException("Meeting not found");

        if (meeting.Status == MeetingStatus.Ended)
            throw new InvalidOperationException("Meeting has ended");

        if (meeting.Participants.Count(p => p.LeftAt == null) >= meeting.MaxParticipants)
            throw new InvalidOperationException("Meeting is at maximum capacity");

        var participant = new MeetingParticipant
        {
            MeetingId = request.MeetingId,
            UserId = null,
            GuestName = request.GuestName,
            GuestEmail = request.GuestEmail,
            Role = ParticipantRole.Guest,
            JoinedAt = DateTime.UtcNow,
            IsMuted = false,
            IsVideoEnabled = true,
            IsScreenSharing = false
        };

        _context.MeetingParticipants.Add(participant);

        if (meeting.Status == MeetingStatus.Scheduled)
        {
            meeting.Status = MeetingStatus.Active;
            meeting.StartedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return new ParticipantDto(
            participant.Id,
            participant.MeetingId,
            participant.UserId,
            null,
            participant.GuestName,
            participant.GuestEmail,
            participant.JoinedAt,
            participant.LeftAt,
            participant.Role,
            participant.IsMuted,
            participant.IsVideoEnabled,
            participant.IsScreenSharing
        );
    }
}