# Continuous integration workflows

The repository's GitHub Actions CI is split into application workflows and reusable build workflows. Application workflows run on pushes and pull requests targeting `main` when their path filters match, and can also be started manually with `workflow_dispatch`. Reusable workflows run only when called by an application workflow.

Each application workflow defines a concurrency group using its workflow name and Git ref, with `cancel-in-progress: true`. A newer run for the same workflow and ref cancels the older run; different workflows and refs use separate groups.

## Workflow overview

| Workflow | Purpose | Runtime | Jobs |
| --- | --- | --- | --- |
| `ci-frontend.yml` | React SPA, React Alerts MFE, Angular Drivers MFE, and browser E2E tests | Node.js 24 | Three application jobs and one Playwright job |
| `ci-api.yml` | API | .NET 10 | Calls `build-dotnet.yml` |
| `ci-dbwriter.yml` | DbWriter | .NET 10 | Calls `build-dotnet.yml` |
| `ci-signalrhub.yml` | SignalRHub | .NET 10 | Calls `build-dotnet.yml` |
| `ci-simulator.yml` | GPS simulator | Python 3.13.5 | Calls `build-python.yml` |
| `ci-ai-worker.yml` | AI anomaly worker | Python 3.13.5 | Calls `build-python.yml` |
| `build-dotnet.yml` | Reusable .NET restore, build, test, and optional container publish | .NET 10 | One build job |
| `build-python.yml` | Reusable Python lint, optional tests, and optional container publish | Python 3.13.5 | One build job |

## Frontend: `ci-frontend.yml`

**Triggers:** Pushes and pull requests to `main` that change `frontend/fleetpulse.frontend/`, `frontend/fleetpulse.alert-mgmt/`, `frontend/fleetpulse.drivers-mgmt/`, `frontend/tests/`, either API or SignalRHub OpenAPI definitions, or this workflow. It can also be run manually.

The workflow has three independent application jobs. Each checks out the repository, sets up Node.js 24, installs dependencies with `npm ci`, generates API contracts, and verifies that the generated client is committed and up to date using `git diff --exit-code -- src/generated`.

| Job | Checks after contract verification |
| --- | --- |
| React SPA (`frontend/fleetpulse.frontend`) | `npm run lint`, `npm run build`, then `npm test -- --run --passWithNoTests` |
| React Alerts MFE (`frontend/fleetpulse.alert-mgmt`) | `npm run build`, then `npm test -- --run --passWithNoTests` |
| Angular Drivers MFE (`frontend/fleetpulse.drivers-mgmt`) | `npx ng build`, then `npx ng test --watch=false` |

The fourth job, Playwright E2E, installs dependencies for all three applications and `frontend/tests`, installs Chromium with its system dependencies, and runs `npm run test:e2e` from `frontend/tests`. It uploads the Playwright report and test results even when earlier steps fail; missing report files are ignored.

## .NET application workflows

`ci-api.yml`, `ci-dbwriter.yml`, and `ci-signalrhub.yml` run on pushes and pull requests to `main` when the corresponding service directory, `Directory.Packages.props`, the calling workflow, or `build-dotnet.yml` changes. Each can also be run manually.

Each application workflow calls the reusable `build-dotnet.yml` with its solution, Docker build context, Dockerfile, and image name:

| Application workflow | Solution | Image |
| --- | --- | --- |
| `ci-api.yml` | `FleetPulse.Api/FleetPulse.Api.slnx` | `fleetpulse-api` |
| `ci-dbwriter.yml` | `FleetPulse.DbWriter/FleetPulse.DbWriter.slnx` | `fleetpulse-dbwriter` |
| `ci-signalrhub.yml` | `FleetPulse.SignalRHub/FleetPulse.SignalRHub.slnx` | `fleetpulse-signalrhub` |

The reusable workflow checks out the repository, sets up .NET 10 with NuGet caching keyed by `**/*.csproj`, then restores, builds in Release without restoring again, and tests in Release without rebuilding. Tests use normal console verbosity.

Container publishing is enabled only for pushes to `main`, not pull requests or manual runs. When enabled, the workflow logs in to GitHub Container Registry (GHCR) and builds and pushes the image tagged `latest` and `sha-<commit>`, with GitHub Actions build caching and OCI source/revision labels.

## Python application workflows

`ci-simulator.yml` and `ci-ai-worker.yml` run on pushes and pull requests to `main` when the corresponding application directory or either the calling workflow or `build-python.yml` changes. Each can also be run manually.

Both call the reusable `build-python.yml`, which sets up Python 3.13.5, caches pip using the application's `src/requirements.txt`, installs the requirements and `ruff`, `pytest`, and `pytest-asyncio`, and runs `ruff check .` from the application's `src/` directory.

| Application workflow | Tests | Container publishing |
| --- | --- | --- |
| `ci-simulator.yml` | None; `run-tests` is false | Disabled |
| `ci-ai-worker.yml` | `python -m pytest -q` | Enabled only on pushes to `main` |

When enabled for the AI worker, publishing builds and pushes to GHCR with `latest` and `sha-<commit>` tags, GitHub Actions build caching, and OCI source/revision labels.

## Maintenance notes

- Keep application path filters aligned with the code and shared inputs each workflow validates. Changes outside those paths do not start its push or pull-request CI.
- The frontend workflow also watches API and SignalRHub OpenAPI definitions because its applications generate API contracts from them.
- .NET workflows watch `Directory.Packages.props`; application workflows also watch their reusable workflow definition so shared build changes trigger CI.
- Manual runs are not restricted by path filters. Reusable `build-dotnet.yml` and `build-python.yml` are called workflows and do not define independent push, pull-request, or manual triggers.
- Update this document when workflow triggers, application coverage, runtimes, validation commands, or publishing behavior changes.
