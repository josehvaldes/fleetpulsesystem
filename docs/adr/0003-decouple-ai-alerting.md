# ADR 0003: Decouple AI alert generation from browser delivery

## Status

Accepted

## Date

2026-06-01

## Context

FleetPulse ingests GPS pings into a Kafka-compatible stream consumed by several independent services. AI-based analysis is implemented in a Python worker, while browser clients receive real-time updates through the .NET SignalR hub. Connecting the AI worker directly to browser sessions would couple AI processing to frontend transport and client availability, and would make it harder for other services to act on the same alert.

The DB writer also needs to persist generated alerts. Alert events therefore need a backend-to-backend distribution mechanism that supports independent consumers as well as real-time browser delivery.

## Decision

Keep AI analysis and browser delivery as separate responsibilities. The Python AI worker consumes the `gps-pings` topic, evaluates telemetry for anomalies, enriches detected violations, and publishes alert events to a configurable Kafka topic. The DB writer consumes that alert topic for persistence and follow-up processing; the SignalR hub consumes it independently and pushes valid alerts to connected clients.

The SignalR hub is the real-time gateway to browser clients. The frontend receives the SignalR event contract and does not connect directly to the AI worker or Kafka broker. Keep the alert payload and topic configuration consistent across the producer and every consumer; use the Compose setting `KAFKA_ALERT_TOPIC=ai-alerts` for the current local topology.

## Alternatives considered

### Push directly from the AI worker to SignalR or the browser

This could reduce one broker hop, but would couple the Python worker to the SignalR transport, its endpoint and authentication model, and frontend availability. It would also leave the DB writer needing a separate delivery path and make replay or independent downstream consumption harder.

### Have the frontend consume the stream directly

This would expose broker connectivity and credentials to browser clients and duplicate stream-consumer responsibilities in the frontend. It is not suitable for the current trust boundary or client architecture.

### Combine analysis and delivery in one service

This would reduce the number of deployed processes, but couple model/runtime dependencies and scaling needs to the real-time gateway. It would also make failures in one responsibility more likely to affect the other.

## Consequences

### Benefits

- AI inference, persistence, and browser delivery can scale and fail independently.
- Multiple backend consumers can receive each alert from the stream without the AI worker knowing about their implementation details.
- The SignalR hub provides one controlled real-time path to browsers, keeping broker and AI implementation details out of the frontend.
- Brokered alert events can be observed and consumed by additional services without adding direct connections from the AI worker to each service.

### Trade-offs and safeguards

- Alert delivery adds broker configuration, consumer groups, topic monitoring, and at least one asynchronous hop. Monitor consumer lag and failures across producer and consumer services.
- Topic names and payload schemas are an integration contract. Validate compatibility and configure producer and consumers consistently; the Python setting defaults to `alerts` when unset, while the current Compose environment configures `ai-alerts` and the .NET consumers use that topic. A deployment that omits or overrides the environment setting inconsistently can silently split the pipeline.
- Kafka-compatible delivery can involve retries or redelivery. Consumers should be idempotent where side effects occur, and alert identifiers should support deduplication where required.
- Decoupling does not guarantee end-to-end delivery or ordering by itself. Configure broker durability, producer delivery handling, consumer offset/commit behavior, and recovery policies to match the system's requirements.
- The SignalR hub remains a real-time delivery dependency for connected browsers. Persisting alerts independently allows clients to recover current state through the API, but clients may miss a transient push while disconnected.

## Implementation notes

The current AI worker detects a driver crossing from inside to outside its assigned working-zone polygon; this differs from the earlier illustrative stationary-in-a-high-risk-zone example. It enriches the violation with an AI assessment before publishing the alert. In the local Compose configuration, the configured alert topic is `ai-alerts`; the Python code's fallback topic is `alerts` if the environment variable is not set. Keep these values aligned in each deployment.

## Related ADRs

- [0007-redpanda-over-azure-and-aws-stream-services.md](0007-redpanda-over-azure-and-aws-stream-services.md)
