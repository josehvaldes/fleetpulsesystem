# ADR 0010: Write structured application logs to stdout

## Status

Accepted

## Date

2026-07-30

## Context

FleetPulse includes services written in .NET and Python and runs them as containers. Sending logs directly from each application to a particular storage backend would couple application configuration and runtime availability to that backend, and would require different integration choices for each language.

The container runtime already captures standard output and standard error. A collector can use that common boundary to forward logs to the selected log store, independently of application code.

## Decision

Applications should emit structured logs to the container's standard output (or standard error for runtimes that distinguish error output). The local Docker Compose observability setup uses Promtail to discover containers and forward their container logs to Grafana Loki.

This does **not** mean eliminating Serilog or all logging sinks. Serilog remains the .NET logging framework and is configured with a JSON console formatter; the Python services use structured logging with a JSON console handler. The decision is to avoid application-level Loki-specific transport as the default collection path. Any additional file or external sink must have an explicit operational requirement and avoid duplicate collection.

## Alternatives considered

### Application-level Loki or vendor-specific sinks

Direct sinks can send events to a backend without relying on container log collection and may offer backend-specific features. They couple application deployment and failure behavior to that backend, add per-language configuration, and can make migration or local setup harder.

### Unstructured console output

Plain text output is easy to inspect, but makes consistent parsing and field-based querying harder. Structured, preferably single-line JSON logs are selected so service, timestamp, severity, message, and correlation fields can be retained by downstream tooling.

## Consequences

### Benefits

- .NET and Python services share a transport and collection boundary while retaining language-appropriate logging frameworks.
- Applications do not need to know the Loki endpoint or implement Loki delivery and retry behavior.
- Console output works naturally with container runtimes and can be collected by other orchestrators or agents where configured.
- Structured logs can carry common fields and trace context, improving search and correlation when those fields are consistently emitted and retained.

### Trade-offs and safeguards

- The log collector and backend are additional services to deploy, secure, monitor, and maintain. Container-runtime capture alone does not provide centralized storage or querying.
- Logs can be lost or unavailable if the runtime's buffers, collector, or backend fail; define appropriate buffering, retention, and alerting for each deployment.
- Promtail's current Docker setup accesses the Docker socket and container log files. Restrict those permissions and the collector to trusted environments; use a deployment-appropriate collection agent and access controls in production.
- Keep output structured and single-line where possible, avoid logging secrets or unnecessary personal data, and standardize field names across languages. Multi-line exceptions and inconsistent schemas can be harder to search.
- Console output is not inherently faster than direct sinks in every workload. Validate throughput and backpressure behavior instead of relying on a general performance claim.
- Python logging can optionally also write to a rotating file. If enabled in a container, review rotation, storage lifetime, duplication, and whether that file is collected; stdout remains the default central collection path.

## Related ADRs

- [0006-observability-stack-and-grafana.md](0006-observability-stack-and-grafana.md)