from pydantic import BaseModel, Field

class AgentAlertResponse(BaseModel):

    risk_level: str = Field(
        description="Risk level of the alert: low | medium | high | critical"
    )
    assessment: str = Field(
        description="1-2 sentences of reasoning combining pattern + context"
    )
    recommended_action: str = Field(
        description="Concrete instruction for the dispatcher."
    )
    auto_escalate: bool = Field(
        description="true only if critical"
    )

