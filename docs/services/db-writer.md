
# DbBatchWriterWorker

```
┌─────────────────────────────────────────────────────────────────┐
│                    DbBatchWriterWorker                          │
│                                                                 │
│  ┌─────────────────────┐     ┌──────────────────────────────┐   │
│  │ RedpandaConsumer    │     │  Flush Loop (every 5s)       │   │
│  │ Service             │     │                              │   │
│  │                     │     │  1. GetBatchedPings()        │   │
│  │  Consume() ────────►│     │  2. Compress (TODO)          │   │
│  │       │             │     │  3. BulkInsert (TODO)        │   │
│  │       ▼             │     │  4. UpsertLatest (TODO)      │   │
│  │  ┌──────────────┐   │     │  5. ClearBatch()             │   │
│  │  │  Concurrent  │   │     └──────────────────────────────┘   │
│  │  │  Bag<GpsPing>│   │                                        │
│  │  │  (Buffer)    │   │                                        │
│  │  └──────────────┘   │                                        │
│  └─────────────────────┘                                        │
└─────────────────────────────────────────────────────────────────┘
```


## Alert Management

### Postgresql persistance
 * 

### Post-persistance work with Hangfire
 * Hangfire for background jobs and alert scalation
    - EscaleteAlerts
    - CleanupAlerts



### Design Decisions

| Decision | Rationale |
| :--- | :--- |
| `BackgroundService` for Kafka consumption | Ties the consumer lifecycle to the ASP.NET host; clean startup/shutdown with `CancellationToken` propagation. Consumer runs on a `Task.Run` thread-pool thread so it does not block app startup. |
| Minimal API instead of MVC controllers | Reduces ceremony for a thin read-only query layer; keeps the service boundary explicit. |
| Mapster for DTO → Response mapping | Zero-reflection mapping configuration with compile-time safety; registered once at startup via `MappingConfig.RegisterMappings()`. |
| Dapper over EF Core | TimescaleDB queries are hand-crafted SQL (time-series aggregations). Dapper gives full SQL control without the overhead of an ORM change-tracker. |
| `NpgsqlDataSource` singleton | Handles connection pooling internally; a single `NpgsqlDataSource` is the recommended usage pattern for Npgsql 7+. |
| Global exception handler (`IExceptionHandler`) | Centralises ProblemDetails error responses; maps `ValidationException` → 422 and `UnauthorizedAccessException` → 401, everything else → 500. |
| Per-driver throttle (2 Hz) in-process | Avoids saturating the SignalR backplane and the browser event loop when GPS simulators produce bursts. A sliding-window `Dictionary<string, DateTimeOffset>` is sufficient for a single-instance deployment. |

