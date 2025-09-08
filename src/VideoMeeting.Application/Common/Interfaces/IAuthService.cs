using VideoMeeting.Application.Features.Authentication.Commands;
using VideoMeeting.Application.Features.Authentication.DTOs;
using VideoMeeting.Domain.Entities;

namespace VideoMeeting.Application.Common.Interfaces;

public interface IAuthService
{
    // Task<AuthResponseDto> RegisterAsync(RegisterUserCommand command, CancellationToken cancellationToken = default);
    // Task<AuthResponseDto> LoginAsync(LoginUserCommand command, CancellationToken cancellationToken = default);
    //
    // Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword,
    //     CancellationToken cancellationToken = default);
    //
    // Task<User?> GetUserByIdAsync(int userId, CancellationToken cancellationToken = default);

    // Task<User?> UpdateProfileAsync(int userId, string firstName, string lastName,
    //     CancellationToken cancellationToken = default);
    //
    // string GenerateJwtToken(User user);
    bool VerifyPassword(string password, string hash);
    string HashPassword(string password);
    
    // JWT Token Validation
    Task<TokenValidationResult> ValidateJwtTokenAsync(string token, CancellationToken cancellationToken = default);
    bool IsTokenValidWithSignature(string token);
}