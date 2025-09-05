using MediatR;
using VideoMeeting.Application.Features.Authentication.DTOs;

namespace VideoMeeting.Application.Features.Authentication.Commands;

public record RegisterUserCommand(
    string Email,
    string FirstName,
    string LastName,
    string Password
) : IRequest<AuthResponseDto>;