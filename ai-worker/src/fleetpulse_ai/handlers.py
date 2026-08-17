
from datetime import datetime, timezone
from typing import Awaitable, Callable

import structlog
from fleetpulse_ai.logging_config import get_logger
from fleetpulse_ai.agents.alarm_analyzer_agent import AlarmAnalyzerAgent
from fleetpulse_ai.detectors.working_zone_violation import WorkingZoneViolationDetector
from fleetpulse_ai.managers.alert_manager import AlertManager
from fleetpulse_ai.models.gps_ping import GpsPing
from fleetpulse_ai.models.alert_event import AlertEvent
from fleetpulse_ai.prometheus import PINGS_PROCESSED, ALERTS_PUBLISHED, ANOMALY_DETECTION_DURATION
from fleetpulse_ai.mock_data import MOCK_DATA_CONTEXT  # Assuming this is defined somewhere in your codebase

from opentelemetry import trace
from opentelemetry import propagate
from opentelemetry.trace import SpanKind

MAX_HISTORY_LENGTH = 10

tracer = trace.get_tracer(__name__)
logger = get_logger(__name__)

def create_ai_worker_handler(
    detector: WorkingZoneViolationDetector,
    manager: AlertManager,
    agent: AlarmAnalyzerAgent
) -> Callable[[dict, dict], Awaitable[None]]:
    driver_history: dict[str, list[GpsPing]] = {}

    async def ai_worker_handler(message: dict, metadata: dict) -> None:
        """
        Process a GPS ping message with the detector and alert manager bound for the worker lifetime.
        """
        print(f" - Message {message}") 

        carrier = {
            "traceparent": metadata.get("traceparent", ""),
            "tracestate": metadata.get("tracestate", ""),
        }
        context = propagate.extract(carrier)

        point = GpsPing(
            latitude=float(message["latitude"]),
            longitude=float(message["longitude"]),
            speed_kmh=float(message["speed_kmh"]),
            heading_degrees=float(message["heading_degrees"]),
            timestamp=message["timestamp"],
        )

        driver_id = message["driver_id"]
        history = driver_history.setdefault(driver_id, [])

        with tracer.start_as_current_span(
                "process_gps_ping", context=context, kind=SpanKind.CONSUMER,
            ) as gpsspan:

            if len(history) >= MAX_HISTORY_LENGTH:
                history.pop(0)

            history.append(point)
            PINGS_PROCESSED.labels(anomaly_detected="false").inc()  # Default to false; will update if anomaly detected

            with structlog.contextvars.bound_contextvars(driver_id=driver_id):

                if len(history) < 2:
                    return

                violation_event = await detector.analyze(driver_id, history)
                if violation_event:
                    logger.info("violation_detected", violation_zone=violation_event.zone_name, violation_type=violation_event.zone_type, exit_time=violation_event.exit_time)

                    # Retrieve driver context from MOCK_DRIVER_CONTEXT. Data will come from database later, but for now we can use a mock.
                    driver_context = MOCK_DATA_CONTEXT.get(driver_id, {})

                    with ANOMALY_DETECTION_DURATION.labels(anomaly_type="working_zone_violation").time():
                        agent_response = await agent.analyze_alert(violation_event, context=driver_context)

                    alert = AlertEvent(
                        driver_id=violation_event.driver_id,
                        exit_location=violation_event.exit_location,
                        exit_speed=violation_event.exit_speed,
                        exit_heading=violation_event.exit_heading,
                        exit_time=violation_event.exit_time,
                        zone_name=violation_event.zone_name,
                        zone_type=violation_event.zone_type,
                        agent_risk_level=agent_response.risk_level,
                        agent_assessment=agent_response.assessment,
                        agent_recommendation=agent_response.recommended_action,
                        agent_auto_escalate=agent_response.auto_escalate,
                        created_at= datetime.now(timezone.utc).isoformat()
                    )

                    PINGS_PROCESSED.labels(anomaly_detected="true").inc()
                    ALERTS_PUBLISHED.labels(severity=alert.agent_risk_level).inc()

                    logger.info("alert_published", agent_risk_level=alert.agent_risk_level)

                    with tracer.start_as_current_span(
                        "publish_alert", context=context, kind=SpanKind.PRODUCER,
                    ) as alertspan:
                        carrier = {}
                        propagate.inject(carrier)
                        alertspan.set_attribute("alert.driver_id", alert.driver_id)
                        alertspan.set_attribute("alert.zone_name", alert.zone_name)
                        alertspan.set_attribute("alert.zone_type", alert.zone_type)
                        alertspan.set_attribute("alert.agent_risk_level", alert.agent_risk_level)
                        alertspan.set_attribute("alert.agent_auto_escalate", alert.agent_auto_escalate)

                        alert.traceparent = carrier.get("traceparent")
                        alert.tracestate = carrier.get("tracestate")

                        await manager.handle_alert(alert)
                    

    return ai_worker_handler
