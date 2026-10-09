# ADR 0011: Use Mediator for in-process request dispatch

## Status

Accepted

## Date

2026-07-15

## Context

The FleetPulse API needs to handle HTTP requests through application-level operations while keeping endpoint mapping separate from business use cases. A request/handler approach supports the CQRS style used for reads and writes, and provides a place for cross-cutting behavior such as validation.

The project needs an in-process mediator, not a distributed message bus: requests are dispatched within the API process, while Kafka remains responsible for cross-service event distribution.

## Decision

Use `Mediator` by martinothamar (`Mediator.Abstractions` and `Mediator.SourceGenerator`) for in-process request/handler dispatch in the API application layer. Register handlers from the application assembly, configure scoped service lifetime, and apply the validation pipeline behavior at the API boundary.

Keep this library choice separate from the broader CQRS decision: use request/handler separation where it improves organization, but do not require separate read/write models or additional abstractions for every endpoint without a use case.

## Alternatives considered

### MediatR

MediatR is a widely used in-process mediator with a mature ecosystem. At the time of this decision, its licensing/commercial direction was a consideration for this project. Reassess current licensing and terms before relying on that as a continuing differentiator.

### Wolverine

Wolverine provides in-process messaging as well as broader messaging and integration capabilities. Those capabilities may be useful in systems seeking a unified application and messaging framework, but exceed FleetPulse's current requirement for in-process dispatch and could overlap with the existing Kafka-based event architecture.

### Direct endpoint-to-service calls

Endpoints could call application services directly, reducing indirection. That approach remains reasonable for simple operations, but does not provide the same consistent request/handler boundary or centralized pipeline behavior as the selected pattern.

## Consequences

### Benefits

- HTTP endpoints can delegate use cases without depending directly on implementation services.
- Request handlers provide a consistent place for application behavior, while pipeline behaviors can apply cross-cutting validation.
- The selected package includes a source generator, reducing reliance on runtime reflection for mediator wiring and dispatch.
- The library is purpose-built for in-process mediator usage and keeps distributed messaging concerns in the existing broker layer.

### Trade-offs and safeguards

- Each request/handler and pipeline adds indirection, so the pattern can become ceremony for trivial operations. Use it where it clarifies application boundaries.
- Source generation introduces build-time tooling and generated-code diagnostics that developers must be able to inspect and troubleshoot.
- The library's APIs, maintenance, licensing, and performance characteristics can change. Review upstream status and terms during dependency upgrades rather than treating them as permanent guarantees.
- Mediator dispatch does not provide durable delivery, retries across process restarts, or inter-service messaging; use Kafka or another durable transport for those requirements.

## Related ADRs
