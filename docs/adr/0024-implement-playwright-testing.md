# ADR 0024 Implement Playwright testing with MSW-based mocks for frontend modules

## Status: 
Proposed 

## Date: 
2026-10-28

## Context

The FleetPulse frontend is fragmented across independently built modules:
- A Vite + React 19 SPA acting as the host shell.
- An Alert MFE built in React.
- A Drivers MFE built in Angular.

These modules are served together and consume REST endpoints through a YARP reverse proxy. They also receive live telemetry and alert updates via a .NET SignalR hub exposed through the same proxy. Any frontend test strategy must cover all three modules consistently without duplicating setup per module.

We already use `@hey-api/openapi-ts` to generate typed clients from the OpenAPI spec, which gives us a contract-testing safety net independent of frontend tests. This means the suite does not need to re-verify contract conformance — it only needs to validate the behavior built on top of the contract (UI flows, MFE loading, rendering of pushed events, error handling).

A full end-to-end integration tier (real API + TimescaleDB via `WebApplicationFactory` and TestContainers) was evaluated and is documented under *Alternatives*. It is **deferred** — see *Consequences* for the resulting testing gap and re-evaluation triggers.

The constraints that shape this decision are:
- Fast, deterministic feedback is required on every PR; long, flaky jobs discourage contributors.
- Mocks and typed clients should derive from the same OpenAPI spec so contract changes propagate without manual sync.
- WebSocket/SignalR traffic is part of the user-visible behavior and must be mockable, not just HTTP.
- The streaming layer (EMQX → Redpanda → DB Writer → SignalR hub) is already exercised by backend integration tests; the frontend mock suite should not duplicate that coverage.

## Decision

Adopt Playwright as the frontend test framework, using MSW (Mock Service Worker) as the mock layer. Tests run entirely against the frontend in mockup mode; no backend containers or in-process API are started for these tests.

### Mock layer

- The SPA and both MFE modules run in mockup mode. MSW intercepts HTTP traffic at the browser service-worker layer, so the React host, React Alert MFE, and Angular Drivers MFE consume the same mocks without per-app wiring.
- MSW handlers and mock data are generated from the same OpenAPI spec used by `@hey-api/openapi-ts`, so mocks stay aligned with the contract by construction. Drift risk is bounded to fields the spec does not describe (e.g. example values for telemetry payloads), which are maintained as explicit fixture overrides.
- Mock data generation uses MSW-compatible plugins / fixture tooling — e.g. an OpenAPI-driven handler generator such as `orval`, `msw-auto-mock`, or `@anatolio/openapi-msw` for handler scaffolding, combined with `@mswjs/data` where relational mock state is needed — so handlers and example payloads are derived from the spec rather than hand-written.

### Real-time layer

- SignalR/WebSocket traffic is mocked with Playwright's native `page.routeWebSocket()` API (v1.49+) to script telemetry and alert pushes against the frontend. This covers the user-visible behavior of pushed events without requiring the real SignalR hub.

### Scope

- Navigation, MFE loading and routing, form validation, error states, alert acknowledgement flows, and rendering of pushed SignalR events.

### CI integration

- Runs on every push and PR.
- Suite is fast and deterministic; no Docker dependency, no database seeding, no streaming broker.

## Alternatives

### A1 — Layered strategy: MSW-based suite + integration E2E via `WebApplicationFactory` + TimescaleDB TestContainer
Considered and **deferred**. The integration tier would exercise the real Dapper/hypertable paths, YARP routing, and the real SignalR hub. It was originally part of this ADR but is dropped from scope for now because:
- `WebApplicationFactory` defaults to `TestServer`, which is in-memory only and does not expose a TCP port; Playwright (an external browser process) cannot reach it. Switching to a Kestrel-bound port is feasible but adds setup complexity that is not justified at the current stage.
- Critical user journeys are few and currently validated manually during demo recording; an automated integration tier is not yet cost-effective.
- The streaming layer (EMQX → Redpanda → DB Writer → SignalR hub) would still require additional TestContainers (Redpanda, possibly EMQX) for full coverage, compounding runtime and flakiness.

Re-evaluate when: critical user journey count grows, contract tests stop catching meaningful drift, or production-incident root causes trace back to integration paths not covered by mock tests.

### A2 — TestContainers for full API + DB stack
Rejected. Slow CI, requires building and maintaining API images for tests, and the `WebApplicationFactory` approach (A1) gives equivalent integration coverage at lower cost when integration testing is eventually added.

### A3 — Custom mock API container returning canned responses
Rejected. Would require maintaining a parallel API implementation, combining the drift risk of MSW mocks with the operational cost of TestContainers and none of the benefits of either. If a standalone mock server is ever needed (e.g. for manual QA or other consumers), prefer WireMock or Mockoon configured from the OpenAPI spec — no custom API code to maintain.

### A4 — MSW without Playwright (unit/component tests via Vitest/Jest only)
Rejected as the complete strategy. Component-level tests do not catch cross-MFE integration issues (host shell mounting Alert and Drivers MFEs), browser-level concerns (routing, service worker registration, WebSocket connections), or end-to-end UI flows. Playwright complements rather than replaces the existing Vitest/Jest suites.

## Consequences

### Positive
- Fast, deterministic frontend test feedback on every PR with no Docker dependency.
- Mocks and typed clients both derive from the OpenAPI spec, so contract changes propagate automatically; drift risk is bounded to WebSocket message shapes and example payload values.
- Single mock layer covers all three frontend modules (React host, React Alert, Angular Drivers) without per-app wiring.

### Negative
- No automated coverage for integration paths: real Dapper/hypertable queries, real YARP routing, real SignalR hub behavior. Bugs in those layers will only be caught by backend integration tests or manual demo runs.
- WebSocket mocking via `page.routeWebSocket()` is hand-rolled; message-shape drift between the mock and the real hub payload format is not caught automatically by tooling.
- Mock fixtures must be maintained for fields the OpenAPI spec does not describe (e.g. realistic example values for telemetry pings, alert payloads).
- The YARP reverse proxy is not exercised; routing bugs at the gateway are not caught.

### Operational
- CI needs one Playwright job (MSW-based) triggered on PR and push.
- An OpenAPI → MSW handler generation step must be wired into the frontend build pipeline so mocks regenerate when the spec changes.
- WebSocket mock fixtures for SignalR message shapes should be co-located with the backend contract definitions to minimize drift (or generated from the hub's payload schema if/when one is published).

## Related ADRs

- ADR 0023: Adopt OpenAPI + `@hey-api/openapi-ts` for Frontend-Backend Contract. This ADR's mocks derive from the same OpenAPI spec consumed by the generated clients.
