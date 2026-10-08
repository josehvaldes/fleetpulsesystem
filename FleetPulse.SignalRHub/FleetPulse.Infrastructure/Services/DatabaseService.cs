using FleetPulse.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace FleetPulse.Infrastructure.Services
{
    public class DatabaseService(NpgsqlDataSource _dataSource, ILogger<DatabaseService> _logger) : IDatabaseService
    {
        public async Task<string> GetVersion(CancellationToken cancellationToken)
        {
            await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);
            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT version();";
            var version = await command.ExecuteScalarAsync(cancellationToken) as string;
            _logger.LogInformation("Database version: {Version}", version);
            return version ?? "Not Available";
        }

    }
}
