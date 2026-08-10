from fleetpulse_ai.settings import settings
from opentelemetry import trace
from opentelemetry.sdk.resources import Resource
from opentelemetry.sdk.trace import TracerProvider
from opentelemetry.sdk.trace.export import BatchSpanProcessor, ConsoleSpanExporter
from opentelemetry.exporter.otlp.proto.grpc.trace_exporter import OTLPSpanExporter
from opentelemetry.sdk.trace.sampling import TraceIdRatioBased, ALWAYS_ON



def setup_tracing() -> None:
    resource = Resource.create({
        "service.name": settings.service_name,
        "service.version": settings.version,
        "deployment.environment": settings.otel_environment,
    })

    if settings.otel_environment == "development":
        sampler = ALWAYS_ON  # Sample all traces in development
    else:
        sampler = TraceIdRatioBased(1.0)  # Sample all traces

    provider = TracerProvider(resource=resource, sampler=sampler)  # Sample all traces

    if settings.console_exporter_enabled:
        provider.add_span_processor(BatchSpanProcessor(ConsoleSpanExporter()))

    if settings.otel_exporter_enabled:
        provider.add_span_processor(BatchSpanProcessor(
            OTLPSpanExporter(endpoint=settings.otel_exporter_endpoint, insecure=True)
        ))
    trace.set_tracer_provider(provider)
