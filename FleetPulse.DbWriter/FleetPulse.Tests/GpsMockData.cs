using FleetPulse.DbWriter.Models;
using System.Text.Json;
namespace FleetPulse.Tests
{
    public static class GpsMockData
    {


        public static List<GpsPing> GetMockGpsPings() 
        {
            var json = File.ReadAllText("./data/recoleta_route_1.json");
            return JsonSerializer.Deserialize<List<GpsPing>>(json) ?? new List<GpsPing>();
        }
    }
}
