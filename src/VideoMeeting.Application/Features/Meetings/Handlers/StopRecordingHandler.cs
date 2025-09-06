using MediatR;
using Microsoft.EntityFrameworkCore;
using VideoMeeting.Application.Common.Interfaces;
using VideoMeeting.Application.Features.Meetings.Commands;
using VideoMeeting.Application.Features.Meetings.DTOs;
using VideoMeeting.Domain.Enums;

namespace VideoMeeting.Application.Features.Meetings.Handlers;

public class StopRecordingHandler : IRequestHandler<StopRecordingCommand, RecordingDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IVonageService _vonageService;

    public StopRecordingHandler(IApplicationDbContext context, IVonageService vonageService)
    {
        _context = context;
        _vonageService = vonageService;
    }

    public async Task<RecordingDto> Handle(StopRecordingCommand request, CancellationToken cancellationToken)
    {
        var recording = await _context.MeetingRecordings
            .Include(r => r.Meeting)
            .FirstOrDefaultAsync(r => r.Id == request.RecordingId && r.MeetingId == request.MeetingId, cancellationToken);

        if (recording == null)
            throw new KeyNotFoundException("Recording not found");

        if (recording.Status != RecordingStatus.Recording)
            throw new InvalidOperationException("Recording is not currently active");

        var participant = await _context.MeetingParticipants
            .FirstOrDefaultAsync(
                p => p.MeetingId == request.MeetingId && p.UserId == request.UserId && p.Role == ParticipantRole.Host,
                cancellationToken);

        if (participant == null)
            throw new UnauthorizedAccessException("Only meeting host can stop recording");

        await _vonageService.StopRecordingAsync(recording.RecordingId, cancellationToken);

        recording.CompletedAt = DateTime.UtcNow;
        recording.Status = RecordingStatus.Completed;

        _context.MeetingRecordings.Update(recording);
        await _context.SaveChangesAsync(cancellationToken);

        return new RecordingDto(
            recording.Id,
            recording.MeetingId,
            recording.SessionId,
            recording.RecordingId,
            recording.FileName,
            recording.FileUrl,
            recording.FileSizeBytes,
            recording.DurationSeconds,
            recording.StartedAt,
            recording.CompletedAt,
            recording.Status
        );
    }
}