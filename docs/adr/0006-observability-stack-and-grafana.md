# ADR 0006: Use an open-source observability stack

## Status

Accepted

## Date

2026-07-01

## Context

FleetPulse is composed of independently running brokers, workers, APIs, and storage services. Diagnosing issues across these components requires metrics for service health and throughput, distributed traces to follow requests and processing across boundaries, and centralized logs for investigation.

The system already runs locally with Docker Compose and instruments its .NET and Python services. The observability setup should work with this containerized topology, use open protocols where practical, and avoid requiring a hosted vendor account for local development and demonstrations.

## Decision

Use a self-hosted Grafana observability stack:

- **Prometheus** scrapes application and infrastructure metrics and evaluates metric-based alerting rules.
- **OpenTelemetry** instrumentation and the OpenTelemetry Collector receive and batch traces; the collector exports them to **Grafana Tempo**.
- **Promtail** discovers Docker containers and forwards their logs to **Grafana Loki**.
- **Grafana** provides the shared interface for metrics, traces, logs, dashboards, and alert status.

Use the stack primarily for local development and as a reference architecture. Treat retention, authentication, storage, availability, and access control as deployment-specific requirements before exposing it as a production service.

## Alternatives considered

### Datadog

Datadog provides a managed, integrated observability service and reduces the amount of infrastructure the team must operate. It introduces recurring vendor costs and service-specific configuration. It was not selected because this project emphasizes a self-hosted, open-source local environment and demonstration of the observability components.

### Separate tools for each signal

Choosing independent vendors or tools for metrics, traces, and logs could optimize each signal separately, but would increase integration and operational overhead and make cross-signal investigation less unified.

## Consequences

### Benefits

- The core stack is open source and can run locally alongside FleetPulse using Docker Compose; there is no observability vendor subscription required for that deployment.
- Prometheus, OpenTelemetry, Tempo, and Loki use widely adopted interfaces, allowing components and exporters to be changed independently if requirements evolve.
- Grafana provides a common place to query and visualize the three signals.
- Local telemetry remains within the developer's environment, subject to the configured storage and access controls.

### Trade-offs and safeguards

- “No vendor subscription” does not mean zero cost: compute, storage, backups, and engineering time are still required, especially in production.
- The team must configure, upgrade, secure, monitor, and troubleshoot multiple services and their data pipelines.
- Retention and persistence are finite and configuration-dependent. The current Compose configuration sets Prometheus retention to 15 days and Loki retention to 120 hours; production retention and backup policies must be chosen deliberately.
- The local Loki configuration disables authentication, and Promtail accesses the Docker socket and container log files. Keep these settings restricted to trusted local environments; production deployments need appropriately secured access and collection permissions.
- Self-hosting means the team owns availability, capacity planning, upgrades, and recovery. This stack does not by itself provide high availability or a complete production alert-response process.
- OpenTelemetry collection is configured for traces; it does not automatically make every service or operation observable. Instrumentation, useful dashboards, and alert rules still need to be maintained and validated.

## Implementation notes

The Compose configuration separates the metrics/logging stack from tracing in `docker-compose-obsv.yml` and `docker-compose-tracing.yml`, while the combined Compose setup includes all components. Prometheus scrapes Redpanda, EMQX, PostgreSQL exporter, the AI worker, the DB writer, SignalR hub, and selected observability services. Its current alert rules cover consumer lag, missing GPS pings, database flush latency, and connected SignalR clients.

## Related documentation

- [Observability overview](../observability.md)
- [Prometheus implementation](../prometheus.md)
