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

    public async Task<AuthResponseDto> RegisterAsync(RegisterUserCommand command,
        CancellationToken cancellationToken = default)
    {
        var email = Email.Create(command.Email);

        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email.Value, cancellationToken);

        if (existingUser != null)
            throw new InvalidOperationException("Email already registered");

        var user = new User
        {
            Email = email.Value,
            FirstName = command.FirstName.Trim(),
            LastName = command.LastName.Trim(),
            PasswordHash = HashPassword(command.Password),
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);

        var token = GenerateJwtToken(user);

        return new AuthResponseDto(
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            token,
            DateTime.UtcNow.Add(_jwtConfig.TokenLifetime)
        );
    }

    public async Task<AuthResponseDto> LoginAsync(LoginUserCommand command,
        CancellationToken cancellationToken = default)
    {
        var email = Email.Create(command.Email);

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email.Value && u.IsActive, cancellationToken);

        if (user == null || !VerifyPassword(command.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid email or password");

        user.LastLoginAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        var token = GenerateJwtToken(user);

        return new AuthResponseDto(
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            token,
            DateTime.UtcNow.Add(_jwtConfig.TokenLifetime)
        );
    }

    public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword,
        CancellationToken cancellationToken = default)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null || !VerifyPassword(currentPassword, user.PasswordHash))
            return false;

        user.PasswordHash = HashPassword(newPassword);
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<User?> GetUserByIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
    }

    public async Task<User?> UpdateProfileAsync(int userId, string firstName, string lastName,
        CancellationToken cancellationToken = default)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null) return null;

        user.FirstName = firstName.Trim();
        user.LastName = lastName.Trim();
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        return user;
    }

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
}