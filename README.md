# FleetPulse System

## Document Revision

| Version | Date | Author | Description
| :--- | :--- | :--- | :--- |
| 1.0 | 2026-07-08 | jose valdes | Created: Architectural document created |
| 1.1 | 2026-07-09 | jose valdes | Added: native MQTT proxy was deprecated inside redpanda. EMQX Broker container is added to receive MQTT messages and forward them to RedPanda |


# Architecture
## Overview
FleetPulse is a real-time, AI-powered logistics control tower designed to monitor a simulated fleet of delivery motorcycles. It demonstrates a high-throughput, event-driven architecture capable of ingesting high-frequency GPS telemetry, analyzing it for anomalies via LLMs, and visualizing it dynamically on a live map.

This project highlights modern distributed systems concepts: Stream Processing, Micro-batching, Temporal Data Compression, and Decoupled Backend Workers.

## High-Level Architecture
The system follows a strict fan-out pattern. Edge devices (simulated) publish telemetry to a stream broker. Independent backend services consume the stream concurrently based on their domain responsibility. The frontend operates as a pure SPA, receiving real-time pushes via WebSockets.

```mermaid
graph TD

subgraph Edge [Edge / Simulation]
Sim[Python GPS Simulator]
end

subgraph Stream [Streaming Layer - Docker]
EMQX[(EMQX <br/> MQTT Broker)]
RP[(Redpanda <br/> Kafka API)]
EMQX -- "Data Bridge (Rule Engine)" --> RP
end

subgraph Processing [Backend Workers]
WS[.NET 10 SignalR Worker]
AI[Python AI Anomaly Worker <br/> LangGraph]
DB[.NET 10 DB Batch Writer]
end
subgraph Storage [Data Layer]
TSDB[(TimescaleDB <br/> Postgres Extension)]
end
subgraph Presentation [Frontend - Cloudflare CDN]
UI[Vite + React 19 SPA <br/> Leaflet.js Maps]
end

Sim -- "MQTT (paho-mqtt)" --> EMQX
RP -- "Kafka Topic: gps-pings" --> WS
RP -- "Kafka Topic: gps-pings" --> AI
RP -- "Kafka Topic: gps-pings" --> DB

AI -- "Kafka Topic: ai-alerts" --> WS
DB -- "Bulk Upsert" --> TSDB
WS -- "WebSocket (SignalR)" --> UI
TSDB -- "REST API (Aggregated)" --> UI


```

## Technology Stack & Rationale
| Layer | Technology | Architectural Rationale |
| :--- | :--- | :--- |
| **Telemetry Ingestion** | Python + `paho-mqtt` | MQTT is the industry standard for IoT/edge devices due to minimal bandwidth and battery overhead. |
| **MQTT Broker** | EMQX Broker (Docker) | MQTT broker for IoT that nativily integrates to Kafka-compatible RedPanda. |
| **Message Broker** | Redpanda (Docker) | Native Kafka API compatibility.  |
| **Real-Time Push** | .NET 10 Worker + SignalR | Maintains persistent WebSocket state. SignalR Groups natively handle routing updates to specific fleet managers. |
| **AI / Analytics** | Python + `aiokafka` + LangGraph | Decoupled async worker. Consumes streams without blocking, uses LLMs for contextual anomaly explanations. |
| **Data Storage** | PostgreSQL + TimescaleDB | Relational reliability for transactions, combined with TimescaleDB's `Hypertables` for high-performance time-series compression and `time_bucket()` aggregations. |
| **Frontend** | Vite + React 19 + TS | Pure SPA. Server-Side Rendering (Next.js) is intentionally avoided as real-time data is stale the moment it's rendered. Vite provides superior HMR for map UI tuning. |
| **Deployment (FE)** | Cloudflare Pages | Static assets served at the edge. Zero cold starts, zero cost, inherently secure. |
| **Deployment (BE)** | Docker Compose (Local) <br> AWS ECS / Azure Container Apps (Prod) | Containerized workloads allow independent scaling of the AI worker vs. the DB writer based on load. |
| **DB Batch Writer** | .NET DB Batch Writer (background service) | Fast developement with Dapper + inline SQL. Using Npgsql library  |
|
## Architectural Deep Dives

