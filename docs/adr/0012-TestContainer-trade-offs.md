# ADR 0012: Use Testcontainers for database integration tests

## Status

Accepted

## Date

2026-08-01

## Context

FleetPulse's API and DB writer rely on PostgreSQL/TimescaleDB behavior, SQL schema, and database mappings. Testing only isolated methods or substituting a mock database cannot verify that HTTP requests, application logic, SQL, and the schema work together against the database engine used by the application.

Integration tests need a known, disposable database that can be initialized from the repository's schema and isolated from developer or shared environment data. API tests also need to exercise HTTP behavior without requiring production authentication configuration.

## Decision

Use Testcontainers for .NET with xUnit to provision PostgreSQL/TimescaleDB containers for database integration tests. Initialize the container from the shared `db/init.sql` schema, inject its connection string into the system under test, and use xUnit collection fixtures to share container setup among tests in a collection. Use `WebApplicationFactory` and the test authentication handler for API-level tests.

Keep unit tests for domain and application logic that do not require database or HTTP integration. The Testcontainers dependency is for integration tests; it does not make Docker a runtime dependency of the application.

## Alternatives considered

### Mock repositories or database services

Mocks are fast and useful for isolated behavior, but cannot validate SQL, schema compatibility, database-specific features, or the real persistence integration. They remain appropriate for unit tests, not as the only coverage for database-backed behavior.

### In-memory or alternative database provider

An in-memory substitute can reduce setup time, but may not reproduce PostgreSQL and TimescaleDB semantics. It risks passing tests that fail against the production database engine.

### Shared developer or CI database

A pre-provisioned database avoids container startup for each test environment, but adds provisioning, cleanup, isolation, credentials, and concurrent-test coordination requirements. It may also allow tests to interfere with shared data if isolation is incomplete.

## Consequences

### Benefits

- Tests exercise real PostgreSQL/TimescaleDB behavior with a controlled schema and disposable database state.
- The API integration suite can test HTTP status codes, serialization, authentication behavior, queries, and persistence together.
- Container lifecycle and schema setup are automated through fixtures, allowing tests to run with the normal test command when a compatible container runtime is available.
- Test authentication avoids weakening or special-casing production authentication code just to enable integration tests.

### Trade-offs and safeguards

- Starting database containers increases test duration and requires Docker or another compatible container runtime on the developer or CI host. Keep unit tests independently runnable and use fixtures to avoid unnecessary repeated startup.
- Tests can become order-dependent or contaminate each other if data is not reset or uniquely seeded. Make cleanup and test isolation explicit, particularly when sharing a container fixture.
- Container image tags must be pinned to a tested PostgreSQL/TimescaleDB version for reproducibility. The current fixtures use `timescale/timescaledb:latest-pg18`, which is not fully pinned; replace it with a fixed version or digest before relying on stable CI results.
- Containerized tests still require CI agents with the necessary runtime and permissions. Validate this prerequisite in each supported pipeline rather than assuming every hosted environment is configured for it.
- Integration tests do not replace unit tests for domain rules or application logic that can be tested without infrastructure.

## Implementation notes

The API and DB writer test projects reference `Testcontainers.PostgreSql` and xUnit v3. Their fixtures start a TimescaleDB container and load the shared `db/init.sql` script. The API fixture also creates a `WebApplicationFactory` with the container connection string and a test authentication scheme. Continue verifying that each integration-test class actually uses the shared fixture/collection so container reuse matches the intended lifecycle.

## Related ADRs
