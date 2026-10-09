# ADR 0007: Use EMQX and Redpanda for telemetry ingestion and streaming

## Status

Accepted

## Date

2026-06-01

## Context

FleetPulse's simulator publishes GPS telemetry using MQTT, while backend consumers need a Kafka-compatible event stream they can consume independently. The system therefore needs both an MQTT broker at the ingestion edge and a streaming broker for fan-out to the AI worker, DB writer, and SignalR hub.

The local development environment is Docker Compose-based. Keeping the broker layer runnable locally and portable across hosting environments supports repeatable development and avoids making application code depend on one cloud provider's managed service API.

## Decision

Use **EMQX** as the MQTT ingress broker and **Redpanda** as the Kafka API-compatible streaming broker. Configure EMQX's rule/connector bridge to publish telemetry to Redpanda topics; backend services consume those topics using Kafka clients.

These components serve different responsibilities: EMQX handles MQTT client ingress, while Redpanda provides the downstream event log and consumer interface. This decision concerns the current local and demonstration architecture; production topology, durability, capacity, security, and support requirements must be evaluated separately.

## Alternatives considered

### Managed cloud streaming services

Azure Event Hubs and Amazon Managed Streaming for Apache Kafka (Amazon MSK) provide managed streaming options and reduce the amount of broker infrastructure the team operates. They are not a complete one-for-one replacement for this pair: MQTT device ingress and any required MQTT-to-stream integration would still need to be addressed. Managed pricing, operational requirements, regional availability, and compatibility should be compared against workload needs rather than assuming either option is inherently more expensive.

### Apache Kafka operated directly

Self-managing Apache Kafka would provide a familiar Kafka ecosystem, but requires operating and maintaining the broker cluster. Redpanda is selected for this project to provide Kafka API compatibility in the existing containerized setup.

## Consequences

### Benefits

- MQTT ingress and Kafka-compatible stream consumption are connected through an explicit bridge, matching the system's producer and consumer protocols.
- Local development can run the broker components with Docker Compose without requiring a cloud account.
- Backend consumers can scale and evolve independently behind Kafka topics, without consuming directly from the MQTT broker.
- Application integrations use MQTT and Kafka-compatible protocols rather than a provider-specific streaming API.

### Trade-offs and safeguards

- The team operates and upgrades multiple stateful broker components, including their storage, networking, monitoring, and recovery procedures.
- Self-hosting is not automatically free: compute, storage, licensing, maintenance, and engineering time still have costs. Confirm the applicable EMQX edition and license for each deployment.
- The current Compose topology uses a single Redpanda node in development mode and floating `latest` image tags in some broker services. It is a local/demo setup, not a production high-availability design; production should pin tested versions and define replication, backup, security, and upgrade policies.
- MQTT QoS 1 can result in redelivery, so downstream processing should tolerate duplicate events. Kafka consumer offsets and bridge behavior also need to be configured and monitored to meet the required delivery semantics.
- A provider-neutral protocol does not remove all provider dependencies: operational tooling, hosting, storage, and configuration still vary by deployment.

## Related ADRs
