
Inside FleetPulse Sluster:
	Redpanda helm:
	- docker.redpanda.com/redpandadata/redpanda:v26.2.5
	- docker.redpanda.com/redpandadata/console:v3.8.0
	
	EMQX-enterprise helm:
	- emqx/emqx-enterprise:6.3
	- K8s job to replace curlimages/curl:8.23.0
	
	prometheus helm: 
	- kube-prometheus-stack without grafana to replace (prom/prometheus:v3.6.0)
	
	services:
	- fleetpulse-dbwriter:1.0
	- fleetpulse-signalrhub:1.0
	- fleetpulse-api:1.0
	- fleetpulse-ai-worker:1.0
	- (YAR is replaced by the Envoy Gateway API in k8s) fleetpulse-yarpproxy:1.0
	
	frontend:
	- fleetpulse-frontend:1.0
	- fleetpulse.alert-mgmt:1.0
	- fleetpulse.drivers-mgmt:1.0
	

outside cluster 
	database: 
	- timescale/timescaledb:2.30.2-pg18
	- prometheuscommunity/postgres-exporter:v0.17.1
	
	grafana:
	- grafana/grafana:13.1.1
	
	promtail:
	- Replaced by Kubernetes pod log (grafana/promtail:2.9.5) 
	
	loki:
	- grafana/loki:2.9.5
	
	tempo:
	- grafana/tempo:2.10.6
	- otel/opentelemetry-collector-contrib:0.158.0