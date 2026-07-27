from azure.identity import DefaultAzureCredential, get_bearer_token_provider
from threading import RLock  # Changed from Lock

class AzureCredentialManager:
    """Singleton manager for Azure credentials and token providers."""
    _instance = None
    _lock = RLock()

    def __new__(cls):
        if cls._instance is None:
            with cls._lock:
                if cls._instance is None:
                    cls._instance = super().__new__(cls)
                    cls._instance._credential = None
                    cls._instance._openai_token_provider = None
        return cls._instance

    def get_credential(self) -> DefaultAzureCredential:
        """Get shared Azure credential (lazy initialization)."""
        if self._credential is None:
            with self._lock:
                if self._credential is None:
                    self._credential = DefaultAzureCredential()
        return self._credential


    def get_openai_token_provider(self):
        """Get token provider for Azure OpenAI."""
        if self._openai_token_provider is None:
            with self._lock:
                if self._openai_token_provider is None:
                    credential = self.get_credential()
                    self._openai_token_provider = get_bearer_token_provider(
                        credential,
                        "https://cognitiveservices.azure.com/.default"
                    )
        return self._openai_token_provider

# Global instance
_credential_manager = AzureCredentialManager()    

def get_credential_manager() -> AzureCredentialManager:
    """Get the shared credential manager instance."""
    return _credential_manager