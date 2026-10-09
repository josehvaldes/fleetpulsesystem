# ADR 0015: Build the driver-management MFE with Angular

## Status

Accepted

## Date

2026-09-01

## Context

FleetPulse already has a React/Vite host and a React alert-management MFE. The driver-management capability is a separate frontend module and should be integrated into the same application without requiring the host to be rewritten in Angular.

The project also aims to demonstrate integration of independently built frontend technologies. That portfolio/learning objective is a secondary project-specific consideration; the selected framework still needs to work as a remote module within the existing host.

## Decision

Build the driver-management MFE with Angular 22 and integrate it into the React host using Angular Architects Native Federation. Expose an explicit mount entry point for the host to load and unmount the Angular UI, and keep the host responsible for providing shared application configuration and integration props such as the API base URL and authentication-token callback.

Keep the React host as the application shell. Do not require the host or the alert-management MFE to migrate to Angular as a consequence of this decision.

## Alternatives considered

### React with Vite

Using React would align the driver module with the host and alert MFE, reduce framework diversity, and allow reuse of React-specific skills and patterns. Angular was selected for the isolated driver module to demonstrate cross-framework federation and because the module can be mounted behind a defined integration contract.

### A standalone Angular application

A separately navigated Angular application would avoid cross-framework mounting, but would split the user experience and require additional routing, authentication, and deployment integration rather than composing the driver capability into the existing host.

## Consequences

### Benefits

- Driver management can be developed and built independently while remaining part of the FleetPulse frontend experience.
- The application demonstrates interoperability between a React host and an Angular remote.
- The explicit mount/unmount boundary makes framework-specific rendering lifecycle the responsibility of the remote.

### Trade-offs and safeguards

- The team must maintain expertise, dependencies, build tooling, and testing practices for both React and Angular.
- Cross-framework integration is not transparent: styles, routing, authentication, API configuration, error handling, and lifecycle must be defined at the boundary.
- Angular and Native Federation add runtime and build configuration. Verify compatibility and loading behavior in both development and production deployments.
- Keep the MFE contract framework-neutral where practical; avoid leaking Angular-specific types or services into the React host.
- This ADR records the project-specific framework choice and learning goal, not a claim that Angular is universally preferable for driver-management interfaces.

## Related ADRs

- [0014-module-federation-over-originjs-for-MFE.md](0014-module-federation-over-originjs-for-MFE.md)
- [0016-Spartan-for-ui-library-in-angular.md](0016-Spartan-for-ui-library-in-angular.md)
- [0017-use-dynamic-loaders-for-mfe-modules.md](0017-use-dynamic-loaders-for-mfe-modules.md)
