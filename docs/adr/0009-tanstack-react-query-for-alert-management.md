

# ADR 0009: Use TanStack Query for remote frontend state

## Status

Accepted

## Date

2026-07-30

## Context

The frontend reads server-owned data such as paginated alerts and needs a consistent way to manage request status, errors, caching, retries, and refreshes. Keeping those concerns in component-local state or a hand-built context would require each feature to recreate request lifecycle and cache behavior.

FleetPulse is a real-time application, but the queried datasets are bounded and the application can request fresh data from its API. Offline access to cached alerts or drivers is not a requirement. Server-state caching should therefore improve in-session UX without becoming a durable offline data store.

## Decision

Use `@tanstack/react-query` for asynchronous server state in the React frontend. Provide a shared `QueryClient` at the application root and define feature query hooks with keys that include all parameters affecting the result, such as alert filters and pagination.

Keep React Query's cache in memory; do not add `@tanstack/react-query-persist-client` or `@tanstack/query-async-storage-persister` unless offline or durable query-cache requirements are introduced. This decision concerns persistence of the React Query cache only; other application state may have its own persistence policy.

Use mutations for server-side changes and invalidate or update the corresponding query keys after success. Coordinate with SignalR-driven updates where live changes should be reflected without waiting for the normal query refresh cycle.

## Alternatives considered

### Native component state or React Context

These are suitable for local UI state and simple shared values, but do not provide a complete server-state lifecycle or query cache. Implementing deduplication, retries, cache freshness, and invalidation independently would add custom code to feature components.

### Redux for all server data

Redux is already used for application state and can represent remote data. Using it for every request would require project-specific conventions and middleware for request status, deduplication, caching, and invalidation; TanStack Query provides a purpose-built model for server state while Redux can remain responsible for client/application state.

### Persist the query cache

Persisted query caching can help applications that require offline startup or durable cache restoration. Those requirements are outside FleetPulse's current scope and would add storage lifecycle, stale-data, and invalidation concerns.

## Consequences

### Benefits

- Query hooks centralize loading, error, retry, and cache behavior for server-owned data.
- Parameterized query keys support separate cached results for different alert pages and filters.
- Mutations can coordinate server changes with affected cached queries rather than requiring manual fetch state in each component.
- Keeping this cache in memory avoids introducing a persistent offline copy of server data.

### Trade-offs and safeguards

- Cache freshness is a product decision: overly long stale times can display old data, while aggressive refetching can increase API load. Tune defaults to the needs of each query and real-time flow.
- Query keys must be stable and include every input that changes the result; mutation invalidation must target those same keys.
- Query cache data is transient and should not be treated as durable state or an offline guarantee.
- The current `useAlerts` hook queries paginated alerts, while `useAlertActions` currently simulates its mutation instead of calling a server endpoint. Its invalidation key should be aligned with the alert-list query keys when the real API mutation is implemented; the intended mutation behavior is not yet fully wired.
- The application also uses Redux persistence for selected Redux state. That is separate from, and does not contradict, the decision not to persist TanStack Query's cache.

## Related ADRs
