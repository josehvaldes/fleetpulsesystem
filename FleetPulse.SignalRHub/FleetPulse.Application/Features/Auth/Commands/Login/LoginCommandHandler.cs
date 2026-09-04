using FleetPulse.Application.Common.Dtos;
using FleetPulse.Application.Common.Interfaces;
using Mediator;

namespace FleetPulse.Application.Features.Auth.Commands.Login
{
    public sealed class LoginCommandHandler(IAuthService authService) : ICommandHandler<LoginCommand, AuthenticationResult>
    {
        public async ValueTask<AuthenticationResult> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            var result = await authService.LoginAsync(request.Username, request.Password, cancellationToken);
            return result;
        }
    }
}
