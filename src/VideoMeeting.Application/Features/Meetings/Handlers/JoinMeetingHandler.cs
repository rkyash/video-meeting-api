using MediatR;
using Microsoft.EntityFrameworkCore;
using VideoMeeting.Application.Common.Interfaces;
using VideoMeeting.Application.Features.Meetings.Commands;
using VideoMeeting.Application.Features.Meetings.DTOs;
using VideoMeeting.Domain.Entities;
using VideoMeeting.Domain.Enums;

namespace VideoMeeting.Application.Features.Meetings.Handlers;

public class JoinMeetingHandler : IRequestHandler<JoinMeetingCommand, ParticipantDto>
{
    private readonly IApplicationDbContext _context;

    public JoinMeetingHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ParticipantDto> Handle(JoinMeetingCommand request, CancellationToken cancellationToken)
    {
        var meeting = await _context.Meetings
            .Include(m => m.Participants)
            .FirstOrDefaultAsync(m => m.Id == request.MeetingId, cancellationToken);

        if (meeting == null)
            throw new KeyNotFoundException("Meeting not found");

        if (meeting.Status == MeetingStatus.Ended)
            throw new InvalidOperationException("Meeting has ended");

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user == null)
            throw new KeyNotFoundException("User not found");

        var existingParticipant = await _context.MeetingParticipants
            .FirstOrDefaultAsync(
                p => p.MeetingId == request.MeetingId && p.UserId == request.UserId && p.LeftAt == null,
                cancellationToken);

        if (existingParticipant != null)
            return new ParticipantDto(
                existingParticipant.Id,
                existingParticipant.MeetingId,
                existingParticipant.UserId,
                $"{user.FirstName} {user.LastName}",
                null,
                null,
                existingParticipant.JoinedAt,
                existingParticipant.LeftAt,
                existingParticipant.Role,
                existingParticipant.IsMuted,
                existingParticipant.IsVideoEnabled,
                existingParticipant.IsScreenSharing
            );

        if (meeting.Participants.Count(p => p.LeftAt == null) >= meeting.MaxParticipants)
            throw new InvalidOperationException("Meeting is at maximum capacity");

        var participantRole =
            meeting.CreatedById == request.UserId ? ParticipantRole.Host : ParticipantRole.Participant;

        var participant = new MeetingParticipant
        {
            MeetingId = request.MeetingId,
            UserId = request.UserId,
            Role = participantRole,
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
            $"{user.FirstName} {user.LastName}",
            null,
            null,
            participant.JoinedAt,
            participant.LeftAt,
            participant.Role,
            participant.IsMuted,
            participant.IsVideoEnabled,
            participant.IsScreenSharing
        );
    }
}