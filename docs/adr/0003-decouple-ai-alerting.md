# Architecture Decision Records 0003

**Decoupled AI Alerting**

The AI worker does not push alerts directly to the frontend. Instead, it evaluates the gps-pings stream, and if an anomaly is detected (e.g., a driver is stationary in a high-risk zone), it publishes a new event to a separate Kafka topic: ai-alerts.

The SignalR Worker subscribes to both topics (gps-pings and ai-alerts) and acts as the single gateway to the browser. This ensures the frontend remains completely agnostic to how many backend workers are processing data behind the scenes.

