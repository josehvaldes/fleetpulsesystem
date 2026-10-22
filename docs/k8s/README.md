# FleetPulse on Kubernetes (local development)

This guide describes the repository's local Kubernetes integration. It is a development/demo setup based on [kind](https://kind.sigs.k8s.io/) and Helm, not a production deployment recipe. Kubernetes hosts the message brokers, Prometheus, Grafana Alloy, and FleetPulse backend workers; the database, Grafana/Loki/Tempo observability services, and frontends remain in Docker Compose.

The implemented resources are under [`kubernetes/helm/`](../../kubernetes/helm/) and [`kubernetes/infrastructure/`](../../kubernetes/infrastructure/). The distribution rationale is in [services_distribution.md](services_distribution.md), and the deployment decision is recorded in [ADR 0022](../adr/0022-Kubernetes-deployment-with-helm.md).

## Topology

| Runs in kind | Runs in Docker Compose alongside kind |
| --- | --- |
| Redpanda and Redpanda Console (upstream Helm chart) | TimescaleDB and PostgreSQL exporter |
| EMQX Enterprise and the EMQX-to-Redpanda bootstrap Job | Grafana, Loki, Tempo, and the OpenTelemetry Collector |
| FleetPulse DB writer, API, SignalR hub, and AI worker (custom Helm chart) | FleetPulse React frontend and its alert/driver micro-frontends |
| Prometheus (kube-prometheus-stack, Grafana disabled) | — |
| Grafana Alloy, collecting Kubernetes pod logs and forwarding them to Loki | — |
| Envoy Gateway and Gateway API routes; MetalLB provides local LoadBalancer support | — |

In-cluster applications use Kubernetes service DNS for Redpanda and use `host.docker.internal` through the chart's `ExternalName` services to reach the Compose-hosted database and OpenTelemetry Collector. The kind cluster is defined as one control-plane node and one worker in [`kind-config.yaml`](../../kubernetes/infrastructure/kind/kind-config.yaml).

## Prerequisites

- Docker Desktop (or a Docker-compatible engine) with its Linux container engine available. The repository's notes use WSL 2 / Ubuntu for the Kubernetes commands.
- `kind`, `kubectl`, and Helm 3 installed and available in the shell.
- Enough memory for the cluster and its stateful services. Redpanda, EMQX, Prometheus, and the app workloads all run locally.
- Local configuration and container images prepared as described below.

Check the active Kubernetes context before applying resources:

```sh
docker version
kind version
kubectl version --client
helm version
```

## 1. Prepare Docker Compose dependencies and application images

Configure the Compose environment using [`docker/env_sample.txt`](../../docker/env_sample.txt) as a reference and keep actual credentials out of source control. The standalone Compose files use the external Docker network `redpanda_network`; create it once if it does not already exist:

```sh
docker network create redpanda_network
```

Start the database and external observability services before deploying the in-cluster workloads. From the repository root:

```sh
docker compose -f docker/docker-compose-postgresql.yml up -d
docker compose -f docker/docker-compose-observability.yml up -d
```

Build the backend and frontend images using the repository's scripts from the root. The chart expects `fleetpulse-dbwriter:1.0`, `fleetpulse-api:1.0`, `fleetpulse-signalrhub:1.0`, and `fleetpulse-ai-worker:1.0`; the frontend Compose file expects the frontend images built by `build-frontend.ps1`.

```powershell
.\build-images.ps1
.\build-frontend.ps1
```

The frontend Compose services are optional if you only want to exercise the in-cluster backend. To run them, configure their environment and start them with `docker compose -f docker/docker-compose-frontend.yml up -d`. The Compose files and their networking assumptions are also described in [docker/README.md](../../docker/README.md).

## 2. Create the kind cluster and networking layer

Run from the repository root. The cluster name is `fleetpulse`.

```sh
kind create cluster --config kubernetes/infrastructure/kind/kind-config.yaml
kubectl get nodes
```

Install MetalLB and its local address pool. The pool is declared in [`metallb-pool.yaml`](../../kubernetes/infrastructure/metallb-pool.yaml):

```sh
helm repo add metallb https://metallb.github.io/metallb
helm repo update
helm upgrade --install metallb metallb/metallb \
  --namespace metallb-system --create-namespace
kubectl apply -f kubernetes/infrastructure/metallb-pool.yaml
```

Install Gateway API CRDs before Envoy Gateway. These versions match the implementation notes:

```sh
kubectl apply -f https://github.com/kubernetes-sigs/gateway-api/releases/download/v1.6.1/standard-install.yaml
helm upgrade --install eg oci://docker.io/envoyproxy/gateway-helm \
  --version v1.9.2 --namespace envoy-gateway-system --create-namespace \
  --set crds.enabled=false
kubectl wait --for=condition=available deployment/envoy-gateway \
  --namespace envoy-gateway-system --timeout=120s
kubectl apply -f kubernetes/infrastructure/gateway.yaml
```

