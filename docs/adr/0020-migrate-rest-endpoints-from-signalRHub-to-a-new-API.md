# Architecture Decision Records Template 

## Title:
ADR 0020: Migrate REST API Endpoints from SignalRHub to a New Dedicated API

## Status: 
Accepted

## Date: 
2026-10-06

## Context
The FleetPulse.SignalRHub service currently serves two fundamentally different architectural responsibilities:

1. Stateful Real-Time Push: Maintaining persistent WebSocket connections via SignalR, consuming Kafka topics (gps-pings, ai-alerts), throttling payloads, and broadcasting to connected clients.

2. Stateless REST Queries: Serving Minimal API requests (/api/v1/drivers, /api/v1/alerts) querying TimescaleDB via Dapper.

Combining these workloads creates an architectural mismatch. WebSockets are memory/state-intensive and require sticky sessions if scaled out. REST queries are stateless, compute-light, and scale perfectly horizontally. Keeping them together forces suboptimal scaling profiles and complicates future iterations on either domain.

To modernize the system incrementally, we are applying the *Strangler Fig pattern*. We will extract the REST query layer into a new dedicated service (FleetPulse.Api), while leaving the WebSocket hub and local login (/api/v1/login) in the original FleetPulse.SignalRHub service.

Because the frontend consists of a React SPA and two Micro-Frontends (MFEs) that expect a single API origin, the migration must be transparent to the client.

## Decision
We will implement a *YARP (Yet Another Reverse Proxy)* gateway as a facade to route traffic to the appropriate backend, and we will utilize a shared Resource Server authentication model.

1. Service Split:
 - FleetPulse.SignalRHub: Will retain /v1/fleetHub (SignalR) and /api/v1/login (local JWT issuer).
 - FleetPulse.Api: Will handle /api/v1/drivers and /api/v1/alerts (stateless queries).

2. Routing Facade: A YARP gateway service will act as the single ingress point. It will route requests based on path to the respective downstream service, keeping the frontend completely agnostic to the backend split.

3. Shared Authentication Model:
 - Both FleetPulse.SignalRHub and FleetPulse.Api will act as independent Resource Servers (validators), not as issuers.
 - Entra ID: We will use a single Entra ID App Registration for both APIs, sharing the api://{apiID}/access_as_user scope. The new API will trust the same audience.
 - Local JWT: FleetPulse.SignalRHub remains the sole issuer of local JWTs. FleetPulse.Api will validate these local tokens using the exact same signing secret, issuer, and audience parameters.
 - Both services will implement the "Smart" policy scheme to dynamically route validation to the Entra ID or Local JWT scheme based on the token's issuer claim.


## Alternatives Considered

** Alternative 1: Gateway-Level Authentication (YARP as Auth Broker)
In this scenario, YARP would validate all incoming tokens. It would require a new Entra ID registration for the YARP service itself. The gateway would strip the incoming token and forward claims as internal headers to the downstream services, which would no longer perform token validation.

 - *Rejected because:* The long-term usage of the YARP service is still under discussion (it may be replaced by a cloud-native API Gateway like AWS API Gateway or Azure API Management in the future). Coupling core authentication logic to a potentially temporary component introduces unnecessary risk. Furthermore, standard microservices best practice dictates that services should own their own AuthN/AuthZ boundaries.

** Alternative 2: Separate Entra ID App Registrations per Service
Creating a new App Registration (api://fleetpulse-api/access_as_user) for the new API.

 - *Rejected because:* This would require the frontend SPA to perform incremental consent and acquire a second access token specifically for the new API. This violates the transparency goal of the Strangler Fig pattern and adds unnecessary complexity to the frontend. (Acknowledged: In a production enterprise system with distinct API ownership, this would be the preferred approach, but it is over-engineered for this portfolio project).

** Alternative 3: Nginx Reverse Proxy
Deploy an Nginx container to act as the gateway. Nginx natively handles path-based routing for the Strangler Fig pattern via location blocks and could support future canary deployments using the split_clients module.

 - *Rejected because:* While Nginx is an industry-standard tool that demonstrates strong infrastructure skills, YARP was chosen for this portfolio project to provide a fully integrated, .NET-native ecosystem. YARP offers seamless integration with the ASP.NET Core middleware pipeline, making WebSocket configuration and connection management for SignalR easier to implement and debug compared to Nginx's manual Upgrade header configurations.

## Consequences

** Pros (Benefits)
 - Independent Scaling: Stateless REST endpoints in FleetPulse.Api can now be scaled horizontally based on query load, without dragging along memory-heavy WebSocket connections.
 - Zero Frontend Impact: By using YARP as a facade, the SPA and MFEs continue to call a single origin. The migration is completely transparent.
 - Clean Separation of Concerns: Push-based streaming logic (Kafka -> SignalR) is fully decoupled from pull-based query logic (Dapper -> TimescaleDB).
 - Simplified Auth for Frontend: Sharing the Entra ID App Registration means the frontend continues to request a single token scope (access_as_user).

** Cons (Trade-offs & Risks)
 - Configuration Duplication: The authentication configuration (Entra ID TenantId/ClientId, Local JWT Secrets, "Smart" scheme logic) must be maintained in both backend services.
 - Network Hop: Introduces an extra network hop through YARP, adding negligible (~1-2ms) latency to REST requests, but architecturally present.
 - WebSocket Proxying: YARP must be explicitly configured to support WebSockets (app.UseWebSockets()) to correctly proxy SignalR traffic. If the SignalR hub is scaled to multiple replicas in the future, YARP will require sticky sessions configuration.

## Related ADRS
 - 0018-support-local-JWT-and-MS-EntraID-authentication.md