1. The Database Ingestion Pipeline (DB Batch Writer)
Writing 250 individual GPS pings per second directly to a relational database causes severe I/O bottlenecks and lock contention. The DB Writer solves this using a 3-stage pipeline:

Micro-Batching: The worker consumes the Kafka stream but holds GPS pings in an in-memory buffer, flushing to the database in bulk every 5 seconds (or at 1,000 records). This turns 250 I/O ops/sec into ~25 bulk ops/sec.
Temporal Compression: Before flushing, the logic drops redundant data. If a driver is stopped at a red light for 30 seconds, only the first and last ping are kept. Moving highway pings are down-sampled to 1 point per 15 seconds.
Dual-Table Write Strategy:
gps_history (Hypertable): Receives the bulk-inserted, compressed historical data. TimescaleDB automatically compresses data older than 7 days to save ~90% disk space.
driver_latest_state (Standard Table): Receives a continuous UPSERT. This table strictly contains exactly one row per active driver (e.g., 500 rows), allowing the frontend to query current locations in milliseconds without scanning millions of historical rows.
2. Frontend Architecture: Why a Pure SPA?
A common question is why this project uses Vite + React instead of Next.js (which was used in the companion e-commerce project).

Stale Data Problem: Next.js Server Components render HTML on the server. By the time the HTML reaches the browser, the GPS coordinates have already changed. SSR provides zero value for live moving objects.
Connection State: WebSockets require long-lived, stateful connections. Next.js Serverless functions (Vercel) aggressively terminate idle connections and enforce strict timeouts.
Performance: Vite's Hot Module Replacement (HMR) is nearly instantaneous, which is critical when iterating on complex map animations and real-time chart state.
3. Decoupled AI Alerting
The AI worker does not push alerts directly to the frontend. Instead, it evaluates the gps-pings stream, and if an anomaly is detected (e.g., a driver is stationary in a high-risk zone), it publishes a new event to a separate Kafka topic: ai-alerts.

The SignalR Worker subscribes to both topics (gps-pings and ai-alerts) and acts as the single gateway to the browser. This ensures the frontend remains completely agnostic to how many backend workers are processing data behind the scenes.
	   

## Database Access 

```
[ Redpanda Topic: "gps-pings" ]
       │
       ▼
[ .NET DB Batch Writer (Background Service) ]
       │
       ├──> 1. In-Memory Buffer (holds for 5 seconds)
       │
       ├──> 2. Compression Logic (drops stopped-driver duplicates)
       │
       ├──> 3. Bulk INSERT -> TimescaleDB `gps_history` table (Hypertable)
       │
       └──> 4. UPSERT -> TimescaleDB `driver_latest_state` table (500 rows)
```



## Local Development Topology
During local development, the entire distributed system is orchestrated via a single docker-compose.yml file, ensuring zero friction for developers cloning the repository.

```
┌─────────────────────────────────────────────────────────┐
│                  Docker Compose Network                 │
│                                                         │
│  ┌─────────────┐  ┌─────────────┐  ┌──────────────────┐ │
│  │  EMQX       │  │ .NET SignalR│  │ Python AI Worker │ │
│  │ (MQTT Broker│  │   Worker    │  │   (LangGraph)    │ │
│  └──────┬──────┘  └─────┬───────┘  └──────────────────┘ │
│         │               │                               │
│         │ Data Bridge   │                               │
│         ▼               │                               │
│  ┌────────────┐         │                               │
│  │  Redpanda  │<>───────┘                               │
│  │ (Kafka API)│                                         │
│  └──────┬─────┘                                         │
│         │                                               │
│  ┌──────┴─────┐  ┌────────────┐                         │
│  │TimescaleDB │<>│ .NET DB    │                         │
│  │  (Postgres)│  │   Writer   │                         │
│  └────────────┘  └────────────┘                         │
└─────────────────────────────────────────────────────────┘
         ▲                              ▲
         │                              │
    [Python Simulator]            [Vite React SPA]
    (Runs on host)                (Runs on host :5173)
```
	