## 3. Load local images and create Kubernetes secrets

Load the locally built application images into kind. Repeat the load step after rebuilding an image:

```sh
kind load docker-image fleetpulse-dbwriter:1.0 --name fleetpulse
kind load docker-image fleetpulse-api:1.0 --name fleetpulse
kind load docker-image fleetpulse-signalrhub:1.0 --name fleetpulse
kind load docker-image fleetpulse-ai-worker:1.0 --name fleetpulse
```

Create the namespace and secrets before installing the charts. The chart expects:

- `fleetpulse-db-credentials`, key `connection-string`, with a connection string whose host is `timescaledb-external` and whose credentials match the Compose database configuration.
- `fleetpulse-auth-credentials`, populated from your local authentication environment file. Use [`auth-credentials.env.sample`](../../kubernetes/helm/env/auth-credentials.env.sample) as a template.
- `fleetpulse-ai-worker-config`, populated from your AI worker environment file. Use [`fleetpulse-ai-worker-config.env.sample`](../../kubernetes/helm/env/fleetpulse-ai-worker-config.env.sample) as a template.
- `emqx-secret`, with `EMQX_DASHBOARD__DEFAULT_USERNAME` and `EMQX_DASHBOARD__DEFAULT_PASSWORD`. See [`emqx.env.sample`](../../kubernetes/helm/env/emqx.env.sample).

Copy the sample files to private local files, update them for your environment, and do not commit the populated files. For example, after creating those private files, apply the application secrets like this:

```sh
kubectl create namespace fleetpulse --dry-run=client -o yaml | kubectl apply -f -
kubectl -n fleetpulse create secret generic fleetpulse-auth-credentials \
  --from-env-file=kubernetes/helm/env/auth-credentials.env \
  --dry-run=client -o yaml | kubectl apply -f -
kubectl -n fleetpulse create secret generic fleetpulse-ai-worker-config \
  --from-env-file=kubernetes/helm/env/fleetpulse-ai-worker-config.env \
  --dry-run=client -o yaml | kubectl apply -f -
kubectl -n fleetpulse create secret generic emqx-secret \
  --from-env-file=kubernetes/helm/env/emqx.env \
  --dry-run=client -o yaml | kubectl apply -f -
```

Create a local, untracked env file containing the `connection-string` key and its database connection string, then create `fleetpulse-db-credentials` from it. Match the database name, user, and password to the Compose configuration; do not commit the populated file.

```sh
kubectl -n fleetpulse create secret generic fleetpulse-db-credentials \
  --from-env-file=/path/to/private/database-credentials.env \
  --dry-run=client -o yaml | kubectl apply -f -
```

## 4. Install the in-cluster services

Add the upstream chart repositories and install Redpanda, EMQX, and Prometheus with the repository's local-development values. These versions align with the values files in `kubernetes/helm/`.

```sh
helm repo add redpanda https://charts.redpanda.com/
helm repo add emqx https://repos.emqx.io/charts
helm repo add prometheus-community https://prometheus-community.github.io/helm-charts
helm repo update

helm upgrade --install redpanda redpanda/redpanda \
  --version 26.2.4 --namespace fleetpulse --create-namespace \
  --values kubernetes/helm/redpanda-values.yaml

helm upgrade --install emqx emqx/emqx-enterprise \
  --version 6.3.2 --namespace fleetpulse --create-namespace \
  --values kubernetes/helm/emqx-enterprise-values.yaml

helm upgrade --install fleetpulse-monitoring \
  prometheus-community/kube-prometheus-stack --version 92.10.1 \
  --namespace monitoring --create-namespace \
  --values kubernetes/helm/kube-prometheus-stack-values-kind.yaml

kubectl apply -f kubernetes/infrastructure/emqx-mqtt-loadbalancer.yaml
```

For development, expose the EMQX's port configured in the `emqx-mqtt-loadbalancer.yaml` manifest
That will be used by the simulator to load GPS data

```sh
kubectl port-forward -n fleetpulse svc/emqx-mqtt-external 1883:1883
```

Install the FleetPulse application chart and apply the EMQX-to-Redpanda bootstrap Job:

```sh
helm upgrade --install fleetpulse kubernetes/helm/fleetpulse-services \
  --namespace fleetpulse --create-namespace

kubectl create configmap emqx-bootstrap-script --namespace fleetpulse \
  --from-file=bootstrap.sh=kubernetes/helm/emqx-redpanda-bridge/bootstrap.sh \
  --dry-run=client -o yaml | kubectl apply -f -
kubectl apply -f kubernetes/helm/emqx-redpanda-bridge/bootstrap-job.yaml
```

The bootstrap Job uses the EMQX credentials from `emqx-secret`. If you change its script and need to rerun the Job, delete the completed Job and apply the manifest again.

