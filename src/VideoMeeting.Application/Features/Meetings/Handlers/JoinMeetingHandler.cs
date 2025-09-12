using MediatR;
using Microsoft.EntityFrameworkCore;
using VideoMeeting.Application.Common.Interfaces;
using VideoMeeting.Application.Features.Meetings.Commands;
using VideoMeeting.Application.Features.Meetings.DTOs;
using VideoMeeting.Domain.Entities;
using VideoMeeting.Domain.Enums;
using VideoMeeting.Shared;
using VideoMeeting.Shared.Configuration;

namespace VideoMeeting.Application.Features.Meetings.Handlers;

public class JoinMeetingHandler : IRequestHandler<JoinMeetingCommand, ParticipantDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IVonageService _vonageService;
    private readonly VonageConfiguration _vonageConfig;

    public JoinMeetingHandler(IApplicationDbContext context, IVonageService vonageService,VonageConfiguration vonageConfig)
    {
        _context = context;
        _vonageService = vonageService;
        _vonageConfig = vonageConfig;
    }

    public async Task<ParticipantDto> Handle(JoinMeetingCommand request, CancellationToken cancellationToken)
    {
        var meeting = await _context.Meetings
            .Include(m => m.Participants)
            .FirstOrDefaultAsync(m => m.RoomCode == request.RoomCode, cancellationToken);

        if (meeting == null)
            throw new KeyNotFoundException("Meeting not found");

        if (!request.UserRole.Equals(RoleConstants.Assessor) && (meeting.Status == MeetingStatus.Scheduled || meeting.Status == MeetingStatus.Ended || meeting.Status == MeetingStatus.Cancelled))
            throw new InvalidOperationException("Meeting has ended or not started");

        // var user = await _context.Users
        //     .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
        //
        // if (user == null)
        //     throw new KeyNotFoundException("User not found");

        // Handle session management for Assessors (Hosts)
        if (request.UserRole == RoleConstants.Assessor)
        {
            // Check if meeting has a sessionId, if not create one
            if (string.IsNullOrEmpty(meeting.SessionId))
            {
                var sessionResponse = await _vonageService.CreateSession();
                if (sessionResponse.Success && !string.IsNullOrEmpty(sessionResponse.Data))
                {
                    meeting.SessionId = sessionResponse.Data;
                    _context.Meetings.Update(meeting);
                    await _context.SaveChangesAsync(cancellationToken);
                }
                else
                {
                    throw new InvalidOperationException($"Failed to create Vonage session: {sessionResponse.Message}");
                }
            }
        }
        
        // Check for existing participant (active or inactive)
        var existingParticipant = await _context.MeetingParticipants
            .FirstOrDefaultAsync(
                p => p.MeetingId == meeting.Id && p.UserId == request.UserId,
                cancellationToken);
        
        // If participant already active, generate token and return existing record
        if (existingParticipant != null && existingParticipant.LeftAt == null)
        {
            var participantToken = GenerateTokenForParticipant(meeting.SessionId, request.UserRole,request.UserName);
            
            return new ParticipantDto(
                existingParticipant.Id,
                existingParticipant.MeetingId,
                existingParticipant.UserId,
                existingParticipant.UserName,
                null,
                null,
                existingParticipant.JoinedAt,
                existingParticipant.LeftAt,
                existingParticipant.Role,
                existingParticipant.IsMuted,
                existingParticipant.IsVideoEnabled,
                existingParticipant.IsScreenSharing,
                meeting.SessionId,
                participantToken,
                _vonageConfig.ApplicationId
            );
        }

        if (meeting.Participants.Count(p => p.LeftAt == null) >= meeting.MaxParticipants)
            throw new InvalidOperationException("Meeting is at maximum capacity");

        var participantRole =
            request.UserRole == "Assessor" ? ParticipantRole.Host : ParticipantRole.Participant;

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
                UserName = request.UserName,
                UserEmail = request.UserEmail,
                Role = participantRole,
                JoinedAt = DateTime.UtcNow,
                IsMuted = true,
                IsVideoEnabled = true,
                IsScreenSharing = false,
                JoinCount = 1
            };

            _context.MeetingParticipants.Add(participant);
        }

        if (meeting.Status == MeetingStatus.Scheduled || meeting.Status == MeetingStatus.Ended || meeting.Status == MeetingStatus.Cancelled)
        {
            meeting.Status = MeetingStatus.Active;
            meeting.IsRecordingEnabled = true;
            meeting.StartedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);

        // Generate token for the participant
        var participantTokenFinal = GenerateTokenForParticipant(meeting.SessionId, request.UserRole,request.UserName);

        return new ParticipantDto(
            participant.Id,
            participant.MeetingId,
            participant.UserId,
            request.UserName,
            null,
            null,
            participant.JoinedAt,
            participant.LeftAt,
            participant.Role,
            participant.IsMuted,
            participant.IsVideoEnabled,
            participant.IsScreenSharing,
            meeting.SessionId,
            participantTokenFinal,
            _vonageConfig.ApplicationId
        );
    }

    private string? GenerateTokenForParticipant(string? sessionId, string userRole,string ? userName)
    {
        if (string.IsNullOrEmpty(sessionId))
            return null;

        var tokenRole = userRole == "Assessor" ? "moderator" : "publisher";
        var tokenResponse = _vonageService.GenerateToken(sessionId, tokenRole,userName);
        
        return tokenResponse.Success ? tokenResponse.Data : null;
    }
}