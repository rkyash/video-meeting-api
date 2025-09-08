using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using VideoMeeting.Application.Common.Interfaces;
using VideoMeeting.Application.Features.Authentication.Commands;
using VideoMeeting.Application.Features.Authentication.DTOs;
using VideoMeeting.Domain.Entities;
using VideoMeeting.Domain.ValueObjects;
using VideoMeeting.Shared.Configuration;
using TokenValidationResult = VideoMeeting.Application.Features.Authentication.DTOs.TokenValidationResult;

namespace VideoMeeting.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly JwtConfiguration _jwtConfig;
    private readonly IApplicationDbContext _context;

    public AuthService(IApplicationDbContext context, JwtConfiguration jwtConfig)
    {
        _context = context;
        _jwtConfig = jwtConfig;
    }

    // public async Task<AuthResponseDto> RegisterAsync(RegisterUserCommand command,
    //     CancellationToken cancellationToken = default)
    // {
    //     var email = Email.Create(command.Email);
    //
    //     var existingUser = await _context.Users
    //         .FirstOrDefaultAsync(u => u.Email == email.Value, cancellationToken);
    //
    //     if (existingUser != null)
    //         throw new InvalidOperationException("Email already registered");
    //
    //     var user = new User
    //     {
    //         Email = email.Value,
    //         FirstName = command.FirstName.Trim(),
    //         LastName = command.LastName.Trim(),
    //         PasswordHash = HashPassword(command.Password),
    //         CreatedAt = DateTime.UtcNow,
    //         IsActive = true
    //     };
    //
    //     _context.Users.Add(user);
    //     await _context.SaveChangesAsync(cancellationToken);
    //
    //     var token = GenerateJwtToken(user);
    //
    //     return new AuthResponseDto(
    //         user.Id,
    //         user.Email,
    //         user.FirstName,
    //         user.LastName,
    //         token,
    //         DateTime.UtcNow.Add(_jwtConfig.TokenLifetime)
    //     );
    // }
    //
    // public async Task<AuthResponseDto> LoginAsync(LoginUserCommand command,
    //     CancellationToken cancellationToken = default)
    // {
    //     var email = Email.Create(command.Email);
    //
    //     var user = await _context.Users
    //         .FirstOrDefaultAsync(u => u.Email == email.Value && u.IsActive, cancellationToken);
    //
    //     if (user == null || !VerifyPassword(command.Password, user.PasswordHash))
    //         throw new UnauthorizedAccessException("Invalid email or password");
    //
    //     user.LastLoginAt = DateTime.UtcNow;
    //     await _context.SaveChangesAsync(cancellationToken);
    //
    //     var token = GenerateJwtToken(user);
    //
    //     return new AuthResponseDto(
    //         user.Id,
    //         user.Email,
    //         user.FirstName,
    //         user.LastName,
    //         token,
    //         DateTime.UtcNow.Add(_jwtConfig.TokenLifetime)
    //     );
    // }

    // public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword,
    //     CancellationToken cancellationToken = default)
    // {
    //     var user = await _context.Users
    //         .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
    //
    //     if (user == null || !VerifyPassword(currentPassword, user.PasswordHash))
    //         return false;
    //
    //     user.PasswordHash = HashPassword(newPassword);
    //     user.UpdatedAt = DateTime.UtcNow;
    //     await _context.SaveChangesAsync(cancellationToken);
    //
    //     return true;
    // }

    // public async Task<User?> GetUserByIdAsync(int userId, CancellationToken cancellationToken = default)
    // {
    //     return await _context.Users
    //         .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
    // }
    //
    // public async Task<User?> UpdateProfileAsync(int userId, string firstName, string lastName,
    //     CancellationToken cancellationToken = default)
    // {
    //     var user = await _context.Users
    //         .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
    //
    //     if (user == null) return null;
    //
    //     user.FirstName = firstName.Trim();
    //     user.LastName = lastName.Trim();
    //     user.UpdatedAt = DateTime.UtcNow;
    //     await _context.SaveChangesAsync(cancellationToken);
    //
    //     return user;
    // }

    public string GenerateJwtToken(User user)
    {
        if (string.IsNullOrEmpty(_jwtConfig.Key))
            throw new InvalidOperationException("JWT Key is not configured");
            
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtConfig.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim("userId", user.Id.ToString())
        };

        var token = new JwtSecurityToken(
            _jwtConfig.Issuer,
            _jwtConfig.Audience,
            claims,
            expires: DateTime.UtcNow.Add(_jwtConfig.TokenLifetime),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public bool VerifyPassword(string password, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }

    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    public async Task<TokenValidationResult> ValidateJwtTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return new TokenValidationResult(
                    IsValid: false,
                    ErrorMessage: "Token is null or empty",
                    ExpiresAt: null,
                    UserId: null,
                    Email: null,
                    FullName: null,
                    IsExpired: false,
                    HasValidSignature: false
                );
            }

            // Remove Bearer prefix if present
            if (token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                token = token.Substring(7);

            var tokenHandler = new JwtSecurityTokenHandler();
            
            // First check if token format is valid
            if (!tokenHandler.CanReadToken(token))
            {
                return new TokenValidationResult(
                    IsValid: false,
                    ErrorMessage: "Invalid token format",
                    ExpiresAt: null,
                    UserId: null,
                    Email: null,
                    FullName: null,
                    IsExpired: false,
                    HasValidSignature: false
                );
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtConfig.Key));
            
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = _jwtConfig.Issuer,
                ValidateAudience = true,
                ValidAudience = _jwtConfig.Audience,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ClockSkew = TimeSpan.Zero
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
            
            // Extract claims
            var userIdClaim = principal.FindFirst("userId")?.Value;
            var emailClaim = principal.FindFirst(ClaimTypes.Email)?.Value;
            var nameClaim = principal.FindFirst(ClaimTypes.Name)?.Value;
            
            if (!long.TryParse(userIdClaim, out var userId))
            {
                return new TokenValidationResult(
                    IsValid: false,
                    ErrorMessage: "Invalid user ID in token",
                    ExpiresAt: ((JwtSecurityToken)validatedToken).ValidTo,
                    UserId: null,
                    Email: emailClaim,
                    FullName: nameClaim,
                    IsExpired: false,
                    HasValidSignature: true
                );
            }

            // // Verify user exists in database
            // var user = await _context.Users
            //     .FirstOrDefaultAsync(u => u.Id == userId && u.IsActive, cancellationToken);
            //
            // if (user == null)
            // {
            //     return new TokenValidationResult(
            //         IsValid: false,
            //         ErrorMessage: "User not found or inactive",
            //         ExpiresAt: ((JwtSecurityToken)validatedToken).ValidTo,
            //         UserId: userId,
            //         Email: emailClaim,
            //         FullName: nameClaim,
            //         IsExpired: false,
            //         HasValidSignature: true
            //     );
            // }

            return new TokenValidationResult(
                IsValid: true,
                ErrorMessage: null,
                ExpiresAt: ((JwtSecurityToken)validatedToken).ValidTo,
                UserId: userId,
                Email: emailClaim,
                FullName: nameClaim,
                IsExpired: false,
                HasValidSignature: true
            );
        }
        catch (SecurityTokenExpiredException)
        {
            // Try to get token info even if expired
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(token);
                
                var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "userId")?.Value;
                var emailClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
                var nameClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
                
                return new TokenValidationResult(
                    IsValid: false,
                    ErrorMessage: "Token has expired",
                    ExpiresAt: jwtToken.ValidTo,
                    UserId: long.TryParse(userIdClaim, out var uid) ? uid : null,
                    Email: emailClaim,
                    FullName: nameClaim,
                    IsExpired: true,
                    HasValidSignature: true
                );
            }
            catch
            {
                return new TokenValidationResult(
                    IsValid: false,
                    ErrorMessage: "Token has expired and cannot be parsed",
                    ExpiresAt: null,
                    UserId: null,
                    Email: null,
                    FullName: null,
                    IsExpired: true,
                    HasValidSignature: false
                );
            }
        }
        catch (SecurityTokenInvalidSignatureException)
        {
            return new TokenValidationResult(
                IsValid: false,
                ErrorMessage: "Invalid token signature",
                ExpiresAt: null,
                UserId: null,
                Email: null,
                FullName: null,
                IsExpired: false,
                HasValidSignature: false
            );
        }
        catch (Exception ex)
        {
            return new TokenValidationResult(
                IsValid: false,
                ErrorMessage: $"Token validation failed: {ex.Message}",
                ExpiresAt: null,
                UserId: null,
                Email: null,
                FullName: null,
                IsExpired: false,
                HasValidSignature: false
            );
        }
    }

    public bool IsTokenValidWithSignature(string token)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(token))
                return false;

            // Remove Bearer prefix if present
            if (token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                token = token.Substring(7);

            var tokenHandler = new JwtSecurityTokenHandler();
            
            if (!tokenHandler.CanReadToken(token))
                return false;

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtConfig.Key));
            
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = _jwtConfig.Issuer,
                ValidateAudience = true,
                ValidAudience = _jwtConfig.Audience,
                ValidateLifetime = false, // Don't validate expiry for signature check
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ClockSkew = TimeSpan.Zero
            };

            tokenHandler.ValidateToken(token, validationParameters, out _);
            return true;
        }
        catch (SecurityTokenInvalidSignatureException)
        {
            return false;
        }
        catch
        {
            return false;
        }
    }
}