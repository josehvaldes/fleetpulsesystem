using Serilog.Events;
using Serilog.Formatting;
using Serilog.Formatting.Display;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace FleetPulse.DbWriter.Logging
{
    sealed class PythonCompatibleJsonFormatter(string defaultService, string defaultVersion) : ITextFormatter
    {
        private static readonly MessageTemplateTextFormatter MessageFormatter = new("{Message:l}", null);
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
        public void Format(LogEvent logEvent, TextWriter output)
        {
            var payload = new Dictionary<string, object?>
            {
                ["timestamp"] = logEvent.Timestamp.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fff'Z'"),
                ["level"] = MapLevel(logEvent.Level),
                ["logger"] = TryGetStringProperty(logEvent, "SourceContext"),
                ["service"] = TryGetStringProperty(logEvent, "service") ?? defaultService,
                ["version"] = TryGetStringProperty(logEvent, "version") ?? defaultVersion,
                ["message"] = RenderMessageLiteral(logEvent)
            };

            foreach (var property in logEvent.Properties)
            {
                if (property.Key is "SourceContext" or "service" or "version")
                {
                    continue;
                }

                payload[property.Key] = ConvertValue(property.Value);
            }

            output.WriteLine(JsonSerializer.Serialize(payload, JsonOptions));
        }

        private static string RenderMessageLiteral(LogEvent logEvent)
        {
            var writer = new StringWriter();
            MessageFormatter.Format(logEvent, writer);
            return writer.ToString().TrimEnd();
        }

        private static string MapLevel(LogEventLevel level) => level switch
        {
            LogEventLevel.Verbose => "trace",
            LogEventLevel.Debug => "debug",
            LogEventLevel.Information => "info",
            LogEventLevel.Warning => "warning",
            LogEventLevel.Error => "error",
            LogEventLevel.Fatal => "critical",
            _ => "info"
        };

        private static string? TryGetStringProperty(LogEvent logEvent, string propertyName)
        {
            if (logEvent.Properties.TryGetValue(propertyName, out var value) && value is ScalarValue scalar)
            {
                return scalar.Value?.ToString();
            }

            return null;
        }

        private static object? ConvertValue(LogEventPropertyValue value) => value switch
        {
            ScalarValue scalar => scalar.Value,
            SequenceValue sequence => sequence.Elements.Select(ConvertValue).ToList(),
            StructureValue structure => structure.Properties.ToDictionary(p => p.Name, p => ConvertValue(p.Value)),
            DictionaryValue dictionary => dictionary.Elements.ToDictionary(
                kvp => ConvertKey(kvp.Key),
                kvp => ConvertValue(kvp.Value)),
            _ => value.ToString()
        };

        private static string ConvertKey(ScalarValue key)
        {
            return key.Value?.ToString() ?? string.Empty;
        }
    }
}
