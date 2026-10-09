# ADR 0008: Organize frontend code around product features

## Status

Accepted

## Date

2026-07-01

## Context

The FleetPulse frontend contains distinct product capabilities, including alert management, driver views, fleet mapping, and login. As the SPA and its federated modules grow, placing all code into broad technical folders can make a feature's UI, state, and behavior harder to locate and change together.

The codebase also has genuinely shared concerns, such as application setup, routing, services, types, and reusable components. The organization should improve feature ownership without forcing every module into a rigid structure or duplicating shared infrastructure.

## Decision

Organize feature-specific frontend code using **Feature-Sliced Design (FSD) principles**: keep a feature's components, hooks, and related behavior together under a named feature area, and expose intentional entry points for consumers. Keep cross-cutting application infrastructure and genuinely reusable code in shared or app-level areas.

Apply the approach pragmatically rather than treating it as a requirement to reproduce every formal FSD layer. Keep dependency direction understandable, avoid circular feature dependencies, and use stable public exports when one feature is consumed by another or by the application shell.

## Alternatives considered

### Traditional technical-layer organization

Grouping all components, hooks, services, and state into separate global folders is familiar and can make shared technical utilities easy to find. As feature count grows, however, a single feature's implementation becomes scattered across those folders, and unrelated feature changes are more likely to touch the same large directories.

### One folder per feature without shared conventions

This is simple to start with, but without conventions for shared code and dependencies, feature folders can develop inconsistent structures or import each other's internals directly. FSD principles provide lightweight guidance while allowing the repository to retain existing app-level organization where appropriate.

## Consequences

### Benefits

- Related UI, hooks, and behavior for a capability are easier to discover and maintain together.
- Feature ownership can reduce accidental coupling between unrelated product areas.
- The structure can support incremental development and align with the project's independently composed frontend modules.
- Shared code remains explicit instead of being copied into multiple features.

### Trade-offs and safeguards

- Feature boundaries and public APIs require team judgment; overly small slices or excessive barrel files can add indirection without improving cohesion.
- Shared functionality can become a dependency bottleneck if feature-specific behavior is moved into generic folders too early.
- Cross-feature imports can create cycles or hidden coupling. Prefer stable public exports and review dependency direction as features evolve.
- The current repository is a pragmatic hybrid, with feature areas alongside app-level `components`, `services`, `store`, `types`, and other folders. New code should follow the intent of this decision without implying that every existing folder already conforms to a strict FSD taxonomy.

## Related ADRs
