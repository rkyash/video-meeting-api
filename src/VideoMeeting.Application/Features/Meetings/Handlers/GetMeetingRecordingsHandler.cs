using MediatR;
using Microsoft.EntityFrameworkCore;
using VideoMeeting.Application.Common.Interfaces;
using VideoMeeting.Application.Features.Meetings.DTOs;
using VideoMeeting.Application.Features.Meetings.Queries;

namespace VideoMeeting.Application.Features.Meetings.Handlers;

public class GetMeetingRecordingsHandler : IRequestHandler<GetMeetingRecordingsQuery, List<RecordingDto>>
{
    private readonly IApplicationDbContext _context;

    public GetMeetingRecordingsHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<RecordingDto>> Handle(GetMeetingRecordingsQuery request, CancellationToken cancellationToken)
    {
        var meeting = await _context.Meetings
            .FirstOrDefaultAsync(m => m.Id == request.MeetingId, cancellationToken);

        if (meeting == null)
            throw new KeyNotFoundException("Meeting not found");

        var recordings = await _context.MeetingRecordings
            .Where(r => r.MeetingId == request.MeetingId)
            .OrderByDescending(r => r.StartedAt)
            .Select(r => new RecordingDto(
                r.Id,
                r.MeetingId,
                r.SessionId,
                r.RecordingId,
                r.FileName,
                r.FileUrl,
                r.FileSizeBytes,
                r.DurationSeconds,
                r.StartedAt,
                r.CompletedAt,
                r.Status
            ))
            .ToListAsync(cancellationToken);

        return recordings;
    }
}