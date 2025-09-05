using MediatR;
using Microsoft.EntityFrameworkCore;
using VideoMeeting.Application.Common.Interfaces;
using VideoMeeting.Application.Features.Meetings.Commands;
using VideoMeeting.Application.Features.Meetings.DTOs;
using VideoMeeting.Domain.Entities;
using VideoMeeting.Domain.Enums;
using VideoMeeting.Domain.ValueObjects;

namespace VideoMeeting.Application.Features.Meetings.Handlers;

public class CreateMeetingCommandHandler : IRequestHandler<CreateMeetingCommand, MeetingResponseDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IVonageService _vonageService;

    public CreateMeetingCommandHandler(IApplicationDbContext context, IVonageService vonageService)
    {
        _context = context;
        _vonageService = vonageService;
    }

    public async Task<MeetingResponseDto> Handle(CreateMeetingCommand request, CancellationToken cancellationToken)
    {
        // var sessionId = await _vonageService.CreateSession();
        // var token = _vonageService.GenerateToken(sessionId.Data.ToString());
        var roomCode = RoomCode.Create();

        var meeting = new Meeting
        {
            Title = request.Title,
            Description = request.Description,
            RoomCode = roomCode.Value,
            ScheduledAt = request.ScheduledAt,
            CreatedById = request.CreatedById,
            IsRecordingEnabled = true,
            IsScreenSharingEnabled = request.IsScreenSharingEnabled,
            MaxParticipants = request.MaxParticipants,
            Status = MeetingStatus.Scheduled,
            CreatedAt = DateTime.UtcNow
        };

        _context.Meetings.Add(meeting);
        await _context.SaveChangesAsync(cancellationToken);

        // Load the created by user for the response
        await _context.Meetings
            .Where(m => m.Id == meeting.Id)
            .Include(m => m.CreatedBy)
            .LoadAsync(cancellationToken);

        return new MeetingResponseDto(
            meeting.Id,
            meeting.Title,
            meeting.Description,
            meeting.SessionId,
            meeting.Token,
            meeting.RoomCode,
            meeting.ScheduledAt,
            meeting.StartedAt,
            meeting.EndedAt,
            meeting.CreatedAt,
            meeting.CreatedById,
            meeting.CreatedBy?.FullName ?? "Unknown User",
            meeting.IsRecordingEnabled,
            meeting.IsScreenSharingEnabled,
            meeting.MaxParticipants,
            meeting.Status,
            meeting.ActiveParticipantCount
        );
    }
}