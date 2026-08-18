from pathlib import Path

import pytest

from fleetpulse_ai.events.violation_event import ViolationEvent
from fleetpulse_ai.main import load_driver_zones
from fleetpulse_ai.handlers import create_ai_worker_handler
from fleetpulse_ai.models.agent_alert_response import AgentAlertResponse
from fleetpulse_ai.models.alert_event import AlertEvent


def test_load_driver_zones_reads_repo_data() -> None:
    data_dir = Path(__file__).resolve().parents[2] / "data"

    zones = load_driver_zones(data_dir=data_dir)

    assert "QWE-1234" in zones
    assert zones["QWE-1234"][0] == "zone_1_south"


@pytest.mark.asyncio
async def test_create_ai_worker_handler_uses_injected_dependencies() -> None:
    class FakeDetector:
        def __init__(self) -> None:
            self.calls = 0

        async def analyze(self, driver_id: str, history: list) -> ViolationEvent | None:
            self.calls += 1
            if len(history) < 2:
                return None

            return ViolationEvent(
                driver_id=driver_id,
                exit_location={"latitude": history[-1].latitude, "longitude": history[-1].longitude},
                exit_speed=history[-1].speed_kmh,
                exit_heading=history[-1].heading_degrees,
                exit_time=history[-1].timestamp,
                zone_name="zone_1_south",
                zone_type="working_zone",
            )

    class FakeManager:
        def __init__(self) -> None:
            self.handled_events: list[ViolationEvent] = []

        async def handle_alert(self, event: ViolationEvent) -> None:
            self.handled_events.append(event)

    class FakeAgent:
        async def analyze_alert(self, event: ViolationEvent, context: dict) -> AgentAlertResponse:
            return AgentAlertResponse(risk_level="high", recommended_action="review", assessment="violation detected", auto_escalate=True)
        
    detector = FakeDetector()
    manager = FakeManager()
    agent = FakeAgent()
    handler = create_ai_worker_handler(detector, manager, agent)

    base_message = {
        "driver_id": "driver_001",
        "latitude": -17.373349,
        "longitude": -66.15688,
        "speed_kmh": 46.0,
        "heading_degrees": 0.0,
        "timestamp": "2026-07-27T19:46:20.843397+00:00",
    }
    metadata = {
        "traceparent": "00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01",
    }

    await handler(base_message, metadata)
    await handler({
        **base_message,
        "latitude": -17.373154,
        "longitude": -66.156813,
        "speed_kmh": 43.6,
        "heading_degrees": 60.5,
        "timestamp": "2026-07-27T19:46:22.769488+00:00",
    }, metadata)

    assert detector.calls == 1
    assert len(manager.handled_events) == 1
    assert manager.handled_events[0].driver_id == "driver_001"


def test_alert_event_serializes_trace_context() -> None:
    alert = AlertEvent(
        driver_id="driver_001",
        exit_location={"latitude": -17.373349, "longitude": -66.15688},
        exit_speed=46.0,
        exit_heading=0.0,
        exit_time="2026-07-27T19:46:20.843397+00:00",
        zone_name="zone_1_south",
        zone_type="working_zone",
        traceparent="00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01",
        tracestate="rojo=00f067aa0ba902b7",
    )

    payload = alert.to_dict()

    assert payload["traceparent"] == alert.traceparent
    assert payload["tracestate"] == alert.tracestate
