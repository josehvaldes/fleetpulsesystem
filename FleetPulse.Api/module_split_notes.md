# Api Module Split tracker (ADR 0021)

Goal: FleetPulse.Api is the only project outside `Alerts/` and `Drivers/`. Common code (`ValidationBehavior`, `ValidationException`, assembly marker) is duplicated per module.

## Decisions

- No `Shared.Kernel` (nothing cross-module yet; the host is the only project outside the module folders). Deviation from ADR section 4.
- `Observability` and auth (`Infrastructure/Settings`, `AddAppAuthentication`) move into the host in step 5.
- `{Module}.Contracts` = HTTP response DTOs; `{Module}.Public` = ports/events (empty for now).
- DB schema split (`drivers` / `alerts`) is deferred: `fleetpulse` schema and `db/init.sql` are shared with the Hub.
- `/dbversion` is reimplemented in the host with `NpgsqlDataSource` when `IDatabaseService` is removed.
- Dependency graph: Domain -> none; Application -> Domain, Public; Infrastructure -> Application, Domain; Api -> Application, Contracts, Domain, Infrastructure; host -> module Api projects.

## Steps

- [x] 1. Baseline: 25 integration tests pass.
- [x] 2. Module csproj files and packages (both modules), placeholder `Class1.cs` / stub `DependencyInjection.cs` removed (except the `*.Tests` projects, handled in step 6).
- [x] 3. Drivers module
  - Domain: `LatestDriverState`, `GpsPing`
  - Contracts: `LastestDriverStateResponse`, `GpsPingResponse` (namespace `FleetPulse.Drivers.Contracts.Response`)
  - Application: `GetDrivers`, `GetDriverHistory`, `IDriversRepository`, duplicated `Common/*`, `DriversApplicationAssemblyMarker`, `AddDriversApplication()`
  - Infrastructure: `DriversRepository` (Dapper)
  - Api: `MapDriversEndpoints()`, `DriversMapping`, `AddDriversModule()`
  - Host wired (`Program.cs`, `ApiRegistry`, Mediator assemblies + Drivers `ValidationBehavior`); old drivers code removed from the flat projects; `Api.Tests` updated to the new contracts namespace.
  - Result: solution builds with 0 warnings, 25 tests pass.
- [x] 4. Alerts module
  - Domain: `Alert`, `AlertStatus`, `RiskLevel`
  - Contracts: `AlertResponse`, `PagedResponse<T>` (namespace `FleetPulse.Alerts.Contracts.Response`)
  - Application: `GetAlertsByStatusDateRange`, `IAlertsRepository`, duplicated `Common/*`, `AlertsApplicationAssemblyMarker`, `AddAlertsApplication()`
  - Infrastructure: `AlertsRepository`, type handlers, `AlertsSqlMapping`
  - Api: `MapAlertsEndpoints()`, `AlertsMapping`, `AddAlertsModule()` (registers Dapper type handlers)
  - Host wired; old alerts code, `ContextMapping` and `SqlMapping` removed; `IDatabaseService` now only has `GetVersion`; `DatabaseServiceTests` ported to `AlertsRepository`.
  - Result: build 0 warnings, 25 tests pass. Old `FleetPulse.Domain` and `FleetPulse.Contracts` are now empty.
- [x] 5. Host cleanup
  - Moved into the host: `Authentication/` (settings + `AddAuthenticationAndAuthorization`), `Observability/` (`Telemetry`, `AddPrometheusMapping`); `/dbversion` now uses `NpgsqlDataSource` directly.
  - `GlobalExceptionHandler` handles both module `ValidationException` types (422).
  - Mediator pipeline now lists only the module `ValidationBehavior`s.
  - Deleted `FleetPulse.Application`, `Contracts`, `Domain`, `Infrastructure`, `Observability` (and slnx entries); host and `Api.Tests` csproj references cleaned; host gained JwtBearer, IdentityModel, System.IdentityModel.Tokens.Jwt and prometheus-net packages.
  - Dockerfile `COPY` lines now list the module csprojs (done here instead of step 7).
  - Result: build 0 warnings, 25 tests pass.
- [x] 6. Tests
  - `FleetPulse.Api.Tests` deleted; `Drivers.Tests` (15 tests) and `Alerts.Tests` (10 tests, `DatabaseServiceTests` renamed `AlertsRepositoryTests`).
  - Each project has its own copy of the fixture, factory, collection, `IntegrationTest` and `TestAuthHandler` (namespace `FleetPulse.{Module}.Tests.IntegrationTests`).
  - `db/init.sql` link is now `..\..\..\db\init.sql`; each project references the host, its module Contracts (Alerts also Infrastructure).
  - Result: build 0 warnings, 25 tests pass (10 + 15).
- [x] 7. Dockerfile `COPY` lines (module csprojs), `migration_notes.md` update note, ADR 0021 status Accepted + addendum
- [x] 8. Final verification
  - Clean rebuild of `FleetPulse.Api.slnx`; 25 tests pass (Alerts 10, Drivers 15).
  - No source or csproj in `Alerts/` references `FleetPulse.Drivers.*` or the reverse (tests excluded).
  - Only `FleetPulse.Api` sits outside `Alerts/` and `Drivers/` (besides `.github`, `.codacy`, `TestResults`).
  - Not done: Docker image build and smoke test through YARP (needs the shared secrets and a running DB/proxy).

## Notes / caveats

- Both the old `ValidationBehavior` and the Drivers one are registered in the host Mediator pipeline. They only run validators found in DI, so with no validators there is no double validation. Revisit when a module adds validators (each behavior resolves `IValidator<T>` for every message).
- Dapper type handlers (Alerts) are global; register them in `AddAlertsModule` before first query.
- `Drivers.Tests` / `Alerts.Tests` still contain their placeholder files.
