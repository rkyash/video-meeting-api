using MediatR;
using VideoMeeting.Application.Features.Authentication.DTOs;

namespace VideoMeeting.Application.Features.Authentication.Commands;

public record LoginUserCommand(
    string Email,
    string Password
) : IRequest<AuthResponseDto>;