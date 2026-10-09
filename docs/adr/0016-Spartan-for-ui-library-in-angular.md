# ADR 0016: Use Spartan UI primitives in the Angular driver MFE

## Status

Accepted

## Date

2026-09-01

## Context

The driver-management module is implemented in Angular and needs a reusable component foundation for common controls and consistent styling. The React frontend uses a shadcn-style component approach, but React components cannot be reused directly inside the Angular MFE.

The Angular module uses Tailwind CSS, so an Angular-native library with composable primitives and styling that can be adapted to the project's design tokens fits the existing stack.

## Decision

Use **Spartan** (`@spartan-ng/brain` and its Helm-style component patterns) as the UI component foundation for the Angular driver-management MFE. Build reusable Angular UI components in the module's shared UI area, and style them using the project's Tailwind configuration and design tokens.

Spartan is the Angular-side analogue for the project's shadcn-inspired approach, not a shared cross-framework component library. React and Angular components remain implemented in their respective frameworks.

## Alternatives considered

### Angular Material

Angular Material provides a mature, integrated Angular component suite and accessibility behavior. It is a viable choice, but would bring its own visual system and styling conventions rather than the composable, Tailwind-oriented approach selected for this module.

### Custom components only

Building every component in-house would maximize control, but duplicates foundational accessibility and interaction work and increases the ongoing maintenance burden.

### Reuse the React shadcn components

The React components depend on React and cannot be consumed as native Angular components. Sharing a visual design language and tokens is practical; sharing framework-bound component implementations is not.

## Consequences

### Benefits

- The Angular MFE gets Angular-native primitives while following a composable, Tailwind-friendly design approach similar to the React UI.
- Shared Angular UI components can centralize styling, variants, and interaction behavior across driver-management screens.
- The project's framework boundary remains explicit instead of embedding one framework's components inside another.

### Trade-offs and safeguards

- The team must maintain Spartan-specific conventions and Angular components in addition to the React UI stack.
- A primitive/component foundation does not guarantee consistent accessibility or visual quality automatically. Verify keyboard interaction, semantics, focus behavior, and responsive styling for each composed control.
- Tailwind classes and design tokens need to remain aligned across the Angular module and host; independently built MFEs can otherwise drift visually.
- Keep package versions compatible with the Angular and Tailwind versions in use, and assess upstream maintenance and migration guidance during upgrades.
- Avoid describing Spartan as a complete drop-in Angular version of shadcn; the two ecosystems have different APIs and component implementations.

## Related ADRs

- [0015-angular-22-for-drivers-dashboard.md](0015-angular-22-for-drivers-dashboard.md)
