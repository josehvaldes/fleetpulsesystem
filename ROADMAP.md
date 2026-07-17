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

7.1 Project Setup
7.2 Structure
7.3 State Management (Zustand)
7.4 Map Implementation
7.5 REST API Endpoints (to add to SignalR worker)


## Phase 7: Python AI Anomaly Worker
** Goal: Detect and explain anomalies using LLMs
6.1 Project Structure
6.2 Detection Pipeline
6.3 LangGraph Workflow
6.4 Alert Output



## Phase 8: Integration & Polish
** Goal: Production-ready system

8.1 Docker Compose Finalization
8.2 Repository Structure (Final)
8.3 Health Checks & Monitoring
8.4 Error Handling & Resilience


## Phase 9: Deployment Preparation (Optional)
** Goal: Cloud deployment readiness

9.1 Frontend → Cloudflare Pages
9.2 Backend → Container Service
9.2 Backend → Container Service

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


