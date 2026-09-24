using FleetPulse.MockFleetHub.Configuration;
using FleetPulse.MockFleetHub.Contracts.Requests;
using FleetPulse.MockFleetHub.Contracts.Response;
using Microsoft.AspNetCore.Mvc;

namespace FleetPulse.MockFleetHub.Features.Login
{
    public static class LoginEndpoints
    {
        public static void MapLoginEndpoints(this IEndpointRouteBuilder app, IConfiguration configuration) 
        {
            var appSettings = configuration.GetSection(AppSettings.SectionName)
            .Get<AppSettings>() ?? new AppSettings();
            var version = appSettings.ApiVersion;
            app.MapPost($"/api/{version}/login", async ([FromBody] LoginRequest request) =>
            {

                var response = new LoginResponse(
                    "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxNTkxNzIzMi1iNGZiLTRhNDgtYjM0MS00ODhlMzhiNjU0MGQiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjE1OTE3MjMyLWI0ZmItNGE0OC1iMzQxLTQ4OGUzOGI2NTQwZCIsIm5hbWUiOiJhZG1pbiIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWUiOiJhZG1pbiIsImp0aSI6Ijk4OWZkOTcxLWFhMWEtNGE3Yy05ZDVlLTkzZGQ3ZTM3MGYwMCIsImlhdCI6MTc4NzU5NDk3MiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiYWRtaW4iLCJzY29wZSI6ImZsZWV0OnJlYWQiLCJuYmYiOjE3ODc1OTQ5NzIsImV4cCI6MTc4NzU5NTg3MiwiaXNzIjoiRmxlZXRQdWxzZSIsImF1ZCI6IkZsZWV0UHVsc2VBdWRpZW5jZSJ9.gI89LycuixgD94phjN5jKjmiudfuzh0OYn890kTAiK0", 
                    request.Username, 
                    3600)
                {
                };
                return response;
            }).WithName("Login")
            .Produces<LoginResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);
        }
    }
}