### Install Grafana Alloy for Kubernetes logs

Loki remains in the Docker Compose observability stack. The FleetPulse chart creates the `loki-external` Service in the `fleetpulse` namespace, resolving to `host.docker.internal`; Alloy uses it to push Kubernetes pod logs to Loki's HTTP push API. Start the Compose observability services before installing Alloy.

Add Grafana's Helm repository and install Alloy using the local values file:

```sh
helm repo add grafana https://grafana.github.io/helm-charts
helm repo update
helm upgrade --install alloy grafana/alloy \
  --namespace fleetpulse --create-namespace \
  --values kubernetes/helm/alloy-values.yaml
```

The Alloy configuration discovers Kubernetes pods, reads their logs, and forwards them to `http://loki-external.fleetpulse.svc.cluster.local:3100/loki/api/v1/push`. Check the Alloy pod logs if collection or delivery is not working:

```sh
kubectl logs --namespace fleetpulse \
  --selector app.kubernetes.io/name=alloy \
  --all-containers=true
```

This replaces the Docker-oriented Promtail setup for Kubernetes pod logs; it does not move Loki into the cluster.


### Install Prometheus configuration
Execute the following commands to apply the Prometheus configuration:

```sh
kubectl apply -f kubernetes/servicemonitors/fleetpulse-dbwriter.yaml
kubectl apply -f kubernetes/servicemonitors/fleetpulse-ai-worker.yaml
kubectl apply -f kubernetes/servicemonitors/fleetpulse-dbwriter.yaml
kubectl apply -f kubernetes/servicemonitors/fleetpulse-signalr-hub.yaml
```

## 5. Apply routes and access the services

The Gateway resource lives in `default`; the HTTPRoute in `fleetpulse` routes driver and alert REST paths to the API and session/SignalR paths to the hub:

```sh
kubectl apply -f kubernetes/infrastructure/httproute.yaml
kubectl get gateway -A
kubectl get httproute -n fleetpulse
kubectl get pods,services -n fleetpulse
```

For local access, find the Envoy Gateway proxy Service and port-forward it (the generated Service name can differ between installations):

```sh
kubectl get services --namespace envoy-gateway-system
kubectl port-forward --namespace envoy-gateway-system \
  service/<envoy-gateway-proxy-service> 8080:80
```

With the port-forward active, the Gateway listener is available at `http://localhost:8080`. The EMQX dashboard and Redpanda Console can be reached with `kubectl port-forward` to their respective Services; Prometheus can be forwarded from the `monitoring` namespace. Example:

```sh
kubectl port-forward --namespace monitoring service/prometheus-operated 9090:9090
```

The Compose frontends are published on ports 5173, 5174, and 5175 by `docker-compose-frontend.yml`; configure their API/hub base URLs to point to the reachable Gateway address for your environment.

## 6. Verify and troubleshoot

```sh
kubectl get nodes
kubectl get pods --all-namespaces
kubectl get pvc --all-namespaces
helm list --all-namespaces
kubectl describe gateway fleetpulse-gateway --namespace default
kubectl describe httproute fleetpulse-backend --namespace fleetpulse
kubectl logs --namespace fleetpulse job/emqx-bootstrap
```

If an app pod cannot start, check that the expected image was built and loaded into the `fleetpulse` kind cluster, and that all required Secrets exist in the `fleetpulse` namespace. If a pod cannot reach the database, collector, or Loki, check that the corresponding Compose service is running, its host port is published, and `host.docker.internal` resolves from the kind node/pod environment. For missing logs, inspect Alloy's logs and confirm the `loki-external` Service resolves to the host and Loki accepts pushes on port 3100.

## 7. Stop the environment

Stop the Compose groups you started from the repository root:

```sh
docker compose -f docker/docker-compose-frontend.yml down
docker compose -f docker/docker-compose-observability.yml down
docker compose -f docker/docker-compose-postgresql.yml down
```

Delete the kind cluster when finished. This also removes in-cluster state and persistent volumes, including Redpanda and Prometheus data:

```sh
kind delete cluster --name fleetpulse
```

## Important limitations

- The Helm values are tuned for a local, single-broker demo. Redpanda disables TLS and SASL; do not reuse these values in a shared or production cluster without configuring authentication, encryption, storage, and resource sizing.
- The database and tracing collector are external to Kubernetes and are reached through `host.docker.internal`; this is environment-dependent and should be replaced with explicit reachable endpoints for other cluster types.
- Images are loaded directly into kind. A registry and production-specific Helm values are needed for remote clusters.
- Deleting the kind cluster discards its local persistent data. Back up anything that must be retained.

For the full service placement and selected image versions, see [services_distribution.md](services_distribution.md). The command-by-command migration scratch notes are historical and are not the supported deployment guide.
