using Dapper;
using FleetPulse.Drivers.Application.Common.Interfaces;
using FleetPulse.Drivers.Domain.Entities;
using Npgsql;

namespace FleetPulse.Drivers.Infrastructure.Repositories
{
    public class DriversRepository(NpgsqlDataSource dataSource) : IDriversRepository
    {
        public async Task<IEnumerable<GpsPing>> GetGPSHistory(string driverId, DateTime startTime, DateTime endTime, CancellationToken cancellationToken)
        {
            await using var conn = await dataSource.OpenConnectionAsync(cancellationToken);
            var sql = "SELECT driver_id, latitude, longitude, speed, heading, timestamp " +
                "FROM fleetpulse.gps_history WHERE driver_id = @DriverId AND timestamp BETWEEN @StartTime AND @EndTime";

            return await conn.QueryAsync<GpsPing>(sql, new { DriverId = driverId, StartTime = startTime, EndTime = endTime });
        }

        public async Task<IEnumerable<LatestDriverState>> GetLatestDriverStatesAsync(DateTime after, CancellationToken cancellationToken)
        {
            await using var conn = await dataSource.OpenConnectionAsync(cancellationToken);
            var sql = "SELECT driver_id, latitude, longitude, speed, heading, last_seen, status " +
                "FROM drivers.driver_latest_state where last_seen > @After";

            return await conn.QueryAsync<LatestDriverState>(sql, new { After = after });
        }
    }
}
