# REST migration notes: FleetPulse.SignalRHub -> FleetPulse.Api

Context: ADR 0020 (docs/adr/0020-...). Strangler Fig. SignalRHub stays untouched until cutover. Api projects are trimmed (only what the 3 REST endpoints need), not full copies.

## Endpoints

| Route | Method | Auth | Action | Handler / DB |
|---|---|---|---|---|
| `/api/v1/drivers` | GET (`from`, `to?`) | Required | Migrate | `GetDriversQuery` -> `DatabaseService.GetLatestDriverStatesAsync` -> `fleetpulse.driver_latest_state` |
| `/api/v1/drivers/{id}/history` | GET (`from`, `to`) | Required | Migrate | `GetDriverHistoryQuery` -> `GetGPSHistory` -> `fleetpulse.gps_history` |
| `/api/v1/alerts` | GET (`pagesize`, `pagenumber`, `status?`, `riskLevel?`, `from?`, `to?`) | Required | Migrate | `GetAlertsByStatusDateRangeQuery` -> `GetAlertsByStatusDateRangeAsync` -> `fleetpulse.alerts` |
| `/api/v1/sessions` (login) | POST | Anonymous | Stays in Hub | `LoginCommand`, `AuthService`, `JwtTokenService` |
| `/v1/fleetHub` | SignalR | Required | Stays in Hub | `FleetHub`, `RealTimeNotifier` |
| `/`, `/health`, `/healthz`, `/dbversion` | GET | `/dbversion` only | Re-create in Api | `DefaultRegistry` |
| Prometheus mapping | | | Check later | `AddPrometheusMapping` (Observability) |

Note: ADR 0020 says `/api/v1/login`, code maps `/api/v1/sessions`. YARP routes must follow the code (or fix the ADR).

## Dependency chain carried over (trimmed)

- Domain: `LatestDriverState`, `GpsPing`, `Alert`, enums `AlertStatus`, `RiskLevel`.
- Contracts: `LastestDriverStateResponse`, `GpsPingResponse`, `AlertResponse`, `PagedResponse<T>`.
- Application: 3 queries + handlers, `IDatabaseService` (only GetVersion, GetLatestDriverStatesAsync, GetGPSHistory, GetAlertsByStatusDateRangeAsync), `ValidationBehavior`, `ValidationException`, `ApplicationAssemblyMarker`.
- Infrastructure: `DatabaseService`, `AlertStatusTypeHandler`, `RiskLevelTypeHandler`, `SqlMapping`, auth validation (Smart scheme, local JWT, Entra, policies).
- Host: registries, Mapster `ContextMapping` (no Kafka), `GlobalExceptionHandler`, logging (`PythonCompatibleJsonFormatter`, `OpenTelemetryEnricher`), settings (`AppSettings`, `CorsSettings`, `OpenTelemetrySettings`), `NpgsqlDataSource`, health check, CORS, OpenTelemetry, Mediator.

Not migrated: Kafka, `FleetHub`, `RealTimeNotifier`, `SignalRSettings`, `IAuthService`, `IJwtTokenService`, `AuthService`, `JwtTokenService`, `LoginRequest`/`LoginResponse`, login validators, `GetAlertsAsync`.

## SignalRHub facts worth remembering

- Auth: `Authentication:Mode` = local | msal | both. Smart policy scheme picks Entra scheme when token issuer starts with `https://login.microsoftonline.com/` or `https://sts.windows.net/`, otherwise local scheme.
- Local JWT: HMAC secret, `Authentication:Local:Jwt` (Scheme, Secret, Issuer, Audience). Entra: `Authentication:EntraId` (Scheme, TenantId, ClientId); audience `api://{ClientId}`. Policies: `FleetUser` (scope `access_as_user`), `FleetAdminOnly` (role `Fleet.admin`).
- SignalR-only bits to drop in the Api: `access_token` query reading for `/v1/fleetHub`.
- Packages in Hub host: FluentValidation 12.1.1, Mapster 10.0.12, Mediator 3.0.2 (+SourceGenerator), JwtBearer 10.0.10, Npgsql 10.0.3, Dapper 2.1.79 (Infrastructure), OpenTelemetry 1.17.0, Serilog.AspNetCore 10.0.0, Serilog.Expressions 5.0.0, Serilog.Sinks.Console 6.1.1, AspNetCore.HealthChecks.NpgSql 9.0.0.
- Tests (Hub): `IntegrationTests/{Drivers/GetDriversTests, Drivers/GetDriversHistory, Alerts/GetAlertsTests, Alerts/DatabaseServiceTests, Infrastructure/*}`; Testcontainers (timescaledb), `TestAuthHandler`, schema from `db/init.sql` via relative link `..\..\db\init.sql` (path will differ in the Api tests project).