## Repository Structure

```SQL
-- Hypertable for historical GPS data
CREATE TABLE gps_history (
    driver_id      VARCHAR(50) NOT NULL,
    timestamp      TIMESTAMPTZ NOT NULL,
    latitude       DOUBLE PRECISION NOT NULL,
    longitude      DOUBLE PRECISION NOT NULL,
    speed          DOUBLE PRECISION,
    heading        INTEGER,
    accuracy       DOUBLE PRECISION,
    raw_payload    JSONB
);

-- Latest state table (one row per driver)
CREATE TABLE driver_latest_state (
    driver_id      VARCHAR(50) PRIMARY KEY,
    latitude       DOUBLE PRECISION NOT NULL,
    longitude      DOUBLE PRECISION NOT NULL,
    speed          DOUBLE PRECISION,
    heading        INTEGER,
    last_seen      TIMESTAMPTZ NOT NULL,
    status         VARCHAR(20) DEFAULT 'moving'  -- moving, stopped, offline
);
```


## Data Models
** MQTT message model

```JSON
	message = {
		"driver_id": "string"
		"timestamp": "datetime - isoformat",
		"latitude": "float",
		"longitude": "float",
		"speed_kmh": "int",
		"heading_degrees": "float",
		"accuracy_meters": "float",
		"status": "string" "decelerating" else "moving",
		"vehicle_type": "string",
	}
```

```
	message = {
		"driver_id": self.config.driver_id,
		"timestamp": datetime.now(timezone.utc).isoformat(),
		"latitude": round(lat, 6),
		"longitude": round(lng, 6),
		"speed_kmh": round(self.current_speed_kmh, 1),
		"heading_degrees": round(self.heading, 1),
		"accuracy_meters": round(abs(random.gauss(4.0, 1.5)), 1),
		"status": self.status if self.status != "decelerating" else "moving",
		"vehicle_type": self.config.vehicle_type,
	}
	
```

** .net DbBatchWriterWorker Data Model
```c#
 public class GpsPing
 {
     public int Id { get; set; }

     [JsonPropertyName("driver_id")]
     public string DriverId { get; init; } = string.Empty;

     [JsonPropertyName("latitude")]
     public double Latitude { get; init; }

     [JsonPropertyName("longitude")]
     public double Longitude { get; init; }

     [JsonPropertyName("speed_kmh")]
     public double Speed { get; set; }

     [JsonPropertyName("heading_degrees")]
     public double Heading { get; init; }

     [JsonPropertyName("accuracy_meters")]
     public double Accuracy { get; init; }

     [JsonPropertyName("status")]
     public string Status { get; init; } = string.Empty;

     [JsonPropertyName("vehicle_type")]
     public string? VehicleType { get; init; }

     [JsonPropertyName("timestamp")]
     public DateTimeOffset Timestamp { get; init; }

     [JsonIgnore]
     public string? RawPayloadJson { get; set; }

 }
 
 public class DriverLastState
 {
     public string Driver_Id { get; set; } = string.Empty;

     public double Latitude { get; set; }

     public double Longitude { get; set; }

     public double Speed { get; set; }

     public double Heading { get; set; }

     public DateTimeOffset Last_Seen { get; set; }

     public string Status { get; set; } = string.Empty;

 }
 
```

## DbBatchWriterWorker

```
┌─────────────────────────────────────────────────────────────────┐
│                    DbBatchWriterWorker                          │
│                                                                 │
│  ┌─────────────────────┐     ┌──────────────────────────────┐   │
│  │ RedpandaConsumer    │     │  Flush Loop (every 5s)       │   │
│  │ Service             │     │                              │   │
│  │                     │     │  1. GetBatchedPings()        │   │
│  │  Consume() ────────►│     │  2. Compress (TODO)          │   │
│  │       │             │     │  3. BulkInsert (TODO)        │   │
│  │       ▼             │     │  4. UpsertLatest (TODO)      │   │
│  │  ┌──────────────┐   │     │  5. ClearBatch()             │   │
│  │  │  Concurrent  │   │     └──────────────────────────────┘   │
│  │  │  Bag<GpsPing>│   │                                        │
│  │  │  (Buffer)    │   │                                        │
│  │  └──────────────┘   │                                        │
│  └─────────────────────┘                                        │
└─────────────────────────────────────────────────────────────────┘
```


