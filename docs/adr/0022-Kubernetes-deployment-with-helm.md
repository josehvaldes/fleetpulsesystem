# ADR: Deploy system in Kubernetes with Helm
## Status
Proposed

## Date
2026-10-14

## Context
The system currently relies on a single docker-compose-aggregated.yml for local deployment. As the number of services grew (Redpanda, EMQX, TimescaleDB, four.NET services, a Python AI worker, three frontends, and the full Grafana observability stack), manual deployment to managed cloud offerings such as Azure Container Apps or AWS ECS became time-consuming and expensive for a portfolio-grade local demonstration.

We need:

- A deployment mechanism that mirrors production Kubernetes primitives(Deployments, StatefulSets, Services, Ingress, PVCs, HPA).
- An infrastructure-as-code workflow that can be committed to the repository.
- The ability to recreate the entire stack from scratch with a single command.
- A path to migrate the same manifests to a managed cluster (AKS/EKS) later.

## Decision
Use a local Kubernetes cluster provisioned with kind, managed through Helm charts as the package format, and a PowerShell bootstrap script for cluster creation, image loading, and chart installation.

*kind* was chosen over Docker Desktop's embedded Kubernetes because:

- Clusters are defined declaratively in kind-config.yaml (IaC).
- Clusters can be multi-node, simulating real scheduling constraints.
- The same scripts run in CI (GitHub Actions supports kind first-class).
- The runtime is containerd, closer to production clusters.
- Multiple clusters can coexist for experimentation.

Helm was chosen over raw manifests because:

- we have previous experience with helm
- The chart is a reusable package that can target dev, staging, and prodvia values.yaml overrides.
- Dependency management for sub-charts (e.g. cert-manager, ingress-nginx,kube-prometheus-stack) is built in.
- Templating reduces duplication across the eight custom services.

## Alternatives considered
1. Docker Desktop Kubernetes — Rejected. Convenient but cannot bescripted or committed, single-node only, not portable to CI, and hidesreal cluster concerns (no StorageClass/Ingress controller management).

2. Minikube — Rejected. Heavier than kind, slower to start, driver selection adds complexity; kind's in-Docker approach is simpler on Windows.

3. Raw manifests instead of Helm — Rejected. Duplicates configuration across environments; Kustomize was considered but Helm's release modelfits upgrade --install and rollback semantics better for this project.

4. Managed cluster (AKS/EKS) — Rejected for now due to cost. The Helmcharts produced here are designed to be deployable to a managed cluster with only values.yaml differences.

## Consequences

### Benefits

- The entire stack can be recreated with .\bootstrap-k8s.ps1.
- Helm charts are portable to AKS/EKS later with values.prod.yaml.
- Multi-node kind surfaces real scheduling and affinity issues early(e.g. Redpanda and TimescaleDB anti-affinity).
- Observability stack (Prometheus, Loki, Tempo, Grafana, OTel Collector)installs via upstream Helm charts, reducing custom manifest maintenance.
- Cluster lifecycle is reproducible and CI-friendly.

### Trade-offs
- Local image loading requires kind load docker-image after each rebuild(mitigated by a wrapper in the bootstrap script).
- kind has no built-in LoadBalancer; MetalLB must be installed forservices of type: LoadBalancer (e.g. EMQX MQTT port 1883 for the simulator).
- Resource constraints on a developer laptop: with ~20 pods and athree-node kind cluster plus Docker Desktop's VM, 16 GB RAM is the practical minimum.
- Persistent volumes use local-path and are not replicated; data losson cluster deletion is expected and acceptable for local development.

### Risks
- Divergence between kind behavior and managed-cluster behavior(e.g. PodSecurityPolicy/PodSecurityAdmission, CNI, CSI).Mitigation: keep a values.prod.yaml and document differences.
- Helm release state stored in-cluster; accidental helm uninstall can drop resources. Mitigation: use --keep-history and GitOps tooling (Flux/ArgoCD) as a future step.
- Image registry strategy: kind load docker-image does not scale toCI. Mitigation: push to a local registry running in kind for CI parity.

### Related ADRs
0006-observability-stack-and-grafana.md
(Future) ADR-0023: GitOps deployment with Flux or ArgoCD
(Future) ADR-0024: Managed cluster migration target (AKS vs EKS)