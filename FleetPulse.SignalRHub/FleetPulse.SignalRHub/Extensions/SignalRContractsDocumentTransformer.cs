using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Schema;

namespace FleetPulse.SignalRHub.Extensions
{
    public sealed class SignalRContractsDocumentTransformer
    : IOpenApiDocumentTransformer
    {
        public async Task TransformAsync(
            OpenApiDocument document,
            OpenApiDocumentTransformerContext context,
            CancellationToken cancellationToken)
        {
            var contractTypes = typeof(Contracts.Marker).Assembly
                .GetTypes()
                .Where(t =>
                    t is { IsClass: true, IsAbstract: false }
                    &&
                    // include only types in the .Alerts and .GpsPing namespaces
                    (t.Namespace?.EndsWith(".Contracts.Response.Alerts") == true ||
                    t.Namespace?.EndsWith(".Contracts.Response.GpsPing") == true)
                    );

            foreach (var type in contractTypes)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var schema = await context.GetOrCreateSchemaAsync(
                    type,
                    parameterDescription: null,
                    cancellationToken);

                document.AddComponent(type.Name, schema);
            }
        }
    }
}