## Known risks / latent bugs (copy as-is for parity, fix later separately)

1. Alerts pagination is a stub: `TotalCount = alerts.Count`, `TotalPages = 1`, Has* flags false; no validation of `pagesize`/`pagenumber` (0 gives negative offset).
2. `GetDriversQuery` takes `DateTimeOffset` but the handler uses `.DateTime`.
3. `GetGPSHistory` does not select `event_id`, `accuracy`, `status`.
4. Secrets must be shared with the Hub via env vars / user secrets, not copied from appsettings.
5. Dockerfile / docker-compose need updating for the Api.
6. `SqlMapping` lives in Infrastructure but uses namespace `FleetPulse.SignalRHub.Mapping`.

## Plan

- Slice 0: foundation (csproj refs/packages, Domain, Application skeleton, Infrastructure, Program wiring, config, auth, health, exception handler, logging).
- Slice 1: `GET /api/v1/drivers` + tests.
- Slice 2: `GET /api/v1/drivers/{id}/history`.
- Slice 3: `GET /api/v1/alerts`.
- Then: parity check vs Hub, YARP routes, remove REST code from Hub.

## Progress log

- [x] Inventory
- [x] Slice 0 (builds with 0 warnings; smoke test: `/health` 200, `/dbversion` 401 without token)
- [x] Slice 1 (`GET /api/v1/drivers`; 7 integration tests pass with Testcontainers)
- [x] Slice 2 (`GET /api/v1/drivers/{id}/history`; 15 integration tests pass in total)
- [x] Slice 3 (`GET /api/v1/alerts`; 25 integration tests pass in total; pagination stub and enum parsing copied as-is, `GetAlertsTests` and `DatabaseServiceTests` ported)

## YARP + compose (done)

- `FleetPulse.YarpProxy/appsettings.json`: `/api/v1/drivers/**` and `/api/v1/alerts/**` -> cluster `fleetpulse-api` (`http://fleetpulse-api:8080/`); `/api/v1/sessions/**` and `/v1/fleetHub/**` -> cluster `fleetpulse-signalrhub` (`http://signalr-hub:8080/`). Previously the `fleetpulse-api` cluster pointed at the Hub. No catch-all route: other paths return 404 at the proxy. Verified locally: routed paths 502 (no backends), unknown 404.
- `docker/docker-compose-services.yml`: added `fleetpulse-api` (image `fleetpulse-api:1.0`, host port 8480, `.env`) and `fleetpulse-yarpproxy` (image `fleetpulse-yarpproxy:1.0`, host port 8580, depends on api + hub), network `redpanda_network`. Not added to `docker/docker-compose.yml` (network `fleetpulse_network`).
- ADR 0020 corrected: `/api/v1/login` -> `/api/v1/sessions`.
- Api container reads the shared `.env`: it needs the same `Authentication__Local__Jwt__{Secret,Issuer,Audience}`, `Authentication__EntraId__{TenantId,ClientId}` and DB connection settings as the Hub (`ConnectionStrings__FleetPulseDb`). `docker/env_sample.txt` still lists old `JwtSettings__*` names and should be refreshed.
- Image build contexts: Api = `FleetPulse.Api/` solution folder, Proxy = `FleetPulse.YarpProxy/` solution folder (`docker build -f FleetPulse.Api/Dockerfile -t fleetpulse-api:1.0 .`).
- Frontends must point their API base URL at the proxy (`http://localhost:8580`) and the proxy origin needs no extra CORS (CORS is answered by the backends using `Cors:AllowedOrigins`).

## Next steps (after Slice 3)

1. Parity check: run Hub and Api against the same DB and diff responses for the 3 endpoints (including 400/401/422 cases).
2. Decide on the latent bugs listed above (alerts paging stub, `Enum.Parse` on invalid `status`/`riskLevel` throws -> 500, `pagesize`/`pagenumber` validation); fix in the Api first, then mirror or drop in the Hub.
3. Add the Api service to docker-compose / deployment config; add YARP routes (`/api/v1/drivers`, `/api/v1/alerts` -> Api; `/api/v1/sessions`, `/v1/fleetHub` -> Hub) and fix ADR 0020 (`login` vs `sessions`).
4. After the cutover is stable, remove REST endpoints, `DatabaseService` read methods and their tests from the Hub.

