from pydantic import ConfigDict
from pydantic_settings import BaseSettings
from typing import Optional

class Settings(BaseSettings):
    # API Configuration
    title: str = "FleetPulse AI Worker"
    description: str = "AI-Powered Worker for FleetPulse System"
    debug: bool = False
    azure_openai_endpoint: str = "https://<your-azure-openai-endpoint>.openai.azure.com/"
    azure_openai_api_version: str = "2024-08-01-preview"
    azure_openai_model_deployment:str = "<your-model-deployment-name>"

settings = Settings()