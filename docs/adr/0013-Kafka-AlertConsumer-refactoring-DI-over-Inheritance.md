# ADR 0013: Create Kafka consumers through a factory and share common helpers

## Status

Accepted

## Date

2026-08-15

## Context

The DB writer has separate Kafka consumers for GPS pings and alerts. They share consumer construction, baseline Kafka configuration, and handling of noisy Kafka client log callbacks, but have different processing behavior and offset requirements. Creating Kafka clients directly in each consumer duplicated setup and made the construction boundary difficult to replace in tests.

The design needs to centralize shared infrastructure without forcing the two consumers to share their message-processing loops or policy-specific configuration.

## Decision

Use dependency-injected composition for Kafka consumer creation through `IKafkaConsumerFactory`. The concrete factory builds the Confluent Kafka consumer and attaches the supplied log and error callbacks.

Retain a small abstract `KafkaConsumer` base class for shared, stateless helpers: constructing the baseline `ConsumerConfig` from settings and mapping Kafka client log messages through the common throttling behavior. Keep subscription, message handling, offset policy, metrics, and lifecycle in the concrete alert and GPS consumers. Consumers may override the baseline configuration where their delivery semantics require it.

## Alternatives considered

### Duplicate consumer construction and configuration

Each consumer could use `ConsumerBuilder` directly and configure its own callbacks. This keeps each class standalone, but duplicates setup and makes future changes harder to apply consistently.

### Composition for all common behavior

A dedicated configuration provider and logging adapter could replace the base class entirely. That would avoid inheritance, but would introduce additional abstractions for a small set of protected stateless helpers. Revisit this if the common behavior grows or requires independent lifecycle/testing.

### Put message-processing behavior in the base class

A template-method base class could own the consume loop and expose hooks for each message type. This might reduce duplicated loop code, but risks hiding important differences in offset handling, buffering, and failure policy. The current decision deliberately shares only setup helpers and leaves the loops explicit.

## Consequences

### Benefits

- Kafka client creation is isolated behind an interface and can be substituted or mocked in tests.
- Shared builder callback setup is centralized in one factory implementation.
- Common defaults and log filtering are reused without conflating the consumers' distinct responsibilities.
- Consumer-specific configuration and processing remain visible in each consumer class.

### Trade-offs and safeguards

- The base class creates an inheritance dependency and exposes shared behavior through protected members. Keep it small and stateless; move helpers to composed services if consumers need different implementations or the base begins accumulating policy.
- The factory abstraction is useful for isolation, but should not grow into a general-purpose Kafka framework without concrete needs.
- Baseline configuration can be overridden by a concrete consumer. Document and test policy differences such as auto-commit/manual commit so configuration changes do not silently alter delivery behavior.
- A factory seam makes construction replaceable but does not itself guarantee test coverage or reliable delivery. Test each consumer's subscription, offset, processing, and shutdown behavior separately.

## Implementation notes

`KafkaConsumerFactory` builds `IConsumer<string, string>` instances and attaches log/error handlers. `AlertConsumer` and `GpsPingConsumer` both derive from `KafkaConsumer` and use its configuration/logging helpers, while maintaining separate consume loops. The alert consumer enables auto-commit; the GPS consumer uses the base manual-commit default and commits after processing/buffering a message.

## Related ADRs