## SignalR Worker - FleetPulse.SignalRHub

The `FleetPulse.SignalRHub` is an ASP.NET Core 10 Minimal API + SignalR server. It has two responsibilities:

1. **Real-time push** – A `BackgroundService` (`GpsPingConsumer`) consumes the `gps-pings` Kafka topic and fans every deserialized ping out to all connected browser clients over WebSockets.
2. **REST query layer** – A set of Minimal API endpoints let the SPA bootstrap its state on load (latest driver positions, GPS history, AI alerts) by reading directly from TimescaleDB via Dapper + Npgsql.

### Architecture

```
┌──────────────────────────────────────────────────────────────────┐
│                        FleetPulse.SignalRHub                     │
│                                                                  │
│  ┌────────────────────────────┐    ┌──────────────────────────┐  │
│  │  GpsPingConsumer           │    │  FleetHub (SignalR)      │  │
│  │  (BackgroundService)       │    │                          │  │
│  │                            │    │  SubscribeFleet(fleetId) │  │
│  │  Kafka Topic: gps-pings    │    │  UnsubscribeFleet(...)   │  │
│  │       │                    │    │                          │  │
│  │       ▼                    │    │  Group: "fleet:{id}"     │  │
│  │  Deserialize MessageWrapper│    └──────────────────────────┘  │
│  │       │                    │              ▲                   │
│  │       ▼                    │              │                   │
│  │  Throttle (max 2 Hz /      │──SendAsync──►│ IHubContext       │
│  │   driver, 500 ms window)   │  ReceiveGpsPing                  │
│  └────────────────────────────┘                                  │
│                                                                  │
│  ┌──────────────────────────────────────────────────────────┐    │
│  │  REST Endpoints (Minimal API, v1)                        │    │
│  │                                                          │    │
│  │  GET /api/v1/drivers?from=<datetime>                     │    │
│  │  GET /api/v1/drivers/{id}/history?from=&to=              │    │
│  │  GET /api/v1/alerts?from=&to=&limit=                     │    │
│  └────────────────┬─────────────────────────────────────────┘    │
│                   │  Dapper + Npgsql                             │
│                   ▼                                              │
│            TimescaleDB (PostgreSQL)                              │
└──────────────────────────────────────────────────────────────────┘
```

### Default API Version

`v1`

### REST Endpoints

| Method | Path | Query params | Description |
| :--- | :--- | :--- | :--- |
| `GET` | `/` | — | Welcome message |
| `GET` | `/health` | — | Health probe |
| `GET` | `/dbversion` | — | Returns the connected PostgreSQL version string |
| `GET` | `/api/v1/drivers` | `from` (DateTime) | Returns the latest state of all drivers active after `from` |
| `GET` | `/api/v1/drivers/{id}/history` | `from`, `to` (DateTime) | Returns the GPS track of a single driver between two timestamps |
| `GET` | `/api/v1/alerts` | `from`, `to` (DateTime), `limit` (int, default 50) | Returns AI-generated alerts within the time window |

### Response Contracts

**`LastestDriverStateResponse`** – returned by `/api/v1/drivers`
```json
{
  "driverId":  "string",
  "latitude":  0.0,
  "longitude": 0.0,
  "speed":     0.0,
  "heading":   0.0,
  "lastSeen":  "string (ISO-8601)",
  "status":    "string"
}
```

**`GpsHistoryResponse`** – returned by `/api/v1/drivers/{id}/history`
```json
{
  "driverId":  "string",
  "latitude":  0.0,
  "longitude": 0.0,
  "speed":     0.0,
  "heading":   0.0,
  "timestamp": "string (ISO-8601)"
}
```

