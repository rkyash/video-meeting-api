using MediatR;
using Microsoft.EntityFrameworkCore;
using VideoMeeting.Application.Common.Interfaces;
using VideoMeeting.Application.Features.Meetings.Commands;
using VideoMeeting.Domain.Enums;

namespace VideoMeeting.Application.Features.Meetings.Handlers;

public class DisconnectFromMeetingHandler : IRequestHandler<DisconnectFromMeetingCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public DisconnectFromMeetingHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DisconnectFromMeetingCommand request, CancellationToken cancellationToken)
    {
        var meeting = await _context.Meetings
            .Include(m => m.Participants)
            .FirstOrDefaultAsync(m => m.RoomCode == request.RoomCode, cancellationToken);

        if (meeting == null)
            throw new KeyNotFoundException("Meeting not found");

        var participant = await _context.MeetingParticipants
            .FirstOrDefaultAsync(
                p => p.MeetingId == meeting.Id && p.UserId == request.UserId && p.LeftAt == null,
                cancellationToken);

        if (participant == null)
            throw new KeyNotFoundException("Participant not found or already disconnected");

        participant.LeftAt = DateTime.UtcNow;
        _context.MeetingParticipants.Update(participant);

        // Check if this was a host (Assessor) participant
        var wasHost = participant.Role == ParticipantRole.Host;
        
        // Check if all participants have left and end the meeting if so
        var activeParticipants = meeting.Participants.Count(p => p.LeftAt == null && p.Id != participant.Id);
        
        // If this was a host, check if there are any remaining hosts
        if (wasHost)
        {
            var remainingHosts = meeting.Participants.Count(p => 
                p.LeftAt == null && 
                p.Id != participant.Id && 
                p.Role == ParticipantRole.Host);
                
            // If no hosts remain, blank the sessionId
            if (remainingHosts == 0)
            {
                meeting.SessionId = string.Empty;
                _context.Meetings.Update(meeting);
            }
        }
        
        // End meeting if all participants have left
        if (activeParticipants == 0 && meeting.Status == MeetingStatus.Active)
        {
            meeting.Status = MeetingStatus.Ended;
            meeting.EndedAt = DateTime.UtcNow;
            _context.Meetings.Update(meeting);
        }

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}