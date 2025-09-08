using System.Security.Claims;
using MediatR;
using VideoMeeting.Api.Extensions;
using VideoMeeting.Application.Common.Models;
using VideoMeeting.Application.Features.Meetings.Commands;
using VideoMeeting.Application.Features.Meetings.DTOs;
using VideoMeeting.Application.Features.Meetings.Queries;

namespace VideoMeeting.Api.Endpoints;

public static class MeetingEndpoints
{
    public static void MapMeetingEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/meetings")
            .WithTags("Meetings")
            .WithOpenApi();

        // Create meeting
        group.MapPost("/", CreateMeetingAsync)
            .WithName("CreateMeeting")
            .WithSummary("Create a new meeting")
            .WithDescription("Creates a new video meeting with Vonage session")
            // .RequireAuthorization()
            .Produces<ApiResponse<MeetingResponseDto>>(StatusCodes.Status201Created)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized);

        // Get meeting by ID
        group.MapGet("/{id:int}", GetMeetingByIdAsync)
            .WithName("GetMeetingById")
            .WithSummary("Get meeting by ID")
            .WithDescription("Retrieves meeting details by meeting ID")
            .Produces<ApiResponse<MeetingResponseDto>>()
            .Produces<ApiResponse>(StatusCodes.Status404NotFound);

        // Get meeting by room code
        group.MapGet("/room/{roomCode}", GetMeetingByRoomCodeAsync)
            .WithName("GetMeetingByRoomCode")
            .WithSummary("Get meeting by room code")
            .WithDescription("Retrieves meeting details by 8-digit room code")
            .Produces<ApiResponse<MeetingResponseDto>>()
            .Produces<ApiResponse>(StatusCodes.Status404NotFound);

        // Get user's meetings
        group.MapGet("/my-meetings", GetUserMeetingsAsync)
            .WithName("GetMyMeetings")
            .WithSummary("Get user's meetings")
            .WithDescription("Retrieves all meetings created by the current user")
            .RequireAuthorization()
            .Produces<ApiResponse<List<MeetingListDto>>>()
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized);

        // Join meeting as authenticated user
        group.MapPost("/{roomCode}/join", JoinMeetingAsync)
            .WithName("JoinMeeting")
            .WithSummary("Join meeting as authenticated user")
            .WithDescription("Joins a meeting as an authenticated user")
            .RequireAuthorization()
            .Produces<ApiResponse<ParticipantDto>>()
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status404NotFound);

        // Join meeting as guest
        group.MapPost("/{roomCode}/join-as-guest", JoinMeetingAsGuestAsync)
            .WithName("JoinMeetingAsGuest")
            .WithSummary("Join meeting as guest user")
            .WithDescription("Allows guest users to join meetings without authentication")
            .Produces<ApiResponse<ParticipantDto>>()
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status404NotFound);

        // Get meeting participants
        group.MapGet("/{id:int}/participants", GetMeetingParticipantsAsync)
            .WithName("GetMeetingParticipants")
            .WithSummary("Get meeting participants")
            .WithDescription("Retrieves all active participants in a meeting")
            .Produces<ApiResponse<List<ParticipantDto>>>()
            .Produces<ApiResponse>(StatusCodes.Status404NotFound);

        // Start meeting recording
        group.MapPost("/{roomCode}/recordings/start", StartRecordingAsync)
            .WithName("StartRecording")
            .WithSummary("Start meeting recording")
            .WithDescription("Starts recording the meeting using Vonage")
            .RequireAuthorization()
            .Produces<ApiResponse<RecordingDto>>()
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status404NotFound);

        // Get meeting recordings
        group.MapGet("/{id:int}/recordings", GetMeetingRecordingsAsync)
            .WithName("GetMeetingRecordings")
            .WithSummary("Get meeting recordings")
            .WithDescription("Retrieves all recordings for a meeting")
            .Produces<ApiResponse<List<RecordingDto>>>()
            .Produces<ApiResponse>(StatusCodes.Status404NotFound);

        // Stop meeting recording
        group.MapPost("/{id:int}/recordings/{recordingId:int}/stop", StopRecordingAsync)
            .WithName("StopRecording")
            .WithSummary("Stop meeting recording")
            .WithDescription("Stops an active recording for the meeting")
            .RequireAuthorization()
            .Produces<ApiResponse<RecordingDto>>()
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status404NotFound);

        // Disconnect from meeting
        group.MapPost("/{roomCode}/disconnect", DisconnectFromMeetingAsync)
            .WithName("DisconnectFromMeeting")
            .WithSummary("Disconnect from meeting")
            .WithDescription("Disconnects the user from the meeting and marks them as left")
            .RequireAuthorization()
            .Produces<ApiResponse<bool>>()
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> CreateMeetingAsync(
        CreateMeetingRequest request,
        ClaimsPrincipal user,
        IMediator mediator)
    {
        try
        {
            var command = new CreateMeetingCommand(
                request.Title,
                request.Description,
                request.ScheduledAt,
                // request.IsRecordingEnabled,
                request.IsScreenSharingEnabled,
                request.MaxParticipants,
                request.CreatedById,
                request.RoomCode
            );

            var response = await mediator.Send(command);
            return response.ToCreatedResponse("Meeting created successfully");
        }
        catch (Exception ex)
        {
            return ResultExtensions.ToBadRequestResponse(ex.Message);
        }
    }

    private static async Task<IResult> GetMeetingByIdAsync(
        int id,
        IMediator mediator)
    {
        var query = new GetMeetingByIdQuery(id);
        var meeting = await mediator.Send(query);

        return meeting == null
            ? ResultExtensions.ToNotFoundResponse("Meeting not found")
            : meeting.ToApiResponse("Meeting retrieved successfully");
    }

    private static async Task<IResult> GetMeetingByRoomCodeAsync(
        string roomCode,
        IMediator mediator)
    {
        var query = new GetMeetingByRoomCodeQuery(roomCode);
        var meeting = await mediator.Send(query);

        return meeting == null
            ? ResultExtensions.ToNotFoundResponse("Meeting not found")
            : meeting.ToApiResponse("Meeting retrieved successfully");
    }

    private static async Task<IResult> GetUserMeetingsAsync(
        ClaimsPrincipal user,
        IMediator mediator)
    {
        var userId = GetUserId(user);
        var query = new GetUserMeetingsQuery(userId);
        var meetings = await mediator.Send(query);

        return meetings.ToApiResponse("User meetings retrieved successfully");
    }

    private static async Task<IResult> JoinMeetingAsync(
        string roomCode,
        JoinMeetingRequest request,
        ClaimsPrincipal user,
        IMediator mediator)
    {
        try
        {
            // var userId = GetUserId(user);
            var currentUser = GetCurrentUser(user);
            if (currentUser == null)
                return ResultExtensions.ToUnauthorizedResponse();
            
            var userId = long.Parse(currentUser.UserId);
            if (userId == 0)
                return ResultExtensions.ToUnauthorizedResponse();
            
            var fullName = currentUser.FirstName + " " + currentUser.LastName;
            var userEmail = currentUser.UsrEmail;
            var command = new JoinMeetingCommand(roomCode, userId,currentUser.RoleName,fullName,userEmail);
            var participant = await mediator.Send(command);
            
            return participant.ToApiResponse("Successfully joined meeting");
        }
        catch (KeyNotFoundException ex)
        {
            return ResultExtensions.ToNotFoundResponse(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return ResultExtensions.ToBadRequestResponse(ex.Message);
        }
        catch (Exception ex)
        {
            return ResultExtensions.ToInternalServerErrorResponse("An error occurred while joining the meeting");
        }
    }

    private static async Task<IResult> JoinMeetingAsGuestAsync(
        string roomCode,
        JoinMeetingAsGuestRequest request,
        IMediator mediator)
    {
        try
        {
            var command = new JoinMeetingAsGuestCommand(roomCode, request.GuestName, request.GuestEmail);
            var participant = await mediator.Send(command);
            return participant.ToApiResponse("Successfully joined meeting as guest");
        }
        catch (KeyNotFoundException ex)
        {
            return ResultExtensions.ToNotFoundResponse(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return ResultExtensions.ToBadRequestResponse(ex.Message);
        }
        catch (Exception ex)
        {
            return ResultExtensions.ToInternalServerErrorResponse("An error occurred while joining the meeting");
        }
    }

    private static async Task<IResult> GetMeetingParticipantsAsync(
        int id,
        IMediator mediator)
    {
        var query = new GetMeetingParticipantsQuery(id);
        var participants = await mediator.Send(query);

        return participants.ToApiResponse("Meeting participants retrieved successfully");
    }

    private static async Task<IResult> StartRecordingAsync(
        string roomCode,
        StartRecordingRequest request,
        ClaimsPrincipal user,
        IMediator mediator)
    {
        try
        {
            var currentUser = GetCurrentUser(user);
            if (currentUser == null)
                return ResultExtensions.ToUnauthorizedResponse();
            var userId = long.Parse(currentUser.UserId);
            if (userId == 0)
                return ResultExtensions.ToUnauthorizedResponse();

            var command = new StartRecordingCommand(roomCode, userId, request.RecordingName);
            var recording = await mediator.Send(command);
            return recording.ToApiResponse("Recording started successfully");
        }
        catch (KeyNotFoundException ex)
        {
            return ResultExtensions.ToNotFoundResponse(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return ResultExtensions.ToBadRequestResponse(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            return ResultExtensions.ToUnauthorizedResponse(ex.Message);
        }
        catch (Exception ex)
        {
            return ResultExtensions.ToInternalServerErrorResponse("An error occurred while starting the recording");
        }
    }

    private static async Task<IResult> GetMeetingRecordingsAsync(
        int id,
        IMediator mediator)
    {
        try
        {
            var query = new GetMeetingRecordingsQuery(id);
            var recordings = await mediator.Send(query);
            return recordings.ToApiResponse("Meeting recordings retrieved successfully");
        }
        catch (KeyNotFoundException ex)
        {
            return ResultExtensions.ToNotFoundResponse(ex.Message);
        }
        catch (Exception ex)
        {
            return ResultExtensions.ToInternalServerErrorResponse("An error occurred while retrieving recordings");
        }
    }

    private static async Task<IResult> StopRecordingAsync(
        int id,
        int recordingId,
        ClaimsPrincipal user,
        IMediator mediator)
    {
        try
        {
            var userId = GetUserId(user);
            if (userId == 0)
                return ResultExtensions.ToUnauthorizedResponse();

            var command = new StopRecordingCommand(id, recordingId, userId);
            var recording = await mediator.Send(command);
            return recording.ToApiResponse("Recording stopped successfully");
        }
        catch (KeyNotFoundException ex)
        {
            return ResultExtensions.ToNotFoundResponse(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return ResultExtensions.ToBadRequestResponse(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            return ResultExtensions.ToUnauthorizedResponse(ex.Message);
        }
        catch (Exception ex)
        {
            return ResultExtensions.ToInternalServerErrorResponse("An error occurred while stopping the recording");
        }
    }

    private static async Task<IResult> DisconnectFromMeetingAsync(
        string roomCode,
        ClaimsPrincipal user,
        IMediator mediator)
    {
        try
        {
            var currentUser = GetCurrentUser(user);
            if (currentUser == null)
                return ResultExtensions.ToUnauthorizedResponse();
            
            var userId = long.Parse(currentUser.UserId);
            if (userId == 0)
                return ResultExtensions.ToUnauthorizedResponse();

            var command = new DisconnectFromMeetingCommand(roomCode, userId);
            var result = await mediator.Send(command);
            return true.ToApiResponse("Successfully disconnected from meeting");
        }
        catch (KeyNotFoundException ex)
        {
            return ResultExtensions.ToNotFoundResponse(ex.Message);
        }
        catch (Exception ex)
        {
            return ResultExtensions.ToInternalServerErrorResponse("An error occurred while disconnecting from the meeting");
        }
    }

    private static int GetUserId(ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirst("UserId")?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }
    
    private static UserClaims? GetCurrentUser(ClaimsPrincipal? user)
    {
        if (user == null) return null!;

        return new UserClaims(
            UserId: user.FindFirst("UserId")?.Value ?? string.Empty,
            GuId: user.FindFirst("GuId")?.Value ?? string.Empty,
            RoleId: user.FindFirst("roleid")?.Value ?? string.Empty,
            RoleName: user.FindFirst("rolename")?.Value ?? string.Empty,
            FirstName: user.FindFirst("firstname")?.Value ?? string.Empty,
            LastName: user.FindFirst("lastname")?.Value ?? string.Empty,
            UsrEmail: user.FindFirst("usr_email")?.Value ?? string.Empty,
            Mobile: user.FindFirst("mobile")?.Value ?? string.Empty,
            CreationDate: user.FindFirst("creationdate")?.Value ?? string.Empty,
            IsActive: user.FindFirst("isactive")?.Value ?? string.Empty,
            AsrId: user.FindFirst("asr_id")?.Value ?? string.Empty,
            AllctdPrgms: user.FindFirst("allctd_prgms")?.Value ?? string.Empty,
            Exp: long.TryParse(user.FindFirst("exp")?.Value, out var expVal) ? expVal : 0,
            Iss: user.FindFirst("iss")?.Value ?? string.Empty,
            Aud: user.FindFirst("aud")?.Value ?? string.Empty
        );
    }

}

// Request DTOs
public record CreateMeetingRequest(
    string Title,
    string? Description,
    DateTime ScheduledAt,
    long CreatedById,
    bool IsRecordingEnabled = false,
    bool IsScreenSharingEnabled = true,
    int MaxParticipants = 20,
    string? RoomCode = null
);

public record JoinMeetingAsGuestRequest(
    string GuestName,
    string? GuestEmail = null
);

public record JoinMeetingRequest(
    string? UserName,
    string? UserRole,
    string? UserEmail = null
);

public record StartRecordingRequest(
    string? RecordingName = null
);

public record UserClaims(
    string UserId,
    string GuId,
    string RoleId,
    string RoleName,
    string FirstName,
    string LastName,
    string UsrEmail,
    string Mobile,
    string CreationDate,
    string IsActive,
    string AsrId,
    string AllctdPrgms,
    long Exp,
    string Iss,
    string Aud
);
