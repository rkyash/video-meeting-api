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
            .FirstOrDefaultAsync(m => m.RoomCode == request.RoomCode, cancellationToken);

        if (meeting == null)
            throw new KeyNotFoundException("Meeting not found");

        if (meeting.Status == MeetingStatus.Ended)
            throw new InvalidOperationException("Meeting has ended");

        // Check for existing guest participant by name and email
        var existingParticipant = await _context.MeetingParticipants
            .FirstOrDefaultAsync(
                p => p.MeetingId == meeting.Id && 
                     p.UserId == null && 
                     p.GuestName == request.GuestName && 
                     p.GuestEmail == request.GuestEmail,
                cancellationToken);

        // If guest is already active, return existing record
        if (existingParticipant != null && existingParticipant.LeftAt == null)
            return new ParticipantDto(
                existingParticipant.Id,
                existingParticipant.MeetingId,
                existingParticipant.UserId,
                null,
                existingParticipant.GuestName,
                existingParticipant.GuestEmail,
                existingParticipant.JoinedAt,
                existingParticipant.LeftAt,
                existingParticipant.Role,
                existingParticipant.IsMuted,
                existingParticipant.IsVideoEnabled,
                existingParticipant.IsScreenSharing,
                null, // SessionId - guests don't get session info
                null  // Token - guests don't get tokens
            );

        if (meeting.Participants.Count(p => p.LeftAt == null) >= meeting.MaxParticipants)
            throw new InvalidOperationException("Meeting is at maximum capacity");

        MeetingParticipant participant;

        if (existingParticipant != null)
        {
            // Reactivate existing guest participant
            existingParticipant.LeftAt = null;
            existingParticipant.JoinedAt = DateTime.UtcNow;
            existingParticipant.JoinCount++;
            existingParticipant.IsMuted = false;
            existingParticipant.IsVideoEnabled = true;
            existingParticipant.IsScreenSharing = false;

            _context.MeetingParticipants.Update(existingParticipant);
            participant = existingParticipant;
        }
        else
        {
            // Create new guest participant
            participant = new MeetingParticipant
            {
                MeetingId = meeting.Id,
                UserId = null,
                GuestName = request.GuestName,
                GuestEmail = request.GuestEmail,
                Role = ParticipantRole.Guest,
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
            null,
            participant.GuestName,
            participant.GuestEmail,
            participant.JoinedAt,
            participant.LeftAt,
            participant.Role,
            participant.IsMuted,
            participant.IsVideoEnabled,
            participant.IsScreenSharing,
            null, // SessionId - guests don't get session info
            null  // Token - guests don't get tokens
        );
    }
}