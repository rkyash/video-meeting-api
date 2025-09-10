using MediatR;
using Microsoft.EntityFrameworkCore;
using VideoMeeting.Application.Common.Interfaces;
using VideoMeeting.Application.Features.Meetings.Commands;
using VideoMeeting.Application.Features.Meetings.DTOs;
using VideoMeeting.Domain.Entities;
using VideoMeeting.Domain.Enums;

namespace VideoMeeting.Application.Features.Meetings.Handlers;

public class StartRecordingHandler : IRequestHandler<StartRecordingCommand, RecordingDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IVonageService _vonageService;

    public StartRecordingHandler(IApplicationDbContext context, IVonageService vonageService)
    {
        _context = context;
        _vonageService = vonageService;
    }

    public async Task<RecordingDto> Handle(StartRecordingCommand request, CancellationToken cancellationToken)
    {
        var meeting = await _context.Meetings
            .Include(m => m.Participants)
            .FirstOrDefaultAsync(m => m.RoomCode == request.RoomCode, cancellationToken);

        if (meeting == null)
            throw new KeyNotFoundException("Meeting not found");

        if (meeting.Status != MeetingStatus.Active)
            throw new InvalidOperationException("Meeting must be active to start recording");

        if (!meeting.IsRecordingEnabled)
            throw new InvalidOperationException("Recording is not enabled for this meeting");

        var participant = await _context.MeetingParticipants
            .FirstOrDefaultAsync(
                p => p.MeetingId == meeting.Id && p.UserId == request.UserId && p.Role == ParticipantRole.Host,
                cancellationToken);

        if (participant == null)
            throw new UnauthorizedAccessException("Only meeting host can start recording");

        // var existingRecording = await _context.MeetingRecordings
        //     .FirstOrDefaultAsync(
        //         r => r.MeetingId == request.MeetingId && r.SessionId == meeting.SessionId &&
        //              r.Status == RecordingStatus.Recording,
        //         cancellationToken);
        //
        // if (existingRecording != null)
        //     throw new InvalidOperationException("Recording is already in progress for this session");

        // var fileName = request.RecordingName ?? $"Meeting_{meeting.Id}_{DateTime.UtcNow:yyyyMMdd_HHmmss}";

        var recordingResp = await _vonageService.StartRecordingAsync(meeting.SessionId, cancellationToken);

        if (!recordingResp.Success)
        {
            throw new InvalidOperationException(recordingResp.Message);
        }
        
        var recording = new MeetingRecording
        {
            MeetingId = meeting.Id,
            SessionId = meeting.SessionId,
            RecordingId = recordingResp.Data.Id,
            FileName = "",
            Status = RecordingStatus.Recording,
            StartedAt = DateTime.UtcNow
        };

        _context.MeetingRecordings.Add(recording);
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