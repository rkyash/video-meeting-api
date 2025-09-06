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

        // Check if all participants have left and end the meeting if so
        var activeParticipants = meeting.Participants.Count(p => p.LeftAt == null && p.Id != participant.Id);
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