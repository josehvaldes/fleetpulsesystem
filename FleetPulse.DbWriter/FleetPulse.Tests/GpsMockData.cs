using FleetPulse.DbWriter.Models;
using System.Text.Json;
namespace FleetPulse.Tests
{
    public static class GpsMockData
    {
        public static List<GpsPingDto> GetMockGpsPings() 
        {
            var scriptPath = Path.Combine(AppContext.BaseDirectory, "db", "recoleta_route_sample_output.json");
            if (!File.Exists(scriptPath)) 
            {
                throw new FileNotFoundException($"The mock GPS data file was not found at path: {scriptPath}");
            }

            var json = File.ReadAllText(scriptPath);
            return JsonSerializer.Deserialize<List<GpsPingDto>>(json) ?? new List<GpsPingDto>();
        }
    }
}
