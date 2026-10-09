# ADR 0014: Use Module Federation for React micro-frontends

## Status

Accepted

## Date

2026-08-01

## Context

FleetPulse is composed of a React SPA and independently built frontend modules, including the React alert-management MFE. The host needs to load remote modules while sharing core React dependencies so that independently delivered UI capabilities can be integrated without combining all their source into one build.

The host and React remote use Vite, so the federation integration must support the existing Vite build and development workflow and provide the runtime capabilities required by the application.

## Decision

Use the Module Federation ecosystem's Vite integration, `@module-federation/vite`, for the React host and React remotes. Configure the remote's exposed modules and shared dependencies in its build configuration, and share React and React DOM as singletons with compatible required versions.

Use the matching enhanced runtime where runtime remote registration and loading are required. This ADR selects the Module Federation implementation for React MFEs; Angular's separate Native Federation setup is covered by the Angular MFE decisions.

## Alternatives considered

### `@originjs/vite-plugin-federation`

OriginJS provides Vite federation functionality and may be suitable for existing projects already using it. The project selected the Module Federation ecosystem to use its Vite plugin and enhanced runtime together. Avoid describing OriginJS generically as “legacy”; compare its current maintenance, feature set, and compatibility when this choice is revisited.

### Single combined frontend build

Bundling all frontend modules directly into the host would avoid runtime remote loading and federation configuration. It would also couple their build and release cycles and reduce the ability to deploy modules independently.

## Consequences

### Benefits

- React MFEs can be built and released separately from the host while exposing named modules for host consumption.
- Shared singleton React dependencies can avoid multiple React runtimes across host and remote, subject to compatible versions and correct runtime configuration.
- The Vite plugin fits the existing toolchain, and the enhanced runtime supports runtime remote loading where needed.

### Trade-offs and safeguards

- Federation introduces runtime coupling: remote availability, entry URLs, exposed-module names, and shared dependency compatibility are required for successful rendering.
- Plugin and runtime configuration adds build and deployment complexity. Test both local development and production builds, including remote loading and failure behavior.
- Shared singleton declarations do not by themselves guarantee compatibility. Keep host and remotes' React versions aligned and verify the generated federation metadata.
- Independently deployed remotes can fail after the host has loaded. Provide useful loading/error states and define versioning and rollback practices.

## Related ADRs

- [0015-angular-22-for-drivers-dashboard.md](0015-angular-22-for-drivers-dashboard.md)
- [0017-use-dynamic-loaders-for-mfe-modules.md](0017-use-dynamic-loaders-for-mfe-modules.md)
