

## AI-Worker

The AI worker is a Python-based streaming service that listens to vehicle GPS events, detects when a driver leaves their assigned working zone, and publishes an AI-assisted alert for downstream consumers.

### Current implementation

- AI platform: Azure OpenAI, accessed through `langchain_openai` with Azure authentication.
- Input stream: the worker consumes GPS pings from the Kafka/Redpanda topic `gps-pings`.
- Detection logic: a `WorkingZoneViolationDetector` compares the previous and current GPS points for a driver and raises a violation when the driver moves from inside to outside their assigned zone polygon.
- Geospatial context: zone polygons are loaded from the repository data directory, including `data/allowed_zones/*.geojson` and `data/driver_zones_mapping.json`.
- AI analysis: once a violation is detected, an `AlarmAnalyzerAgent` evaluates the event and returns a structured response with `risk_level`, `assessment`, `recommended_action`, and `auto_escalate`.
- Alert publishing: an `AlertManager` publishes the resulting alert to the configured output topic (currently defaulting to `alerts`; older project references may still use `ai-alerts`).

### Processing flow

1. Consume a GPS ping from `gps-pings`.
2. Keep a short rolling history per driver (up to 10 pings).
3. Detect a working-zone violation using the latest two points in that history.
4. Send the violation context to the Azure OpenAI agent for assessment.
5. Publish the enriched alert event for downstream processing.

### Key components

- `fleetpulse_ai/main.py`: worker entry point and orchestration flow.
- `fleetpulse_ai/detectors/working_zone_violation.py`: rule-based violation detection.
- `fleetpulse_ai/agents/alarm_analyzer_agent.py`: Azure OpenAI-based alert assessment.
- `fleetpulse_ai/managers/kafka_consumer.py` and `fleetpulse_ai/managers/alert_manager.py`: Kafka/Redpanda input/output integration.
- `fleetpulse_ai/settings.py`: runtime configuration for Azure OpenAI, Kafka broker settings, and topic names.

### Notes

This is currently a rules-plus-LLM workflow rather than a fully autonomous multi-agent system. The core behavior is already implemented and is designed to evolve as the alerting heuristics become more sophisticated.
