using MediatR;
using VideoMeeting.Application.Common.Interfaces;
using VideoMeeting.Application.Features.Authentication.Commands;
using VideoMeeting.Application.Features.Authentication.DTOs;

namespace VideoMeeting.Application.Features.Authentication.Handlers;

public class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, AuthResponseDto>
{
    private readonly IAuthService _authService;

    public LoginUserCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<AuthResponseDto> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        return await _authService.LoginAsync(request, cancellationToken);
    }
}