# ADR 0021 split FleetPulse.API into two in-process modules (Modular Monolith)

## Status: 
Proposed

## Date: 
2026-10-10

## Context
FleetPulse.Api currently hosts two distinct functional areas in a single Minimal API project:

Drivers — GET /api/v1/drivers, GET /api/v1/drivers/{id}/history
Alerts — GET /api/v1/alerts, plus the projection logic that feeds the SignalR ReceiveAlert callback
Both areas share the same database connection, DI container, and HTTP pipeline. The lack of a structural boundary has the usual symptoms of a growing monolith:

Domain types (e.g., Alert, Driver, GpsPing) cross-reference each other inside one project.
Endpoint registration, repository classes, and DTOs live in flat namespaces; it is unclear which code "belongs" to which capability.
Any change in the Alerts area triggers a full redeploy of Drivers endpoints, even though their build/test surfaces are independent.
The only integration seam today is the database, which makes a future extraction (e.g., Alerts → its own service) expensive.
A full move to microservices now would be premature: there is no operational need for independent scaling, no separate teams, and no network-fault-tolerance requirements between Drivers and Alerts. The right move is to introduce module boundaries inside the same deployable — a Modular Monolith — so that domain ownership is explicit today and extraction is mechanical tomorrow.

This ADR records the module cut, the project layout, and the inter-module communication patterns.

## Decision
** 1. Split the API into two in-process modules
Decompose FleetPulse.Api into two modules hosted in the same ASP.NET Core process:

