from functools import lru_cache
from dotenv import find_dotenv
from pydantic import ConfigDict
from pydantic_settings import BaseSettings
from typing import Optional

ENV_FILE = find_dotenv(usecwd=True) or ".env"

class Settings(BaseSettings):
    # API Configuration
    title: str = "FleetPulse AI Worker"
    description: str = "AI-Powered Worker for FleetPulse System"
    debug: bool = False


    #Logging Configuration
    log_level: str = "INFO"
    log_file: Optional[str] = None # Path to log file. If None, only console logging is used
    log_to_console: bool = True

    # Additional Logging outputs
    service_name: str = "ai.worker"  # Optional service name to bind to all logs (e.g., 'ai-worker')
    version: str = "1.0.0"  # Application version for logging and monitoring

    # Azure OpenAI Configuration
    
    azure_openai_endpoint: str = "https://oai-petshop-test.openai.azure.com/"
    azure_openai_api_version: str = "2024-08-01-preview"
    azure_openai_model_deployment:str = "gpt-4.1-mini_shopassist"

    kafka_bootstrap_servers: str = "localhost:19092"
    kafka_group_id: str = "ai-worker-group"
    kafka_topics: str = "gps-pings"
    kafka_alert_topic: str = "alerts"
    prometheus_metrics_port: int = 8000

    # OpenTelemetry Configuration
    otel_exporter_endpoint: str = "http://otel-collector:4317"  # OTLP gRPC endpoint for OpenTelemetry Collector
    console_exporter_enabled: bool = False  # Enable console logging for OpenTelemetry
    otel_exporter_enabled: bool = True  # Enable OTLP exporter for OpenTelemetry
    otel_environment: str = "development"  # Environment name for OpenTelemetry resource attributes (e.g., 'development', 'production')

    model_config = ConfigDict(
        str_max_length=200,
        env_file=ENV_FILE,
        env_file_encoding="utf-8",
        case_sensitive=False,
        extra="ignore"
        )

@lru_cache()
def get_settings() -> Settings:
    """
    Get cached application settings.
    
    Returns:
        Settings instance
    """
    return Settings()


settings = get_settings()