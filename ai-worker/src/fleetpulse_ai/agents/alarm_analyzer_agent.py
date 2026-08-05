import time
from langchain_openai import AzureChatOpenAI
from langchain_core.prompts import ChatPromptTemplate
from fleetpulse_ai.prompts.templates import AlertTemplate
from fleetpulse_ai.agents.azure_credentials_manager import get_credential_manager
from fleetpulse_ai.models.agent_alert_response import AgentAlertResponse
from fleetpulse_ai.events.violation_event import ViolationEvent
from fleetpulse_ai.settings import settings
from fleetpulse_ai.logging_config import get_logger
logger = get_logger(__name__)
class AlarmAnalyzerAgent:
    def __init__(self, model_deployment: str):

        self.llm = None
        self.prompt = None
        self.model_deployment = model_deployment
        self._initialize_llm()

    def _initialize_llm(self):
        if self.llm is None:
            credential_manager = get_credential_manager()
            token_provider = credential_manager.get_openai_token_provider()

            self.llm = AzureChatOpenAI(
                azure_endpoint=settings.azure_openai_endpoint,
                api_version=settings.azure_openai_api_version,
                deployment_name=self.model_deployment,
                azure_ad_token_provider=token_provider,
                temperature=0
            ).with_structured_output(AgentAlertResponse)

        self.prompt = ChatPromptTemplate.from_messages([
            ("system", AlertTemplate.SYSTEM_PROMPT),
            ("human", "Query: {query}\n\nContext: {context}\n\n")
        ])

    async def analyze_alert(self, alert: ViolationEvent, context: dict) -> AgentAlertResponse:
        
        if self.llm is None or self.prompt is None:
            self._initialize_llm()

        query = f"Analyze the following alert: {alert}"
        logger.info("alert_analysis_started", driver_id=alert.driver_id)
        messages = self.prompt.format_messages(query=query, context=context)

        start_time = time.perf_counter()
        decision = await self.llm.ainvoke(messages)
        duration_ms = (time.perf_counter() - start_time) * 1000
        logger.info("alert_analysis_completed", driver_id=alert.driver_id, duration_ms=duration_ms)

        return AgentAlertResponse(
            risk_level=decision.risk_level,
            assessment=decision.assessment,
            recommended_action=decision.recommended_action,
            auto_escalate = decision.auto_escalate
        )

