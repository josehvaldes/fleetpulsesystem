

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

| Method | Path | Auth | Query params | Description |
| :--- | :--- | :--- | :--- | :--- |
| `GET` | `/` | No | — | Welcome message |
| `GET` | `/health` | No | — | Health probe |
| `GET` | `/healthz` | No | — | ASP.NET Core health checks endpoint |
| `GET` | `/dbversion` | Yes | — | Returns the connected PostgreSQL version string |
| `POST` | `/api/v1/login` | No | — (body: `LoginRequest`) | Authenticates a user and returns an access token |
| `GET` | `/api/v1/drivers` | Yes | `from` (DateTime, required), `to` (DateTime, optional) | Returns the latest state of drivers matching the date range |
| `GET` | `/api/v1/drivers/{id}/history` | Yes | `from` (DateTime, required), `to` (DateTime, required) | Returns a driver's GPS history within the date range |
| `GET` | `/api/v1/alerts` | Yes | `pagesize` (int, required), `pagenumber` (int, required), `status` (string, optional), `riskLevel` (string, optional), `from` (DateTime, optional), `to` (DateTime, optional) | Returns a paged list of alerts matching the supplied filters |

All `/api/v1/*` endpoints except `/api/v1/login` require authorization. The `/dbversion` endpoint also requires authorization.

### Response Contracts

**`LoginResponse`** – returned by `POST /api/v1/login`
```json
{
  "accessToken": "string",
  "tokenType": "Bearer",
  "expiresIn": 0,
  "username": "string"
}
```

**`LastestDriverStateResponse[]`** – returned by `GET /api/v1/drivers`
```json
[
  {
    "driverId": "string",
    "latitude": 0.0,
    "longitude": 0.0,
    "speed": 0.0,
    "heading": 0.0,
    "lastSeen": "string",
    "status": "string"
  }
]
```

**`GpsPingResponse[]`** – returned by `GET /api/v1/drivers/{id}/history`
```json
[
  {
    "driverId": "string",
    "latitude": 0.0,
    "longitude": 0.0,
    "speed": 0.0,
    "heading": 0.0,
    "timestamp": "string"
  }
]
```

**`PagedResponse<AlertResponse>`** – returned by `GET /api/v1/alerts`
```json
{
  "items": [
    {
      "id": "string",
      "driverId": "string",
      "eventLatitude": 0.0,
      "eventLongitude": 0.0,
      "exitSpeed": 0.0,
      "exitTime": "string",
      "zoneName": "string",
      "zoneType": "string",
      "riskLevel": "string",
      "status": "string",
      "assessment": "string",
      "recommendation": "string",
      "autoEscalate": false,
      "raisedAt": "string"
    }
  ],
  "totalCount": 0,
  "pageNumber": 0,
  "pageSize": 0,
  "totalPages": 0,
  "hasPreviousPage": false,
  "hasNextPage": false
}
```
### Authentication models
Authentication is configured under `Authentication` in `appsettings.json`. The `Authentication:Mode` setting enables local JWT validation, Microsoft Entra ID JWT validation, or both. The checked-in configuration uses `both`.

| Mode | Behavior |
| :--- | :--- |
| `local` | Enables validation of locally issued JWTs using the signing secret, issuer, and audience in `Authentication:Local:Jwt`. The `/api/v1/login` endpoint authenticates against the configured default credentials in `Authentication:Local:Auth` and issues a local access token. |
| `msal` | Enables JWT bearer validation for tokens issued by Microsoft Entra ID, using `Authentication:EntraId:TenantId` and `Authentication:EntraId:ClientId`. The API validates Entra ID access tokens; MSAL is typically used by a client to acquire those tokens. |
| `both` | Enables both validators. The `Smart` policy scheme examines the bearer token issuer and forwards Entra ID issuers to the Entra ID scheme; other tokens are forwarded to the local scheme. |

In `local` or `both` mode, configure `Authentication:Local:Jwt:Secret`, `Issuer`, and `Audience`; use a sufficiently strong secret and supply it through deployment secrets or environment configuration. Configure the local login values at `Authentication:Local:Auth:DefaultUserId`, `DefaultUsername`, and `DefaultPassword`. The current login service compares the submitted username and password with those configured default values and rejects mismatches.

