using FleetPulse.DbWriter.Models;
using System.Text.Json;
namespace FleetPulse.Tests
{
    public static class GpsMockData
    {
        public static List<GpsPingDto> GetMockGpsPings() 
        {
            var json = File.ReadAllText("./data/recoleta_route_sample_output.json");
            return JsonSerializer.Deserialize<List<GpsPingDto>>(json) ?? new List<GpsPingDto>();
        }
    }
}
