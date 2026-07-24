
from abc import ABC, abstractmethod

from fleetpulse_ai.events.violation_event import ViolationEvent
from fleetpulse_ai.models.gps_ping import GpsPing


class BaseDetector(ABC):
    """Abstract base class for all detectors."""

    @abstractmethod
    async def analyze(self, driver_id: str, history: list[GpsPing]) -> ViolationEvent | None:
        """Analyze the driver's GPS history and return a ViolationEvent if a violation is detected."""