**`AlertResponse`** – returned by `/api/v1/alerts`
```json
{
  "id":          "string",
  "driverId":    "string",
  "alertType":   "string",
  "severity":    "string",
  "description": "string",
  "context":     "string",
  "createdAt":   "string (ISO-8601)"
}
```

### SignalR Hub

| Property | Value |
| :--- | :--- |
| Hub URI | `/v1/fleetHub` |
| Server → Client callback | `ReceiveGpsPing` |
| Payload | `GpsPingDto` (see below) |

Clients can subscribe to a specific fleet's updates to receive targeted pushes in the future (group-based fan-out is wired; the current implementation broadcasts to `Clients.All`):

```js
// Connect
const conn = new signalR.HubConnectionBuilder().withUrl("/v1/fleetHub").build();

// Subscribe to fleet updates
await conn.invoke("SubscribeFleet", "fleet-42");

// Receive real-time GPS pings
conn.on("ReceiveGpsPing", (ping) => { /* update map marker */ });
```

**`GpsPingDto`** – pushed by `ReceiveGpsPing`
```json
{
  "driver_id":        "string",
  "latitude":         0.0,
  "longitude":        0.0,
  "speed_kmh":        0.0,
  "heading_degrees":  0.0,
  "accuracy_meters":  0.0,
  "status":           "string",
  "vehicle_type":     "string | null",
  "timestamp":        "string (ISO-8601)"
}
```

### Kafka / Message Envelope

Messages on the `gps-pings` topic are wrapped in a `MessageWrapper` envelope before the actual ping payload:

```json
{
  "kafka_key":  "string",
  "client_id":  "string",
  "payload":    "<JSON-encoded GpsPingDto>"
}
```

`GpsPingConsumer` unwraps the envelope and deserializes the inner `payload` field before forwarding to clients.

### Throttling (Back-pressure)

To prevent overwhelming the browser with high-frequency updates, `GpsPingConsumer` enforces a **per-driver rate limit of 2 Hz (500 ms minimum interval)**. Messages that arrive faster than this cadence for a given driver are silently dropped on the server side.

### CORS

In development mode the server allows credentials from the Vite SPA at `http://localhost:5173`. Credentials must be allowed because SignalR uses cookies/tokens for the WebSocket handshake.

### Configuration (`appsettings.json`)

```json
{
  "ConnectionStrings": {
    "FleetPulseDb": "Host=timescaledb-1;Port=5432;Database=fleetpulse;Username=...;Password=..."
  },
  "Kafka": {
    "BootstrapServers": "localhost:19092",
    "GroupId":          "fleetpulse-hub-consumer",
    "Topic":            "gps-pings",
    "AutoOffsetReset":  "Earliest",
    "EnableAutoCommit": "true"
  },
  "SignalR": {
    "CallbackMethod": "ReceiveGpsPing"
  }
}
```

### Design Decisions

| Decision | Rationale |
| :--- | :--- |
| `BackgroundService` for Kafka consumption | Ties the consumer lifecycle to the ASP.NET host; clean startup/shutdown with `CancellationToken` propagation. Consumer runs on a `Task.Run` thread-pool thread so it does not block app startup. |
| Minimal API instead of MVC controllers | Reduces ceremony for a thin read-only query layer; keeps the service boundary explicit. |
| Mapster for DTO → Response mapping | Zero-reflection mapping configuration with compile-time safety; registered once at startup via `MappingConfig.RegisterMappings()`. |
| Dapper over EF Core | TimescaleDB queries are hand-crafted SQL (time-series aggregations). Dapper gives full SQL control without the overhead of an ORM change-tracker. |
| `NpgsqlDataSource` singleton | Handles connection pooling internally; a single `NpgsqlDataSource` is the recommended usage pattern for Npgsql 7+. |
| Global exception handler (`IExceptionHandler`) | Centralises ProblemDetails error responses; maps `ValidationException` → 422 and `UnauthorizedAccessException` → 401, everything else → 500. |
| Per-driver throttle (2 Hz) in-process | Avoids saturating the SignalR backplane and the browser event loop when GPS simulators produce bursts. A sliding-window `Dictionary<string, DateTimeOffset>` is sufficient for a single-instance deployment. |