Drivers module — owns driver state, GPS history, latest-state projection.
Alerts module — owns alerts, risk levels, escalation, recommendations.
Both modules run inside a single FleetPulse.Api composition root, behind the existing YARP gateway. Endpoints retain their current /api/v1/* paths; YARP routing does not change.

** 2. Each module follows Clean Architecture and owns four layers
For each module {Alerts | Drivers}:

```text

FleetPulse.{Module}.Public/         # contracts published to other modules
FleetPulse.{Module}.Domain/         # entities, value objects, domain services
FleetPulse.{Module}.Application/    # CQRS handlers (Mediator.SourceGenerator), ports, projections
FleetPulse.{Module}.Infrastructure/# Dapper + Npgsql repositories, port implementations
FleetPulse.{Module}.Api/           # Minimal API endpoint definitions: Add{Module}Endpoints()
```

Reference rules (compile-time):

Domain depends on Shared.Kernel only.
Application depends on Domain + Public (its own and, when consuming, another module's Public).
Infrastructure depends on Application + Domain (never on another module's Application).
Api depends on Application + Infrastructure of the same module.
The composition root (FleetPulse.Api) references every .Api and .Infrastructure project — it is the only place where both modules meet.
The dependency graph is a DAG; no cyclic project references are permitted.

** 3. Each module exposes a .Public library as its only integration surface
Each module owns a FleetPulse.{Module}.Public project. It contains only the contracts other modules may consume:

Sync port interfaces — e.g., IDriverInfoPort, IAlertQueryPort.
Async event contracts — INotification records, e.g., AlertRaisedNotification, DriverStatusChangedNotification.
Read-model DTOs — e.g., DriverSnapshot, AlertSummaryDto.
Internal folders/namespaces: Ports/, Events/, ReadModels/.

A module may reference another module's Public library but never its Application, Domain, or Infrastructure. The Public library is the integration contract surface; it is owned and versioned by the publishing module.

** 4. A minimal FleetPulse.Shared.Kernel library holds neutral primitives only
```text

FleetPulse.Shared.Kernel/
    DriverId.cs            # readonly record struct
    AlertId.cs            # readonly record struct
    IDateTimeProvider.cs
    IEventBus.cs          # optional thin facade over IMediator
```

Shared.Kernel is not a place for service interfaces, DTOs, or domain entities. It exists only for value objects that genuinely belong to no single module.

** 5. Inter-module communication follows three patterns
* Pattern A — Synchronous port (supplier publishes contract).*

The supplier module owns the interface in its Public library; it implements it in its Infrastructure. The consumer injects the interface.
Phase 1 (now): all cross-module calls are synchronous via ports.

* Pattern B — Asynchronous event (Mediator INotification).*

The publisher module owns the notification contract in its Public library; the subscriber module implements INotificationHandler<T> in its Application layer.

Phase 2 (next): fire-and-forget projections (e.g., Drivers maintains a driver_latest_alert view populated from AlertRaisedNotification) replace synchronous queries whenever read-freshness requirements allow eventual consistency.

* Pattern C — Shared Kernel primitives.*

Only neutral value objects (DriverId, AlertId), IDateTimeProvider, and (optionally) IEventBus live in Shared.Kernel. Domain service interfaces and event contracts do not belong here.

** 6. Database schemas mirror module boundaries
Postgres/TimescaleDB schemas:

drivers — driver_latest_state, gps_history (hypertable).
alerts — alert, alert_escalation.
Rules:

No cross-schema foreign keys. References between modules use the value-object ID (driver_id as text/uuid) only.
Read models that join across schemas are maintained by the consuming module via events (Pattern B), or as Postgres views owned by neither module's write side.

** 7. Composition root wires the modules
```csharp

// FleetPulse.Api/Program.cs
builder.Services.AddSharedKernel();
builder.Services.AddDriversModule(builder.Configuration);
builder.Services.AddAlertsModule(builder.Configuration);
```

Each module exposes a single Add{Module}Module extension method. Cross-module port wiring lives inside the supplier's **Add{name}Module** (Pattern A) — never at the composition root and never in a generic AddApplicationServices helper.

** 8. Extraction path to microservices
When (and only when) a module needs to scale or deploy independently:

The Public library of that module becomes its HTTP API surface (an OpenAPI document + a client library).
In the other module's Infrastructure, the port implementation is replaced by an HTTP client (e.g., DriverInfoPort → HttpDriverInfoPort).
The Mediator INotification is replaced by a Redpanda producer/consumer; the IEventBus facade (if present) swaps its implementation.
Domain and Application layers are unchanged.
No code in Domain or Application is rewritten for extraction.


## Alternatives
** A. Shared BuildingBlocks.dll containing cross-module interfaces
A single library holds IAlertService, IDriverService, and shared DTOs that both modules reference.
Rejected: it becomes a coupling magnet, neither module owns the contracts, and any change to a shared interface forces recompilation of both modules. It also fails to model ownership, which makes future extraction ambiguous.

** B. Shared FleetPulse.Contracts library for events only
A neutral library holds every INotification contract; modules publish and subscribe via that library.
Rejected: the publisher of an event should own the event. A neutral contracts library re-creates the BuildingBlocks problem at the event layer and obscures change authority.

** C. Consumer-owns-port (strict Hexagonal / Ports & Adapters)
The consuming module defines the port in its Application layer; the supplier implements it. Cross-module DI wiring lives at the composition root.
Considered, not chosen: it is purer from a DDD perspective, but it forces the composition root to register every cross-module port, and it blurs the project reference DAG (Drivers.Infrastructure must reference Alerts.Application, and vice-versa). The supplier-publishes variant chosen here yields a cleaner DAG and a more direct microservice extraction path.

## Consequences
** Benefits
- Explicit ownership — every cross-module contract lives in the publishing module's Public library; change authority is unambiguous from the namespace.
- Clean DAG — compile-time dependencies form a directed acyclic graph; cyclic references are caught at build time.
- Independent test surfaces — each module's domain and application layers can be unit-tested in isolation; integration tests live at the composition root.
- Parallel development — Drivers and Alerts work can proceed without colliding once the Public contracts are agreed.
- Microservice extraction is mechanical — replace a port implementation or swap an in-memory INotification for Redpanda; no domain code changes.
- Database hygiene — schema-per-module prevents accidental cross-cutting writes and keeps write models independent.
- Preserves the existing API surface — YARP routes and /api/v1/* paths do not change; the frontend is unaffected.

** Trade-offs
- More projects — roughly 11 projects instead of 1. Builds are slightly slower; Solution Explorer is more crowded.
- More boilerplate — each module defines Add{Module}Module, port interfaces, DTO mapping at the boundary.
- Discipline required — the rules in §2 and §3 must be enforced by code review and (optionally) an ArchUnit-style test. A single select * from alerts, drivers join in someone's repository quietly undoes the boundary.
- Mapping at the boundary — DTOs in Public must not leak domain entities; every port call may require a mapping. This is by design but adds code.
- Phase 2 will introduce eventual consistency — moving from sync ports to events for some flows changes failure semantics; subscribers must be idempotent.


** Risks 
- Schema-creep in Shared.Kernel — without vigilance, it accumulates "shared" service interfaces and becomes the BuildingBlocks we rejected. Mitigation: a code-review rule that any addition to Shared.Kernel must justify why it cannot live in a module's Public.
- Cross-module SQL joins — developers may be tempted to write a Dapper query that joins drivers and alerts schemas directly. This is invisible to the compiler. Mitigation: an integration test that asserts no repository issues SQL containing both schema names; or schema-level DB roles with restricted permissions.
- Premature event introduction — using Pattern B where a sync port would do introduces eventual-consistency bugs. Mitigation: prefer Pattern A until a concrete requirement (e.g., fan-out to multiple subscribers, decoupling write latency) justifies an event.
- Symmetric cross-references — if Drivers and Alerts need data from each other, the temptation is to put the port in the consumer rather than the supplier (Alternative C). Mitigation: always ask "who owns this contract?" — the answer is the module whose data the contract describes.
- Extraction-induced contract breakage — when a module is extracted, in-process INotification semantics (in-order, in-transaction) may not hold over Redpanda. Mitigation: design event handlers to be idempotent from day one, even while in-process.


## Related ADRS
- 