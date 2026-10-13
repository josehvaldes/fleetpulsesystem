# Continuous integration workflows

The repository's GitHub Actions CI is split by technology stack. Push and pull-request runs target `main` and use path filters, so a workflow normally runs only when its own application directories or workflow definition changes. Each workflow can also be started manually with `workflow_dispatch`.

All three workflows define a concurrency group per workflow and Git ref, with `cancel-in-progress: true`. A newer run for the same ref therefore cancels the older run; runs for different refs or workflows do not share a group.

## Workflow overview

| Workflow | Applications | Runtime | Jobs |
| --- | --- | --- | --- |
| `ci-frontend.yml` | React SPA, React Alerts MFE, Angular Drivers MFE | Node.js 24 | Three independent application jobs |
| `ci-dotnet.yml` | DbWriter, SignalRHub, API | .NET 10 | One matrix job with one entry per solution |
| `ci-python.yml` | GPS simulator, AI anomaly worker | Python 3.13.5 | One matrix job with one entry per application |

## Frontend: `ci-frontend.yml`

**Triggers:** Pushes and pull requests to `main` that change files under `fleetpulse.frontend/`, `fleetpulse.alert-mgmt/`, or `fleetpulse.drivers-mgmt/`, or change this workflow. It can also be run manually.

The workflow has one Ubuntu job for each frontend application. Each job checks out the repository, configures Node.js 24 with npm dependency caching keyed by that application's `package-lock.json`, and performs a clean install using `npm ci`.

| Job | Checks after install |
| --- | --- |
| React SPA (`fleetpulse.frontend`) | `npm run lint`, `npm run build`, then `npm test -- --run --passWithNoTests` |
| React Alerts MFE (`fleetpulse.alert-mgmt`) | `npm run build`, then `npm test -- --run --passWithNoTests` |
| Angular Drivers MFE (`fleetpulse.drivers-mgmt`) | `npx ng build`, then `npx ng test --watch=false` |

The React SPA is the only frontend job that currently runs a lint command. The React test command permits a successful run when no tests are found; the Angular test command does not use that option.

## .NET: `ci-dotnet.yml`

**Triggers:** Pushes and pull requests to `main` that change files under `FleetPulse.DbWriter/`, `FleetPulse.SignalRHub/`, or `FleetPulse.Api/`, or change this workflow. It can also be run manually.

The `build-test` job runs on Ubuntu and expands a matrix over these solutions:

- `FleetPulse.DbWriter/FleetPulse.DbWriter.slnx` — DbWriter worker
- `FleetPulse.SignalRHub/FleetPulse.SignalRHub.slnx` — SignalRHub worker
- `FleetPulse.Api/FleetPulse.Api.slnx` — API service

For each solution, the job checks out the repository, configures .NET 10 with NuGet caching (using `**/*.csproj` as the cache dependency path), restores, builds in Release without restoring again, and runs tests in Release without rebuilding. Test output uses normal console verbosity. Matrix `fail-fast` is enabled, so a failing solution cancels other in-progress matrix entries.

## Python: `ci-python.yml`

**Triggers:** Pushes and pull requests to `main` that change files under `ai-worker/` or `simulator/`, or change this workflow. It can also be run manually.

The `lint-test` job runs on Ubuntu and expands a matrix across `simulator` and `ai-worker`. Each entry uses Python 3.13.5, caches pip dependencies against that application's `src/requirements.txt`, and runs from the corresponding `src/` directory. The steps install the requirements and the CI tools (`ruff`, `pytest`, and `pytest-asyncio`), then run `ruff --version` and `ruff check .`.

Pytest is currently run only for `ai-worker` (`python -m pytest -q`); the simulator matrix entry is linted but has no test step. Matrix `fail-fast` is disabled, so a failure in one application does not cancel the other application's checks.

## Maintenance notes

- Keep each workflow's `paths` filters aligned with the applications it validates. Changes outside those paths do not start push or pull-request CI for that workflow.
- Changes to a workflow file itself are included in that workflow's path filter.
- Manual runs are not restricted by the path filters.
- If a new application, solution, runtime version, or test command is added, update both the corresponding workflow and this document.