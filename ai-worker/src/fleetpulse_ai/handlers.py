
from typing import Awaitable, Callable

from fleetpulse_ai.agents.alarm_analyzer_agent import AlarmAnalyzerAgent
from fleetpulse_ai.detectors.working_zone_violation import WorkingZoneViolationDetector
from fleetpulse_ai.managers.alert_manager import AlertManager
from fleetpulse_ai.models.gps_ping import GpsPing
from fleetpulse_ai.models.alert_event import AlertEvent
from fleetpulse_ai.prometheus import PINGS_PROCESSED, ALERTS_PUBLISHED, ANOMALY_DETECTION_DURATION

MAX_HISTORY_LENGTH = 10

def create_ai_worker_handler(
    detector: WorkingZoneViolationDetector,
    manager: AlertManager,
    agent: AlarmAnalyzerAgent
) -> Callable[[dict], Awaitable[None]]:
    driver_history: dict[str, list[GpsPing]] = {}

    async def ai_worker_handler(message: dict) -> None:
        """
        Process a GPS ping message with the detector and alert manager bound for the worker lifetime.
        """

        point = GpsPing(
            latitude=float(message["latitude"]),
            longitude=float(message["longitude"]),
            speed_kmh=float(message["speed_kmh"]),
            heading_degrees=float(message["heading_degrees"]),
            timestamp=message["timestamp"],
        )

        driver_id = message["driver_id"]
        history = driver_history.setdefault(driver_id, [])

        if len(history) >= MAX_HISTORY_LENGTH:
            history.pop(0)

        history.append(point)
        PINGS_PROCESSED.labels(anomaly_detected="false").inc()  # Default to false; will update if anomaly detected
        if len(history) < 2:
            print(f"Not enough data to analyze for driver {driver_id}. Current history length: {len(history)}")
            return

        violation_event = await detector.analyze(driver_id, history)
        if violation_event:
            print(f"Violation detected for driver {driver_id}: {violation_event.to_dict()}")
            # Add more context to the violation event for the agent later
            
            with ANOMALY_DETECTION_DURATION.labels(anomaly_type="working_zone_violation").time():
                agent_response = await agent.analyze_alert(violation_event, context={"driver_id": driver_id})

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
                agent_auto_escalate=agent_response.auto_escalate
            )

            PINGS_PROCESSED.labels(anomaly_detected="true").inc()
            ALERTS_PUBLISHED.labels(severity=alert.agent_risk_level).inc()
            await manager.handle_alert(alert)
            

    return ai_worker_handler
