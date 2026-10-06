# Architecture Decision Records Template 

## Title: 
Aggregate CI Workflows by Technology Stack

## Status: 
Accepted 

## Date: 
2026-10-05

## Context
The FleetPulse project is structured as a polyglot monorepo containing 7 independent applications spanning 3 distinct technology stacks:

- Frontend (3 apps): Vite + React 19 SPA, React Alerts MFE, Angular Drivers MFE.
- .NET Backend (2 apps): SignalRHub Worker, DbBatchWriterWorker.
- Python Backend (2 apps): GPS Simulator, AI Anomaly Worker (LangGraph).

We need to implement GitHub Actions workflows to automatically build and run unit tests for these services. Because the applications are independent, CI runs should be optimized to only execute when the relevant codebase changes.

We evaluated how to group these CI pipelines. Workflow triggers in GitHub Actions (on.push.paths) are scoped at the workflow file level, meaning if multiple jobs are in a single file, they will all trigger if any matching path changes.

## Decision
We will implement 3 aggregate CI workflow files, grouped by technology stack, rather than 1 monolithic file or 7 individual project files.

The repository will contain:

[1] .github/workflows/ci-dotnet.yml (Triggers on changes in backend/SignalRHub/** or backend/DbBatchWriter/**)
[2] .github/workflows/ci-frontend.yml (Triggers on changes in frontend/spa/**, frontend/alerts-mfe/**, or frontend/drivers-mfe/**)
[3] .github/workflows/ci-python.yml (Triggers on changes in simulator/** or ai-worker/**)

Inside each workflow file, we will use a job matrix to fan out and build the individual applications in parallel (e.g., the ci-dotnet.yml workflow will have a matrix containing both the SignalRHub and DbBatchWriter solutions).

## Alternatives
** Alternative 1: One workflow per project (7 Workflow Files)
Instead of grouping by stack, we create ci-signalr-hub.yml, ci-react-spa.yml, ci-angular-mfe.yml, etc.

*Pros:
Maximum granularity in PR status checks (e.g., you can require exactly ci-react-spa to pass before merge).
No matrix complexity; each file is flat and simple.
Easier to attach specific deployment steps or environment secrets to individual apps later.

*Cons:
YAML duplication (boilerplate for actions/checkout and setup-node is repeated up to 3 times per stack).
Harder to maintain branch protection rules (7 required status checks instead of 3).
Shared-code triggers (e.g., a root package.json or tsconfig.json) must be manually added to the paths: filter of multiple frontend workflow files.

** Alternative 2: One monolithic workflow for all projects (1 Workflow File)
A single ci.yml containing jobs for .NET, Node, and Python.

* Pros: 
Single file to maintain.

* Cons: 
GitHub's paths filter is workflow-level, not job-level. A change to a Python file would trigger the .NET and Node.js jobs. To prevent this, we would need 3rd-party actions (like dorny/paths-filter) and if: conditions on every job, essentially recreating native GitHub functionality manually. Toolchain caches (npm, pip, nuget) would also coexist in the same file, increasing complexity.

## Consequences
**Benefits (Pros):

- Clean PR Checks: Branch protection rules only need to require 3 checks (ci-dotnet, ci-frontend, ci-python). The Actions UI clearly shows which stack failed.
- Toolchain Isolation: Caching (actions/setup-dotnet, setup-node, setup-python) is isolated per workflow file, preventing cache-key collisions.
- Reduced Boilerplate: By using a matrix strategy inside each file, we define the checkout and setup steps once per stack, rather than duplicating them for 7 projects.
- Native Path Filtering: Uses GitHub's built-in on.push.paths trigger cleanly without relying on third-party conditional logic.

**Trade-offs & Risks (Cons):

- Coarser Trigger Granularity: If a PR touches the React SPA, the ci-frontend.yml workflow runs, which also builds the Angular MFE. This is a minor waste of compute compared to Alternative 1, but acceptable for a portfolio project.

- Matrix Complexity: Developers less familiar with GitHub Actions matrices might find the YAML slightly harder to read than flat, sequential steps.

- Shared File Triggers: If a shared root configuration file is changed (e.g., docker-compose.yml), none of the 3 workflows will trigger automatically unless those shared paths are explicitly added to the paths: filter of the relevant workflows.

## Related ADRS
