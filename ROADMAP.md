# FleetPulse Development Roadmap
Current Status

✅ Redpanda container deployed
✅ EMQX container deployed
✅ Python connectivity test script working
✅ TimescaleDB container
✅ EMQX → Redpanda data bridge configured
✅ Kafka topics created
✅ Database schema
⬜ All backend workers
⬜ Frontend

## Phase 1: Database Foundation
** Goal: Persistent storage ready for data ingestion
1.1 TimescaleDB Container
1.2 Database Schema (db/init.sql)
1.3 Verification
** ✅ Status: Done

## Phase 2: Stream Pipeline Configuration
** Goal: Data flows from MQTT → Kafka
2.1 Kafka Topics
2.2 EMQX Data Bridge Configuration
** ✅ Status: Done

## Phase 3: GPS Simulator
** Goal: Realistic fleet simulation generating telemetry
3.1 Project Structure
3.2 Core Components
3.3 Simulation Parameters

## Phase 4: .NET DB Batch Writer
** Goal: Efficient persistence with micro-batching and compression
4.1 Project Setup
4.2 Architecture
4.3 Key Implementation Details
4.4 SQL Operations
4.4 SQL Operations

## Phase 5: .NET SignalR Worker
** Goal: Real-time push to frontend via WebSockets
5.1 Project Setup
```bash
	dotnet new web -n FleetPulse.SignalRHub --framework net10.0
```

5.2 Architecture
```
FleetPulse.SignalRHub/
├── Program.cs
├── Hubs/
│   └── FleetHub.cs                # SignalR hub
├── Workers/
│   ├── GpsPingConsumer.cs         # Consumes gps-pings topic
│   └── AlertConsumer.cs           # Consumes ai-alerts topic
├── Services/
│   └── FleetStateManager.cs       # Tracks connected clients & subscriptions
└── Models/
    ├── GpsUpdateDto.cs
    └── AlertDto.cs
```

5.3 Hub Contract
```csharp
// Hubs/FleetHub.cs
public class FleetHub : Hub
{
    // Client can subscribe to specific fleet/region
    public async Task SubscribeFleet(string fleetId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"fleet:{fleetId}");
    }
    
    public async Task UnsubscribeFleet(string fleetId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"fleet:{fleetId}");
    }
}

// Methods called FROM server TO client
// - ReceiveGpsPing(GpsUpdateDto update)
// - ReceiveAlert(AlertDto alert)
// - ReceiveDriverOffline(string driverId)
```


5.4 Kafka → SignalR Flow
```csharp
// Workers/GpsPingConsumer.cs
// Consumes gps-pings, calls _hubContext.Clients.Group(...).SendAsync("ReceiveGpsPing", dto)
// Throttle: Don't send every ping, aggregate and send at 2Hz per driver max
```
Deliverable: WebSocket endpoint broadcasting live GPS updates

## Phase 6: Frontend SPA
** Goal: Interactive real-time fleet visualization

6.1 Project Setup
6.2 Structure
6.3 State Management (Zustand)
6.4 Map Implementation
6.5 REST API Endpoints (to add to SignalR worker)


## Phase 7: Python AI Anomaly Worker
** Goal: Detect and explain anomalies using LLMs
7.1 Project Structure
```text
ai-worker/
├── pyproject.toml
├── src/
│   └── fleetpulse_ai/
│       ├── __init__.py
│       ├── main.py
│       ├── kafka_consumer.py      # aiokafka consumer for gps-pings
│       ├── kafka_producer.py      # Producer for ai-alerts topic
│       ├── detectors/
│       │   ├── __init__.py
│       │   ├── base.py            # Abstract detector
│       │   ├── speed_anomaly.py   # Rule-based: speeding, sudden stops
│       │   ├── geofence.py        # Geofence violations
│       │   └── route_deviation.py # Off-route detection
│       ├── analyzers/
│       │   └── llm_explainer.py   # LangGraph workflow for context
│       └── config.py
└── data/
    └── geofences.json             # Danger zones, restricted areas
```

