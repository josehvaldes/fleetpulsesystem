# ADR 0002: Batch GPS Ingestion and Separate Current-State Storage

## Status

Accepted

## Date

2026-06-01

## Context

FleetPulse ingests frequent GPS telemetry from simulated vehicles through MQTT and Redpanda's Kafka-compatible streaming interface. Multiple independent consumers use the stream for different purposes, including real-time updates, AI-assisted alert detection, and persistence.

Persisting every GPS event as an individual database operation would increase database round trips and write overhead as ingestion volume grows. At the same time, the application has two distinct query needs:

1. Operational screens need the latest known position and status for each driver.
2. Historical and analytical views need time-stamped telemetry over a requested period.

Serving both needs from a single large historical dataset would make current-state queries unnecessarily dependent on the size and indexing of that dataset. The project therefore separates the write-optimized ingestion path from the read model used to retrieve the latest driver state.

The design also reduces redundant historical samples before persistence. This application-level reduction is distinct from TimescaleDB's own storage compression: reducing the number of samples changes the retained data, while database compression changes how retained data is stored.

## Decision

Use a **dual-table storage strategy in PostgreSQL with the TimescaleDB extension**, populated by a dedicated .NET background DB Writer.

### 1. Micro-batch incoming GPS events

The DB Writer consumes GPS events from the `gps-pings` Redpanda topic and accumulates them in an in-memory buffer. It flushes a batch when the configured time threshold is reached or the batch reaches its configured record limit.

The design documents a target of flushing every 5 seconds or at 1,000 records. These thresholds are configuration/workload assumptions and should be tuned using measured throughput, database latency, and memory usage.

Bulk writes reduce per-record database overhead compared with issuing a separate insert for every event.

### 2. Reduce redundant historical samples

Before writing historical telemetry, the writer applies the project's temporal-reduction rules. The intended behavior is to avoid storing repeated, low-information samples—for example, retaining representative points when a vehicle remains stopped and sampling moving vehicles at a lower cadence.

The current design notes describe retaining the first and last sample during a stop and down-sampling moving-vehicle history to approximately one point every 15 seconds. These rules intentionally trade some raw-sample fidelity for reduced storage and ingestion volume. They should be treated as domain-specific retention rules, not as lossless compression.

### 3. Store history and latest state separately

- **`gps_history` — TimescaleDB hypertable:** Stores the retained historical GPS samples with their event timestamps. It supports time-range queries and time-series analysis. TimescaleDB storage-compression policies may be applied to older data; the intended policy in the original design is to compress data older than seven days.
- **`driver_latest_state` — regular PostgreSQL table:** Stores the most recent known state for each driver. The writer updates the row using an UPSERT keyed by driver identity. This allows current-position queries to read a bounded current-state dataset instead of searching the full historical series.

The latest-state table is intended to contain at most one current row per driver represented in the system. If the application needs to distinguish active from inactive drivers, that status and the policy for removing or expiring stale drivers must be defined explicitly.

## Alternatives considered

### Insert every GPS event individually

**Rejected for the current workload.** One database operation per event creates avoidable round-trip and statement overhead. Individual inserts may still be appropriate at low volume or when per-event transactional handling is required.

### Use only the historical hypertable for all queries

**Rejected for the current access pattern.** The hypertable is appropriate for time-series history, but current-state reads would need to identify the latest row per driver from historical data. A separate latest-state table makes that read path explicit and predictable.

### Use only the latest-state table

**Rejected.** Overwriting a driver's row would discard the time-series history needed for playback, investigation, and analysis.

### Persist every raw sample without temporal reduction

**Not selected for the current storage target.** Keeping all raw samples preserves maximum fidelity but increases write and storage volume. If later use cases require exact raw telemetry—for example, forensic reconstruction—the project should revisit this trade-off or preserve a separate raw stream/archive.

## Consequences

### Benefits

- Bulk persistence reduces database operation overhead compared with one insert per GPS event.
- Historical writes and latest-state reads have separate storage models optimized for their respective purposes.
- Current-state queries do not need to scan the full history to find each driver's latest position.
- Temporal reduction and TimescaleDB storage compression can reduce storage requirements, though they operate at different stages and should be measured separately.
- The dedicated writer keeps database persistence decoupled from the real-time SignalR and AI-processing consumers.

### Trade-offs and risks

- **In-memory buffer loss:** Events held only in memory can be lost if the writer stops before flushing. Kafka/Redpanda offset-commit behavior, retries, and shutdown flushing must be configured to avoid acknowledging data before it is durably handled.
- **Duplicate processing:** Retries or replays can process an event more than once. Inserts and UPSERTs should be designed for idempotency where practical, using stable event identity and appropriate database constraints.
- **Reduced historical fidelity:** Temporal reduction intentionally discards samples. Sampling rules must be validated against map playback, alert investigation, and analytics requirements.
- **Freshness versus batching:** A longer flush interval reduces write frequency but delays persistence. The latest-state update latency should be measured against the application's operational needs.
- **Ordering and late events:** Out-of-order events can cause an older ping to overwrite a newer latest-state row unless the UPSERT checks event timestamps or another monotonic ordering rule.
- **Retention and compression are operational policies:** The seven-day TimescaleDB compression target is not guaranteed merely by choosing a hypertable; the required policy must be configured, verified, and monitored.
- **Buffer sizing:** The record threshold and flush interval must account for burst traffic and memory limits, including behavior when the database is unavailable.
- **Schema and index maintenance:** Both tables require appropriate keys and indexes. The hypertable's time partitioning and uniqueness constraints must follow TimescaleDB's requirements.

## Validation and follow-up

- Load-test ingestion at the expected event rate and during bursts.
- Measure batch size, flush duration, write latency, buffer occupancy, and database resource usage.
- Verify that a late or replayed event cannot replace a newer `driver_latest_state` row.
- Test graceful shutdown and recovery after a writer/database interruption.
- Confirm that the configured TimescaleDB compression policy matches the intended seven-day threshold.
- Validate temporal-reduction rules against the history and investigation scenarios supported by FleetPulse.

## Related ADRs

- ADR 0001: Use a React + Vite Single-Page Application.
- Future ADR: Event delivery guarantees, consumer offsets, and idempotent processing.
