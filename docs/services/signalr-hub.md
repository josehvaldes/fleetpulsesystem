

## SignalR Worker - FleetPulse.SignalRHub

The `FleetPulse.SignalRHub` is an ASP.NET Core 10 Minimal API + SignalR server. It has two responsibilities:

1. **Real-time push** – A `BackgroundService` (`GpsPingConsumer`) consumes the `gps-pings` Kafka topic and fans every deserialized ping out to all connected browser clients over WebSockets.
2. **REST query layer** – A set of Minimal API endpoints let the SPA bootstrap its state on load (latest driver positions, GPS history, AI alerts) by reading directly from TimescaleDB via Dapper + Npgsql.

### Architecture

```
┌─────────────────────────────────────────────────────────────────────┐
│                        FleetPulse.SignalRHub                        │
│                                                                     │
│  ┌────────────────────────────────┐    ┌──────────────────────────┐ │
│  │  GpsPingConsumer/AlertConsumer │    │  FleetHub (SignalR)      │ │
│  │      (BackgroundService)       │    │                          │ │
│  │                                │    │  SubscribeFleet(fleetId) │ │
│  │  Kafka Topic: gps-pings        │    │  UnsubscribeFleet(...)   │ │
│  │       │                        │    │                          │ │
│  │       ▼                        │    │  Group: "fleet:{id}"     │ │
│  │  Deserialize MessageWrapper    │    └──────────────────────────┘ │
│  │       │                        │              ▲                  │
│  │       ▼                        │              │                  │
│  │  Throttle (max 2 Hz /          │──SendAsync──►│ IHubContext      │
│  │   driver, 500 ms window)       │   ReceiveGpsPing/ReceiveAlert   │
│  └────────────────────────────────┘                                 │
│                                                                     │
│  ┌──────────────────────────────────────────────────────────┐       │
│  │  REST Endpoints (Minimal API, v1)                        │       │
│  │                                                          │       │
│  │  GET /api/v1/drivers?from=<datetime>                     │       │
│  │  GET /api/v1/drivers/{id}/history?from=&to=              │       │
│  │  GET /api/v1/alerts?from=&to=&limit=                     │       │
│  └────────────────┬─────────────────────────────────────────┘       │
│                   │  Dapper + Npgsql                                │
│                   ▼                                                 │
│            TimescaleDB (PostgreSQL)                                 │
└─────────────────────────────────────────────────────────────────────┘
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
    "id": string,
    "driverId": string,
    "eventLatitude": number,
    "eventLongitude": number,
    "exitSpeed": number,
    "exitTime": string,
    "zoneName": string,
    "zoneType": string,
    "riskLevel": string,
    "status": string,
    "assessment": string,
    "recommendation": string,
    "autoScale": bool,
    "raisedAt": string
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