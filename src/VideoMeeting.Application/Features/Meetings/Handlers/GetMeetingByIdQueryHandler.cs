using MediatR;
using Microsoft.EntityFrameworkCore;
using VideoMeeting.Application.Common.Interfaces;
using VideoMeeting.Application.Features.Meetings.DTOs;
using VideoMeeting.Application.Features.Meetings.Queries;

namespace VideoMeeting.Application.Features.Meetings.Handlers;

public class GetMeetingByIdQueryHandler : IRequestHandler<GetMeetingByIdQuery, MeetingResponseDto?>
{
    private readonly IApplicationDbContext _context;

    public GetMeetingByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<MeetingResponseDto?> Handle(GetMeetingByIdQuery request, CancellationToken cancellationToken)
    {
        var meeting = await _context.Meetings
            .Include(m => m.CreatedBy)
            .Include(m => m.Participants)
            .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);

        if (meeting == null)
            return null;

        return new MeetingResponseDto(
            meeting.Id,
            meeting.Title,
            meeting.Description,
            meeting.SessionId,
            meeting.RoomCode,
            meeting.ScheduledAt,
            meeting.StartedAt,
            meeting.EndedAt,
            meeting.CreatedAt,
            meeting.CreatedById,
             "Unknown User",
            meeting.IsRecordingEnabled,
            meeting.IsScreenSharingEnabled,
            meeting.MaxParticipants,
            meeting.Status,
            meeting.ActiveParticipantCount
        );
    }
}

public class GetMeetingByRoomCodeQueryHandler : IRequestHandler<GetMeetingByRoomCodeQuery, MeetingResponseDto?>
{
    private readonly IApplicationDbContext _context;

    public GetMeetingByRoomCodeQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<MeetingResponseDto?> Handle(GetMeetingByRoomCodeQuery request,
        CancellationToken cancellationToken)
    {
        var meeting = await _context.Meetings
            .Include(m => m.CreatedBy)
            .Include(m => m.Participants)
            .FirstOrDefaultAsync(m => m.RoomCode == request.RoomCode, cancellationToken);

        if (meeting == null)
            return null;

        return new MeetingResponseDto(
            meeting.Id,
            meeting.Title,
            meeting.Description,
            meeting.SessionId,
            meeting.RoomCode,
            meeting.ScheduledAt,
            meeting.StartedAt,
            meeting.EndedAt,
            meeting.CreatedAt,
            meeting.CreatedById,
            "" ?? "Unknown User",
            meeting.IsRecordingEnabled,
            meeting.IsScreenSharingEnabled,
            meeting.MaxParticipants,
            meeting.Status,
            meeting.ActiveParticipantCount
        );
    }
}

public class GetUserMeetingsQueryHandler : IRequestHandler<GetUserMeetingsQuery, List<MeetingListDto>>
{
    private readonly IApplicationDbContext _context;

    public GetUserMeetingsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<MeetingListDto>> Handle(GetUserMeetingsQuery request, CancellationToken cancellationToken)
    {
        var meetings = await _context.Meetings
            .Include(m => m.CreatedBy)
            .Include(m => m.Participants)
            .Where(m => m.CreatedById == request.UserId)
            .OrderByDescending(m => m.CreatedAt)
            .Select(m => new MeetingListDto(
                m.Id,
                m.Title,
                m.Description,
                m.ScheduledAt,
                m.StartedAt,
                m.EndedAt,
                "Unknown User",
                m.Status,
                m.Participants.Count(p => p.LeftAt == null),
                m.IsRecordingEnabled
            ))
            .ToListAsync(cancellationToken);

        return meetings;
    }
}

public class GetMeetingParticipantsQueryHandler : IRequestHandler<GetMeetingParticipantsQuery, List<ParticipantDto>>
{
    private readonly IApplicationDbContext _context;

    public GetMeetingParticipantsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ParticipantDto>> Handle(GetMeetingParticipantsQuery request,
        CancellationToken cancellationToken)
    {
        var participants = await _context.MeetingParticipants
            .Where(p => p.MeetingId == request.MeetingId && p.LeftAt == null)
            .Select(p => new ParticipantDto(
                p.Id,
                p.MeetingId,
                p.UserId,
                 null,
                p.GuestName,
                p.GuestEmail,
                p.JoinedAt,
                p.LeftAt,
                p.Role,
                p.IsMuted,
                p.IsVideoEnabled,
                p.IsScreenSharing,
                null, // SessionId - not provided in this context
                null,  // Token - not provided in this context
                ""
            ))
            .ToListAsync(cancellationToken);

        return participants;
    }
}