## Slice 1 result

- Added: `LastestDriverStateResponse` (Contracts), `GetDriversQuery` + handler (Application), `ContextMapping` (Mapster, drivers only), `DriversRegistry`, `/api/{AppSettings:ApiVersion}` group with `RequireAuthorization()` in `ApiRegistry`. Host now references Contracts and Mapster 10.0.12. Query behavior copied as-is (`DateTimeOffset` -> `.DateTime`, `to` unused).
- Tests (`FleetPulse.Api.Tests/IntegrationTests/`): `Infrastructure/{IntegrationTestFixture, IntegrationTestCollection, IntegrationTest, FleetPulseWebApplicationFactory, TestAuthHandler}` and `Drivers/GetDriversTests` (5 ported + missing `from` -> 400 + invalid token -> 401). `db/init.sql` link `..\..\db\init.sql` works unchanged (same folder depth as the Hub tests). Docker is required for the tests.
- Gotcha: `Program.cs` reads `builder.Configuration` eagerly (auth validation), before `ConfigureAppConfiguration` in-memory values are applied. The test factory therefore sets the Jwt Secret/Issuer/Audience with `builder.UseSetting(...)`; only the connection string goes through `AddInMemoryCollection`.
- Not ported yet: `GetDriversHistory` tests (Slice 2), `GetAlertsTests` and `DatabaseServiceTests` (Slice 3; Hub fixture `PostgresFixture` may be needed for the latter).
- User changed Entra ids in `appsettings.json` to all-zero GUIDs (same as the Hub); keep it that way so startup works with Mode `both`.

## Slice 0 result (Api solution layout)

- Solution file now lists all projects (`FleetPulse.Api.slnx`). Removed `Class1.cs` placeholders (except Contracts and Api.Tests, still untouched).
- Domain: 3 entities, 2 enums. Application: `IDatabaseService` (trimmed), `ValidationBehavior`, `ValidationException`, `AddApplicationDependencies()` (no config param). Observability: `AddPrometheusMapping` (HTTP metrics only, no custom metrics), `Telemetry` (source `FleetPulse.Api`).
- Infrastructure: `AddInfrastructure` = auth validation + `IDatabaseService`; `DatabaseService` (no `GetAlertsAsync`); type handlers; `SqlMapping` now in namespace `FleetPulse.Infrastructure.Mapping`. Settings trimmed (no `AuthSettings`, no refresh-token fields). SignalR `access_token` query handling removed from auth.
- Host: `Program.cs`, `DependencyInjection.AddDependencies`, `Configuration/`, `Logging/`, `Middleware/GlobalExceptionHandler`, `Registry/ApiRegistry` + `DefaultRegistry` (`/`, `/health`, `/healthz`, `/dbversion`). No `UseHttpsRedirection` (runs behind YARP over HTTP). `public partial class Program` kept for WebApplicationFactory.
- Contracts project is still empty and NOT yet referenced by the host; add the reference and Mapster (10.0.12) in Slice 1 together with `ContextMapping` (no Kafka mappings) and the `/api/{version}` group with `RequireAuthorization()`.
- Config: `appsettings.json` has empty Jwt Secret/Issuer/Audience and Entra ids on purpose. `appsettings.Development.json` sets Issuer `FleetPulse`, Audience `FleetPulseAudience` and the Entra ids, but NOT the Secret. Provide the same secret as the Hub via `dotnet user-secrets set "Authentication:Local:Jwt:Secret" "<same as Hub>"` (project `FleetPulse.Api`) or env var `Authentication__Local__Jwt__Secret`. Startup fails with a clear message if it is missing.
- Dockerfile fixed (base image was `dotnet/10.0-alpine`, now `dotnet/aspnet:10.0-alpine`) and now copies all csprojs; build context must be the `FleetPulse.Api` solution folder. docker-compose entry for the Api is not part of this workspace and still has to be added.
- Dev DB: same connection string as the Hub (`localhost:5432`, DB `fleetpulse`). `launchSettings` ports: http 5226, https 7038.


## Migration Cost in Github Copilot

Tokens: 401.30 
USD cost: $4.01
Time spent: 2 hours