7.2 Detection Pipeline

```python

# detectors/base.py
class BaseDetector(ABC):
    @abstractmethod
    async def analyze(self, driver_id: str, history: list[GpsPing]) -> Anomaly | None:
        pass

# Example: Speed anomaly
class SpeedAnomalyDetector(BaseDetector):
    async def analyze(self, driver_id, history) -> Anomaly | None:
        # Compare current speed to speed limit for road type
        # Detect sudden deceleration (hard braking)
        # Return Anomaly if detected
```


7.3 LangGraph Workflow

```Python
# analyzers/llm_explainer.py
from langgraph.graph import StateGraph, END

def build_explainer_graph():
    workflow = StateGraph(AlertState)
    
    workflow.add_node("gather_context", gather_driver_context)
    workflow.add_node("llm_analyze", call_llm_for_explanation)
    workflow.add_node("format_alert", format_final_alert)
    
    workflow.set_entry_point("gather_context")
    workflow.add_edge("gather_context", "llm_analyze")
    workflow.add_edge("llm_analyze", "format_alert")
    workflow.add_edge("format_alert", END)
    
    return workflow.compile()
```
7.4 Alert Output

```Python
# Published to ai-alerts topic
{
    "driver_id": "DRV-042",
    "alert_type": "geofence_violation",
    "severity": "high",
    "description": "Driver entered restricted industrial zone",
    "context": {
        "zone_name": "Port Warehouse Restricted Area",
        "duration_seconds": 45,
        "llm_explanation": "Driver may be taking unauthorized shortcut through restricted port area. This zone has had 3 theft incidents this month. Recommend immediate contact."
    },
    "location": {"lat": 40.7128, "lng": -74.0060}
}
```


## Phase 8: Prometeus + Grafana implementation

8.1 Define Architecture Overview

8.2 Phase 1: Infrastructure (Docker Compose)

8.2 Phase 2: Prometheus Configuration

8.4 Phase 3: Instrument Each Service
- [] NET Services (SignalR + DB Writer)
- [] Python AI Worker
- [] Redpanda (Built-in)
- [] EMQX (Built-in)
- [] TimescaleDB (via postgres_exporter)

8.5 Phase 4: Grafana Dashboards
- [] Dashboard Provisioning
- [] Dashboard Panels to Build

8.6 Phase 5: Alerting Rules (Optional)

8.7 Phase 6: Update README Documentation

```markdown
## Observability Stack
FleetPulse includes a lightweight Prometheus + Grafana observability stack for local development and production pattern demonstration.
### Quick Start

```bash
# Infrastructure is included in docker-compose.yml
docker compose up -d prometheus grafana postgres-exporter

# Access
# Prometheus: http://localhost:9090
# Grafana:    http://localhost:3000  (admin / fleetpulse)
```

## Phase 9: Integration & Polish
** Goal: Production-ready system

9.1 Docker Compose Finalization
9.2 Repository Structure (Final)
9.3 Health Checks & Monitoring
9.4 Error Handling & Resilience


## Phase 10: Deployment Preparation (Optional)
** Goal: Cloud deployment readiness

10.1 Frontend → Cloudflare Pages
10.2 Backend → Container Service
10.2 Backend → Container Service

## Development Timeline Estimate

| Phase | Duration | Dependencies |
|-------|----------|--------------|
| 1. Database Foundation | 0.5 day | None |
| 2. Stream Pipeline Config | 0.5 day | Phase 1 |
| 3. GPS Simulator | 1-2 days | Phase 2 |
| 4. .NET DB Batch Writer | 2-3 days | Phase 1, 2 |
| 5. .NET SignalR Worker | 2-3 days | Phase 2 |
| 6. Python AI Worker | 3-4 days | Phase 2 |
| 7. Frontend SPA | 3-5 days | Phase 5 |
| 8. Integration & Polish | 2-3 days | All |
| **Total** | **~15-20 days** | |


