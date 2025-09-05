namespace VideoMeeting.Application.Features.Authentication.DTOs;

public record AuthResponseDto(
    long UserId,
    string Email,
    string FirstName,
    string LastName,
    string Token,
    DateTime ExpiresAt
);

public record UserProfileDto(
    long Id,
    string Email,
    string FirstName,
    string LastName,
    DateTime CreatedAt,
    DateTime? LastLoginAt,
    bool IsActive
);