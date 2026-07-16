using Confluent.Kafka;
using Dapper;
using FleetPulse.DbWriter.Models;
using Npgsql;
using Npgsql.Internal;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;

namespace FleetPulse.DbWriter.Services
{
    public class DatabaseService(NpgsqlDataSource _dataSource, ILogger<DatabaseService> _logger) : IDatabaseService
    {
        public async Task BulkInsertPingsAsync(List<GpsPing> pings, CancellationToken cancellationToken)
        {
            if (pings == null || pings.Count == 0)
                return;

            await using var conn = await _dataSource.OpenConnectionAsync(cancellationToken);
            await using var tx = await conn.BeginTransactionAsync(cancellationToken);

            const string copySql = """
            COPY gps_history
                (driver_id, timestamp, latitude, longitude, speed, heading, accuracy, raw_payload)
            FROM STDIN (FORMAT BINARY)
            """;


            await using (var writer = await conn.BeginBinaryImportAsync(copySql, cancellationToken))
            {
                foreach (var p in pings)
                {
                    await writer.StartRowAsync(cancellationToken);
                    writer.Write(p.DriverId, NpgsqlDbType.Varchar);
                    writer.Write(p.Timestamp,NpgsqlDbType.TimestampTz);
                    writer.Write(p.Latitude, NpgsqlDbType.Double);
                    writer.Write(p.Longitude, NpgsqlDbType.Double);
                    writer.Write(p.Speed, NpgsqlDbType.Double);
                    writer.Write(p.Heading, NpgsqlDbType.Integer);
                    writer.Write(p.Accuracy, NpgsqlDbType.Double);
                    writer.Write(p.RawPayloadJson ?? "{}", NpgsqlDbType.Jsonb);
                }

                await writer.CompleteAsync(cancellationToken);   // commits the COPY stream
            }

            await tx.CommitAsync(cancellationToken);
            _logger.LogInformation("Inserted {Count} pings into gps_history", pings.Count);
        }

        public async Task DeletePingsForDriverAsync(string driverId, CancellationToken cancellationToken)
        {
            await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);
            var deleteSql = "DELETE FROM gps_history WHERE driver_id = @DriverId";
            var response = await connection.ExecuteAsync(deleteSql, new { DriverId = driverId });
            _logger.LogInformation("Deleted {Count} pings for driver {DriverId}", response, driverId);
        }

        public async Task<DriverLastState?> GetDriverLastState(string driverId, CancellationToken cancellationToken)
        {
            await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);

            var selectSql = "SELECT * FROM driver_latest_state WHERE driver_id = @DriverId";
            
            var lastState = await connection.QueryFirstOrDefaultAsync<DriverLastState>(selectSql, new { DriverId = driverId });
            return lastState;
        }

        public async Task<List<GpsPing>> GetGpsPingsForDriverAsync(string driverId, CancellationToken cancellationToken)
        {
            await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);
            var selectSql = "SELECT * FROM gps_history WHERE driver_id = @DriverId";
            var pings = await connection.QueryAsync<GpsPing>(selectSql, new { DriverId = driverId });
            return pings.ToList();
        }

        public async Task<string> GetVersion(CancellationToken cancellationToken)
        {
            await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);

            var version = await connection.ExecuteScalarAsync<string>("SELECT version();");
            return version ?? "Not Available";
        }

        public async Task UpsertLatestStateAsync(IReadOnlyList<GpsPing> pings,CancellationToken ct = default)
        {
            if (pings.Count == 0) return;

            // Within a single flush window, keep only the newest ping per driver.
            var latestPerDriver = pings
                .GroupBy(p => p.DriverId)
                .Select(g => g.MaxBy(p => p.Timestamp)!).Select(p => new DriverLastState
                {
                    Driver_Id = p.DriverId,
                    Latitude = p.Latitude,
                    Longitude = p.Longitude,
                    Speed = p.Speed,
                    Heading = p.Heading,
                    Last_Seen = p.Timestamp,
                    Status = p.Status
                })
                .ToList();
            _logger.LogInformation("Upserting latest state for {Count} drivers", latestPerDriver.Count);
            const string sql = """
                        INSERT INTO driver_latest_state
                            (driver_id, latitude, longitude, speed, heading, last_seen, status)
                        VALUES
                            (@Driver_Id, @Latitude, @Longitude, @Speed, @Heading, @Last_Seen, @Status)
                        ON CONFLICT (driver_id) DO UPDATE SET
                            latitude  = EXCLUDED.latitude,
                            longitude = EXCLUDED.longitude,
                            speed     = EXCLUDED.speed,
                            heading   = EXCLUDED.heading,
                            last_seen = EXCLUDED.last_seen,
                            status    = EXCLUDED.status
                        """;

            await using var conn = await _dataSource.OpenConnectionAsync(ct);
            await conn.ExecuteAsync(new CommandDefinition(sql, latestPerDriver, cancellationToken: ct));
        }
    }
}
