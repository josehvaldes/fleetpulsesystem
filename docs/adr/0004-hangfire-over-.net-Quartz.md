# ADR 0004: Use Hangfire for background jobs

## Status

Accepted

## Date

2026-08-01

## Context

The DB writer consumes alert events and persists them to PostgreSQL. After persistence, it must dispatch standard alert processing and schedule a delayed check for high-risk alerts whose `auto_escalate` flag is enabled. A recurring cleanup job is also required. These jobs should run outside the Kafka message-handling path so that alert consumption does not wait for notification or escalation work.

The worker is an ASP.NET Core/.NET hosted service and PostgreSQL is already part of the persistence architecture. Background job state must survive process restarts; an in-memory timer or queue alone would not meet that requirement.

## Decision

Use Hangfire in the DB writer for enqueued, delayed, and recurring background jobs, with Hangfire's PostgreSQL storage provider. Route alert work through named queues so escalation and standard alert processing can be assigned separate worker capacity.

Jobs must re-read the alert's current state before acting. In particular, a delayed escalation should proceed only if the alert still exists, remains open, and is eligible for escalation. Job handlers should be safe to retry and should avoid duplicate side effects.

## Alternatives considered

### Quartz.NET

Quartz.NET supports scheduled and recurring jobs, persistent job stores, and clustering. It is a viable alternative, but would introduce a separate scheduling abstraction and configuration model for this worker. Hangfire's enqueue/schedule APIs and PostgreSQL storage fit the current job-dispatch flow directly.

### In-process timers or hosted-service loops

These options have a small runtime footprint, but scheduled work would be lost when the process stops unless additional persistence and recovery logic were built. They are not selected for durable alert scheduling.

## Consequences

### Benefits

- Delayed, queued, and recurring work uses one job framework and can be persisted in PostgreSQL.
- Alert jobs are dispatched separately from Kafka consumption, keeping message handling focused on validation, persistence, and dispatch.
- Named queues allow worker allocation to be configured for escalation and standard processing independently.
- Jobs can be retried after transient failures, provided handlers are idempotent and side effects are guarded.

### Trade-offs and safeguards

- Hangfire adds package, schema, server, and operational configuration, and its storage adds load to PostgreSQL. Monitor job backlog, failures, and database capacity.
- Persisting the alert and creating its Hangfire job are separate operations. A process failure between them can leave a saved alert without scheduled work. If this gap is unacceptable, use a transactional outbox or another atomic dispatch design.
- Retries and concurrent workers can execute a job more than once. Keep handlers idempotent, re-check alert status at execution time, and make external notification/escalation operations deduplicable.
- Queue assignment alone does not guarantee a particular completion time. Configure worker capacity and monitor queue latency against the escalation service-level objective.
- PostgreSQL storage couples job durability and availability to the database. Database maintenance or outages can delay job execution.

## Implementation notes

The current DB writer configures Hangfire PostgreSQL storage and separate worker registrations for the `escalation-alerts` and `standard-alerts` queues. It schedules escalation after a 10-second delay for high-risk alerts with `auto_escalate` enabled and enqueues standard alert processing. The recurring cleanup processor is registered hourly.

The escalation and standard alert handlers currently contain placeholder business logic. The escalation job checks the alert state before proceeding, but the actual escalation side effect still needs implementation; this ADR records the framework decision, not a claim that the end-to-end notification flow is complete.

## Related ADRs
