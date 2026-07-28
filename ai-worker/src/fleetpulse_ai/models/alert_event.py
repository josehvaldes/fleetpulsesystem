from dataclasses import dataclass
import json

from fleetpulse_ai.events.violation_event import ViolationEvent
from fleetpulse_ai.models.agent_alert_response import AgentAlertResponse

@dataclass
class AlertEvent:
    """Base class for alert events."""
    driver_id: str
    exit_location: dict[str, float]
    exit_speed: float
    exit_heading: float
    exit_time: str | None = None
    zone_name: str | None = None
    zone_type: str | None = None

    agent_risk_level: str | None = None
    agent_assessment: str | None = None
    agent_recommendation: str | None = None
    agent_auto_escalate: bool | None = None

    def update_alert_event(self, agent_response: AgentAlertResponse, event: ViolationEvent) -> None:
        """Update the alert event with the agent's response."""

        self.driver_id = event.driver_id
        self.exit_location = event.exit_location
        self.exit_speed = event.exit_speed
        self.exit_heading = event.exit_heading
        self.exit_time = event.exit_time
        self.zone_name = event.zone_name
        self.zone_type = event.zone_type

        self.agent_risk_level = agent_response.risk_level
        self.agent_assessment = agent_response.assessment
        self.agent_recommendation = agent_response.recommendation
        self.agent_auto_escalate = agent_response.auto_escalate

    def to_dict(self) -> dict:
        """Convert the event to a dictionary."""
        return {
            "driver_id": self.driver_id,
            "exit_location": self.exit_location,
            "exit_speed": self.exit_speed,
            "exit_heading": self.exit_heading,
            "exit_time": self.exit_time,
            "zone_name": self.zone_name,
            "zone_type": self.zone_type,
            "agent_risk_level": self.agent_risk_level,
            "agent_assessment": self.agent_assessment,
            "agent_recommendation": self.agent_recommendation,
            "agent_auto_escalate": self.agent_auto_escalate
        }

    def to_json(self) -> str:
        """Convert the event to a JSON string."""
        return json.dumps(self.to_dict())