using FleetPulse.Application.Features.Auth.Commands.Login;
using FleetPulse.Contracts.Requests;
using FleetPulse.Contracts.Response.Auth;
using FleetPulse.SignalRHub.Validators;
using FluentValidation;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace FleetPulse.SignalRHub.Registry
{
    public static class AuthRegistry
    {
        public static void RegisterAuth(this IEndpointRouteBuilder app, string version) 
        {
            app.MapPost($"/api/{version}/login", async (IMediator mediator,
                IValidator<LoginRequest> loginValidator,
                [FromBody] LoginRequest request) =>
            {
                var validationResult = await loginValidator.ValidateAsync(request);
                validationResult.ThrowIfInvalid();

                var command = new LoginCommand(request.Username, request.Password);
                var result = await mediator.Send(command);

                // Map application DTO to API contract
                var response = new LoginResponse(result.AccessToken, result.Username, result.ExpiresIn)
                {
                    RawRefreshToken = result.RawRefreshToken,
                    RefreshTokenExpiry = result.RefreshTokenExpiry
                };

                return response;
            });
        }
    }
}
