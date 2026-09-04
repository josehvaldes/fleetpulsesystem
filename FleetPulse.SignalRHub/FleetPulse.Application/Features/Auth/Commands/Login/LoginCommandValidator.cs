using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace FleetPulse.Application.Features.Auth.Commands.Login
{
    public sealed class LoginCommandValidator: AbstractValidator<LoginCommand>
    {
        public LoginCommandValidator() 
        {
            RuleFor(x => x.Username).NotEmpty().WithMessage("Username is required.");
            RuleFor(x => x.Password).NotEmpty().WithMessage("Password is required.");
        }
    }
}
