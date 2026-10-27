# ADR 0023: Adopt OpenAPI + openapi-typescript for Frontend-Backend Contract Enforcement

## Status:
Proposed

## Date:
2026-10-28

## Context

During the development of FleetPulse, a recurring integration failure was observed: changes to the .NET `fleetpulse-api` response shapes (renamed DTO fields, removed properties, type changes) silently broke the React SPA and the Angular/React micro-frontends at runtime. The breakages were only discovered when a developer or QA manually exercised the affected UI flow, often after the breaking change had already merged. The backend's Testcontainers-based integration tests verified that the API behaved correctly in isolation, but no mechanism verified that the frontend's expectations still matched the backend's actual output across the YARP gateway.

The root cause is structural: the frontend consumes hand-written `fetch` calls and hand-maintained TypeScript interfaces for API responses. These types drift independently from the .NET DTOs that produce them. There is no compile-time or CI-time enforcement that the two sides agree on the shape of the data crossing the network boundary. The two codebases are deployed independently (frontend via Cloudflare Pages, backend via Docker Compose locally and ECS/ACA in production) and share no compile-time contract.

This is, at its core, a contract problem between two independently-deployed codebases. We need a mechanism that fails the build when the contract drifts, before the breaking change reaches the frontend pipeline — not after.

## Decision

Adopt **OpenAPI schema generation on the backend** combined with **`openapi-typescript`** on the frontend as the structural contract enforcement mechanism between `fleetpulse-api` and the frontend applications (React SPA, React alert MFE, Angular drivers MFE).

Concretely:

1. The `fleetpulse-api` .NET 10 project emits its `openapi.json` document using the built-in ASP.NET Core OpenAPI support (Microsoft's `Microsoft.AspNetCore.OpenApi` package, no Swashbuckle required for .NET 10).
2. A build step in the API project (or a dedicated CI job) publishes the generated `openapi.json` as a build artifact alongside the API image.
3. The frontend monorepo consumes the published `openapi.json` and runs `openapi-typescript` to generate typed schema files into a known location (e.g., `frontend/src/generated/api-schema.ts`).
4. Frontend code imports types from the generated file instead of declaring hand-written interfaces for API responses. Existing `fetch` wrappers are refactored to be generic over the generated types; new thin client functions replace ad-hoc calls over time.
5. CI runs a verification step that regenerates types from the latest committed `openapi.json` and fails the build if the generated file differs from what is committed. This catches the "developer changed the DTO but forgot to regenerate" failure mode.

**Scope:** This ADR covers the REST endpoints exposed by `fleetpulse-api` (drivers, alerts, and any future REST surface). It does **not** cover SignalR message shapes, MQTT payloads, or Kafka event schemas — those cross different transports and will be addressed in separate ADRs if needed.

## Alternatives

**Pact (consumer-driven contract testing)**
- Pros: True behavioral contracts — verifies actual request/response cycles including status codes, headers, and payloads, not just types. Works across HTTP and message boundaries. Stronger guarantee than schema-only checks.
- Cons: Higher setup cost; requires writing consumer tests in the frontend and provider tests in the backend, plus operating a Pact broker for contract exchange. Overkill for the common failure mode of "I renamed a field and the FE silently broke".
- Decision: Defer to a future ADR for the 5–10 most critical endpoints (driver latest state, alert list, live SignalR message shapes) where behavioral verification adds value beyond structural type safety.

**Hand-written TypeScript types with manual code review**
- Pros: Zero tooling cost; no new dependencies.
- Cons: This is the status quo and the source of the problem. Relies on developers remembering to update both sides and on reviewers catching drift manually. Has already failed multiple times in practice.

**NSwag / Swagger Codegen for full client generation**
- Pros: Generates a complete HTTP client, not just types — fewer hand-written fetch wrappers.
- Cons: Heavier output; opinionated client structure that fights the existing fetch-based code in the React SPA. `openapi-typescript` produces only types, which is lighter, less intrusive, and lets us keep our own thin fetch wrappers and error-handling patterns.


## Consequences

**Benefits:**
- Compile-time type safety on the frontend: a renamed .NET DTO field becomes a TypeScript compile error in CI, blocking the breaking change before it merges — not after QA catches it.
- Single source of truth: the .NET DTOs are the contract; the frontend cannot drift from them by accident.
- Low operational overhead: no additional services to operate (unlike a Pact broker); just a build step on the API side and an `openapi-typescript` npm devDependency on the frontend.
- Better developer experience: frontend developers get IDE autocomplete and inline documentation for API responses, derived from .NET XML doc comments when emitted.
- Foundation for future tooling: once types are flowing from the schema, adopting a generated HTTP client (`openapi-fetch`, `@hey-api/openapi-ts`) becomes a small incremental step rather than a rewrite.

**Trade-offs:**
- The frontend now has a build-time dependency on the backend's `openapi.json` artifact. CI ordering must ensure the API build runs (and publishes its schema) before the frontend type-regeneration step; otherwise the frontend must consume a pinned `openapi.json` from the last merged API build.
- Generated type files are large and not human-readable. They should be committed (so diffs are reviewable in PRs) but treated as build output — reviewers will need to learn to read `openapi-typescript` output and resist the urge to hand-edit it.
- The contract enforced is **structural** (types and shapes), not **behavioral**. The frontend can still misinterpret a field's semantics, and the backend can return a structurally-valid-but-semantically-wrong value. OpenAPI does not catch "the API returns 200 OK with the wrong data" — that remains a job for integration tests to be implemented in future ADRs.

**Risks:**
- If developers skip the local regeneration step and commit hand-edited generated files, contract enforcement weakens. Mitigated by the CI verification step that regenerates and diffs against the committed file.
- If the .NET project does not annotate DTOs with `[Description]` attributes or XML doc comments, the generated types carry no documentation and the IDE experience is diminished. Mitigated by adopting DTO annotation conventions as part of the rollout.
- OpenAPI does not capture runtime-only contracts (conditional fields, enum values defined in code rather than schema, header-driven variants). The team must be disciplined about representing all meaningful contracts in the schema; otherwise structural safety is incomplete.
- `openapi-typescript` is a single community library; if it is abandoned, migration effort is non-trivial. Mitigated by the fact that the generated output is plain TypeScript types, portable to any other generator with a small adapter.

## Related ADRs