In `msal` or `both` mode, `Authentication:EntraId:TenantId` and `ClientId` are required. Local and Entra ID scheme names are configurable at `Authentication:Local:Jwt:Scheme` and `Authentication:EntraId:Scheme`; they must differ when `Mode` is `both`.

Protected endpoints and the SignalR hub use authorization. The `FleetUser` policy requires the `scope` claim to contain `access_as_user`, while `FleetAdminOnly` requires the `Fleet.admin` role; these policies are available for endpoints that explicitly select them. For SignalR connections, bearer tokens may be supplied as the `access_token` query parameter on `/v1/fleetHub`.

### SignalR Hub

| Property | Value |
| :--- | :--- |
| Hub URI | `/v1/fleetHub` |
| GPS callback | `ReceiveGpsPing` (configured by `SignalR:GpsPingCallbackMethod`) |
| GPS payload | `GpsPing` domain model (see below) |
| Alert callback | `ReceiveAlert` (configured by `SignalR:AlertCallbackMethod`) |
| Alert payload | `AlertResponse` contract (see below) |

Kafka consumers do not call the hub directly. They use the application-facing `IRealTimeNotifier` interface, implemented by `RealTimeNotifier` with `IHubContext<FleetHub>`. `GpsPingConsumer` sends GPS pings through `SendgpsPingToAllAsync`; `AlertConsumer` sends alerts through `SendAlertToAllAsync`. The notifier broadcasts both callbacks to all connected clients. It maps alert entities to `AlertResponse` before sending; GPS pings are sent as the `GpsPing` model.

The hub exposes `SubscribeFleet(fleetId)` and `UnsubscribeFleet(fleetId)` for joining or leaving the `fleet:{fleetId}` SignalR group. The notifier also supports group sends, but the current Kafka consumers use broadcast methods, not group-targeted sends.

```js
const conn = new signalR.HubConnectionBuilder().withUrl("/v1/fleetHub").build();
conn.on("ReceiveGpsPing", (ping) => { /* update GPS state */ });
conn.on("ReceiveAlert", (alert) => { /* update alert state */ });

await conn.start();
await conn.invoke("SubscribeFleet", "fleet-42");
```

**`GpsPing`** – pushed by `ReceiveGpsPing` (property names are sent as shown)
```json
{
  "event_id": "string",
  "driver_id": "string",
  "latitude": 0.0,
  "longitude": 0.0,
  "speed": 0.0,
  "heading": 0.0,
  "accuracy": 0.0,
  "status": "string",
  "timestamp": "string (ISO-8601)"
}
```

**`AlertResponse`** – pushed by `ReceiveAlert`; the alert entity is adapted to this API contract before broadcast
```json
{
  "id": "string",
  "driverId": "string",
  "eventLatitude": 0.0,
  "eventLongitude": 0.0,
  "exitSpeed": 0.0,
  "exitTime": "string (ISO-8601)",
  "zoneName": "string",
  "zoneType": "string",
  "riskLevel": "string",
  "status": "string",
  "assessment": "string",
  "recommendation": "string",
  "autoEscalate": false,
  "raisedAt": "string (ISO-8601)"
}
```

### Kafka Input Messages

The SignalR Hub Kafka consumers deserialize each topic message directly as JSON; they do not unwrap a `MessageWrapper`. `GpsPingConsumer` reads `GpsPingDto` messages from `gps-pings`, applies per-driver throttling, maps them to `GpsPing`, and sends them through `IRealTimeNotifier`. Its input fields include:

```json
{
  "driver_id": "string",
  "latitude": 0.0,
  "longitude": 0.0,
  "speed_kmh": 0.0,
  "heading_degrees": 0.0,
  "accuracy_meters": 0.0,
  "status": "string",
  "vehicle_type": "string or null",
  "timestamp": "string (ISO-8601)"
}
```

`AlertConsumer` reads `AlertDto` messages directly from `ai-alerts`, maps them to the domain `Alert`, and sends them through the same notifier interface. Its input includes driver and exit-location data, exit speed/heading/time, zone details, agent risk/assessment/recommendation/escalation fields, and creation time. These Kafka input DTOs differ from the client-facing SignalR payloads documented above.

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


## Backend Clean Architecture and CQRS

The FleetPulse.SignalRHub follows the Clean Architecture organization and dependency fLow, and the CQRS implementation with "Mediator.SourceGenerator"