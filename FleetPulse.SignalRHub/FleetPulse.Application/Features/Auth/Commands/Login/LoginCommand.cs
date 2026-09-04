using FleetPulse.Application.Common.Dtos;
using Mediator;

namespace FleetPulse.Application.Features.Auth.Commands.Login
{
    public sealed record class LoginCommand(string Username, string Password) : ICommand<AuthenticationResult>
    {
    }
}
