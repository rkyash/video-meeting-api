using System.Security.Claims;
using MediatR;
using VideoMeeting.Api.Extensions;
using VideoMeeting.Application.Common.Interfaces;
using VideoMeeting.Application.Common.Models;
using VideoMeeting.Application.Features.Authentication.Commands;
using VideoMeeting.Application.Features.Authentication.DTOs;

namespace VideoMeeting.Api.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth")
            .WithTags("Authentication")
            .WithOpenApi();

        group.MapPost("/register", RegisterAsync)
            .WithName("Register")
            .WithSummary("Register a new user account")
            .WithDescription("Creates a new user account and returns JWT token")
            .Produces<ApiResponse<AuthResponseDto>>(StatusCodes.Status201Created)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status409Conflict);

        group.MapPost("/login", LoginAsync)
            .WithName("Login")
            .WithSummary("Authenticate user and get JWT token")
            .WithDescription("Validates user credentials and returns JWT token")
            .Produces<ApiResponse<AuthResponseDto>>()
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized);
        //
        // group.MapGet("/profile", GetProfileAsync)
        //     .WithName("GetProfile")
        //     .WithSummary("Get current user profile")
        //     .WithDescription("Returns the current authenticated user's profile information")
        //     .RequireAuthorization()
        //     .Produces<ApiResponse<UserProfileDto>>()
        //     .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
        //     .Produces<ApiResponse>(StatusCodes.Status404NotFound);

        // group.MapPost("/change-password", ChangePasswordAsync)
        //     .WithName("ChangePassword")
        //     .WithSummary("Change user password")
        //     .WithDescription("Updates the current user's password")
        //     .RequireAuthorization()
        //     .Produces<ApiResponse>(StatusCodes.Status204NoContent)
        //     .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
        //     .Produces<ApiResponse>(StatusCodes.Status401Unauthorized);

        group.MapPost("/validate-token", ValidateTokenAsync)
            .WithName("ValidateToken")
            .WithSummary("Validate JWT token")
            .WithDescription("Validates a JWT token and returns validation details")
            .Produces<ApiResponse<TokenValidationResult>>()
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> RegisterAsync(
        RegisterUserCommand command,
        IMediator mediator)
    {
        try
        {
            var response = await mediator.Send(command);
            return response.ToCreatedResponse("User registered successfully");
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("Email already registered"))
        {
            return ResultExtensions.ToConflictResponse("Email address is already registered");
        }
        catch (Exception ex)
        {
            return ResultExtensions.ToBadRequestResponse(ex.Message);
        }
    }

    private static async Task<IResult> LoginAsync(
        LoginUserCommand command,
        IMediator mediator)
    {
        try
        {
            var response = await mediator.Send(command);
            return response.ToApiResponse("User authenticated successfully");
        }
        catch (UnauthorizedAccessException)
        {
            return ResultExtensions.ToUnauthorizedResponse("Invalid email or password");
        }
        catch (Exception ex)
        {
            return ResultExtensions.ToBadRequestResponse(ex.Message);
        }
    }

    // private static async Task<IResult> GetProfileAsync(
    //     ClaimsPrincipal user,
    //     IAuthService authService)
    // {
    //     var userId = GetUserId(user);
    //     if (userId == 0)
    //         return ResultExtensions.ToUnauthorizedResponse();
    //
    //     var userEntity = await authService.GetUserByIdAsync(userId);
    //
    //     if (userEntity == null)
    //         return ResultExtensions.ToNotFoundResponse("User not found");
    //
    //     var profile = new UserProfileDto(
    //         userEntity.Id,
    //         userEntity.Email,
    //         userEntity.FirstName,
    //         userEntity.LastName,
    //         userEntity.CreatedAt,
    //         userEntity.LastLoginAt,
    //         userEntity.IsActive
    //     );
    //
    //     return profile.ToApiResponse("Profile retrieved successfully");
    // }

    // private static async Task<IResult> ChangePasswordAsync(
    //     ChangePasswordRequest request,
    //     ClaimsPrincipal user,
    //     IAuthService authService)
    // {
    //     var userId = GetUserId(user);
    //     if (userId == 0)
    //         return ResultExtensions.ToUnauthorizedResponse();
    //
    //     var success = await authService.ChangePasswordAsync(userId, request.CurrentPassword, request.NewPassword);
    //
    //     if (!success)
    //         return ResultExtensions.ToBadRequestResponse("Current password is incorrect");
    //
    //     return ResultExtensions.ToNoContentResponse("Password changed successfully");
    // }

    private static async Task<IResult> ValidateTokenAsync(
        ValidateTokenRequest request,
        IAuthService authService)
    {
        try
        {
            var result = await authService.ValidateJwtTokenAsync(request.Token);
            return result.ToApiResponse("Token validation completed");
        }
        catch (Exception ex)
        {
            return ResultExtensions.ToBadRequestResponse(ex.Message);
        }
    }

    private static int GetUserId(ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirst("userId")?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }
}

public record ChangePasswordRequest(string CurrentPassword, string NewPassword);

public record ValidateTokenRequest(string Token);