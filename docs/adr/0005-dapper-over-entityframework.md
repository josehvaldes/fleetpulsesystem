# ADR 0005: Use Dapper for database access

## Status

Accepted

## Date

2026-06-15

## Context

FleetPulse uses PostgreSQL with the TimescaleDB extension. Its persistence workloads include high-volume GPS telemetry writes, updates to each driver's latest state, and time-series queries that use PostgreSQL- and TimescaleDB-specific SQL such as bulk inserts, upserts, and `time_bucket()` aggregations.

The DB batch writer buffers and flushes telemetry in batches, while API query paths need direct control over aggregation and filtering. These workloads favor explicit SQL and a small data-access layer over an object-oriented change-tracking model. The project also uses Npgsql as its PostgreSQL driver.

## Decision

Use Dapper with Npgsql for database access in the DB batch writer and API persistence/query paths. Keep SQL explicit and parameterized, and map query results to application models or DTOs. Use Npgsql's connection pooling (through a shared `NpgsqlDataSource`) rather than creating a separate connection pool per operation.

This decision selects a data-access approach; it does not prohibit using a different library for a separately justified use case. Revisit the choice if the persistence model or workload changes materially.

## Alternatives considered

### Entity Framework Core

EF Core provides change tracking, LINQ-based queries, and integrated schema-migration tooling. Those features are less valuable for the current batch-oriented writes and hand-tuned time-series queries, and can make provider-specific SQL and bulk operations less direct. It would also add ORM behavior and abstractions that the current persistence paths do not need.

### Direct Npgsql without Dapper

Using Npgsql alone would retain full SQL control and avoid an ORM, but would require more repetitive command and row-mapping code. Dapper provides lightweight parameter binding and result mapping while still using Npgsql underneath.

## Consequences

### Benefits

- SQL remains visible and can be tuned for TimescaleDB-specific operations and query plans.
- Batch inserts, upserts, and aggregations can be expressed directly without translating through an ORM query model.
- The data-access layer stays small and avoids change-tracking overhead for these workloads.
- Dapper uses parameterized commands and works on top of the existing Npgsql driver.

### Trade-offs and safeguards

- The application owns SQL correctness, query evolution, and coordination with schema changes; Dapper does not provide change tracking or schema migrations.
- Result mapping is less protected by compile-time model configuration than a fully mapped ORM. Keep DTOs aligned with query projections and cover important queries with tests.
- PostgreSQL- and TimescaleDB-specific SQL ties these persistence paths to the selected database. This is intentional for the current design, but reduces portability.
- Dapper does not guarantee that a query is fast. Measure representative workloads and inspect query plans before making performance claims or adding optimizations.
- Keep SQL parameterized, and use the shared `NpgsqlDataSource` so connection pooling and lifecycle management remain consistent.

## Related ADRs

- [0002-dual-table-strategy.md](0002-dual-table-strategy.md)