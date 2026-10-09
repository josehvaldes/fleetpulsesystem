# ADR 0017: Resolve MFE remotes from runtime configuration

## Status

Accepted

## Date

2026-09-24

## Context

Micro-frontend entry URLs can differ between local, staging, and production deployments. Embedding deployment-specific remote URLs in the host's build configuration would require rebuilding the host when a remote moves or when the environment changes.

The React host integrates two kinds of remotes: the React alert MFE using Module Federation and the Angular driver MFE using Native Federation. Both need to be initialized with deployment-specific remote entry information before their modules are loaded.

## Decision

Load application configuration at startup from the host's public `/config/app-config.json` and resolve remote entry URLs from that runtime configuration. Initialize React remotes through `@module-federation/enhanced/runtime` and initialize Angular remotes through the Native Federation orchestrator with the configured remote map.

Keep build-time federation configuration for concerns that are inherently part of a remote's build, such as its name, exposed modules, and shared dependencies. This decision makes remote locations configurable at runtime; it does not make the remote's exposed API or federation format build-independent.

## Alternatives considered

### Embed environment-specific remotes in the host build

Build-time remote URLs are straightforward and can be validated during compilation. They require a separate host build for each deployment configuration and make changing a remote location dependent on rebuilding and redeploying the host.

### Discover all remotes from a federation manifest/service

A central manifest or registry could support more dynamic composition and remote discovery. It adds an additional service or publication contract; the current application uses a small runtime JSON configuration file because its set of remotes is known.

## Consequences

### Benefits

- Remote entry locations can be changed per deployment without rebuilding the React host, as long as the runtime config is updated and served correctly.
- The host can initialize both the React Module Federation remote and Angular Native Federation remote before rendering the corresponding feature.
- Build artifacts can be promoted between environments while deployment-specific URLs are supplied by the environment's configuration.

### Trade-offs and safeguards

- The host now depends on runtime configuration being available, valid, and loaded before federation initialization. Validate required fields and fail clearly when the file cannot be fetched or parsed.
- Runtime remote URLs are a trust boundary: serve configuration and remote assets from approved origins over HTTPS in production, apply appropriate content security policy/CORS settings, and do not allow untrusted parties to change remote locations.
- Runtime configuration does not ensure compatibility. Keep remote names, exposed module paths, federation type, shared dependency versions, and host integration contracts aligned across independently deployed artifacts.
- A remote may be unavailable or incompatible after the host has started. Provide user-visible loading and error states, monitor remote-load failures, and document rollback/versioning procedures.
- The current React wrapper uses `React.lazy` and `Suspense` for loading UI, but a thrown remote-load error still needs an error boundary for a graceful rendered fallback. The Angular wrapper currently logs load errors; it should also provide a visible failure state.
- Because the configuration is fetched at runtime, stale browser or CDN caching of `app-config.json` can preserve old remote URLs. Set explicit cache headers or version the config according to the deployment strategy.

## Implementation notes

The host's `appConfig` module fetches `/config/app-config.json`. The React federation initializer passes `react_remotes.alerts` to the enhanced runtime, while the Native Federation initializer passes `angular_remotes` to `initFederation`. The Vite federation plugin and Angular federation config still define remote metadata and exposed modules at build time.

## Related ADRs

- [0014-module-federation-over-originjs-for-MFE.md](0014-module-federation-over-originjs-for-MFE.md)
- [0015-angular-22-for-drivers-dashboard.md](0015-angular-22-for-drivers-dashboard.md)
