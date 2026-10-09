# ADR 0001: Use a React + Vite Single-Page Application

## Status

Accepted

## Date

2026-06-01

## Context

FleetPulse is an operational control-tower application for monitoring simulated delivery vehicles. Its primary interface is highly interactive: it displays driver locations on a live map, updates operational views from a continuous telemetry stream, and presents alerts and driver-management screens.

The application receives real-time updates from the backend through SignalR over a persistent WebSocket connection. The browser must maintain client-side state and update the relevant views as new events arrive. The main application is also deployed as static frontend assets, with backend HTTP and SignalR traffic handled separately.

The primary frontend requirements are therefore:

- Interactive, continuously updated views for live telemetry.
- Client-side handling of SignalR events and UI state.
- Straightforward local development and fast feedback when tuning maps and dashboards.
- Static hosting and deployment independent of backend services.
- A path to compose independently developed frontend modules as the project grows.

Server-side rendering (SSR) is not a primary requirement for this authenticated, operational interface. The decision is based on the application's workload and deployment model, not on a claim that SSR or Next.js cannot support real-time applications.

## Decision

Use **React with Vite to build the host Single-Page Application (SPA)**.

The host loads the application in the browser, maintains the interactive UI state, and connects to backend HTTP endpoints and the SignalR hub for data. The built frontend can be deployed as static assets through a CDN or static hosting platform such as Cloudflare Pages.

Frontend modules may use different frameworks where there is a clear reason to do so. The host SPA decision does not require every independently deployed module to use React; module integration and dependency-sharing decisions are handled separately.

## Alternatives considered

### Next.js

Next.js supports client-side interactivity and real-time connections, so it is technically capable of implementing this application. However, its SSR and server-component capabilities do not provide a clear benefit for the core live-telemetry screen, where the browser must continue processing events after the initial page load.

Using Next.js would introduce server-rendering and/or server-runtime choices that are not required by the current product. WebSocket support also depends on the hosting and deployment architecture: some serverless platforms impose connection-duration or runtime constraints, but those constraints are not inherent to Next.js itself.

Next.js remains a reasonable option if future requirements create a material need for SSR, SEO, server-rendered public pages, or framework-integrated server functionality.

### Other SPA toolchains

Other React-compatible build tools could also satisfy the requirements. Vite was selected for its direct SPA development workflow and fast development feedback, including Hot Module Replacement (HMR).

## Consequences

### Benefits

- Real-time event handling and view updates remain in the browser, close to the UI state they affect.
- Static hosting separates frontend delivery from backend service execution and avoids requiring a frontend server runtime for the host application.
- Vite provides a lightweight development and build workflow with fast feedback during map and dashboard development.
- The host can evolve to load separately developed frontend modules without making SSR a prerequisite.

### Trade-offs and risks

- The browser is responsible for client-side state, connection lifecycle, reconnect behavior, and recovery after a refresh or disconnection.
- A SPA does not provide SSR by default. Initial rendering, public-page SEO, and server-rendered content would need separate consideration if they become requirements.
- Static hosting does not eliminate the need to operate and secure the backend APIs and SignalR service.
- Independently built micro-frontends can duplicate framework or UI-library dependencies unless sharing is deliberately configured. Dependency sharing and runtime compatibility require separate decisions.
- Live telemetry can become stale during network interruptions. The UI must handle connection status and, where appropriate, rehydrate from HTTP endpoints after reconnecting.

## Related ADRs

- ADR 0002: The database ingestion pipeline and dual-table strategy.
- Future ADR: Micro-frontend integration and dependency-sharing strategy.
