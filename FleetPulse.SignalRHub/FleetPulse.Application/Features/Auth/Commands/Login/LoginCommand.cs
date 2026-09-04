using FleetPulse.Application.Common.Dtos;
using Mediator;
using System;
using System.Collections.Generic;
using System.Text;

namespace FleetPulse.Application.Features.Auth.Commands.Login
{
    public sealed record class LoginCommand(string Username, string Password) : ICommand<AuthenticationResult>
    {
    }
}
