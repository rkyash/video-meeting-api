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
            .FirstOrDefaultAsync(m => m.RoomCode == request.RoomCode, cancellationToken);

        if (meeting == null)
            throw new KeyNotFoundException("Meeting not found");

        if (meeting.Status == MeetingStatus.Ended)
            throw new InvalidOperationException("Meeting has ended");

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user == null)
            throw new KeyNotFoundException("User not found");

        // Check for existing participant (active or inactive)
        var existingParticipant = await _context.MeetingParticipants
            .FirstOrDefaultAsync(
                p => p.MeetingId == meeting.Id && p.UserId == request.UserId,
                cancellationToken);

        // If participant already active, return existing record
        if (existingParticipant != null && existingParticipant.LeftAt == null)
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

        MeetingParticipant participant;

        if (existingParticipant != null)
        {
            // Reactivate existing participant
            existingParticipant.LeftAt = null;
            existingParticipant.JoinedAt = DateTime.UtcNow;
            existingParticipant.JoinCount++;
            existingParticipant.IsMuted = false;
            existingParticipant.IsVideoEnabled = true;
            existingParticipant.IsScreenSharing = false;
            existingParticipant.Role = participantRole;

            _context.MeetingParticipants.Update(existingParticipant);
            participant = existingParticipant;
        }
        else
        {
            // Create new participant
            participant = new MeetingParticipant
            {
                MeetingId = meeting.Id,
                UserId = request.UserId,
                Role = participantRole,
                JoinedAt = DateTime.UtcNow,
                IsMuted = false,
                IsVideoEnabled = true,
                IsScreenSharing = false,
                JoinCount = 1
            };

            _context.MeetingParticipants.Add(participant);
        }

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