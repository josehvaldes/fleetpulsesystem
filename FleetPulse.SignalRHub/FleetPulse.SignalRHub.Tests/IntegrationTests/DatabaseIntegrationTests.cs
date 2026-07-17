using FleetPulse.SignalRHub.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace FleetPulse.SignalRHub.Tests.IntegrationTests
{
    public class DatabaseIntegrationTests
    {
        private static DatabaseService CreateDatabaseServiceInstance()
        {
            // Here you would typically set up your database connection string and any other required settings.
            var connectionString = "Host=localhost;Port=5432;Database=fleetpulse;Username=fleetpulse;Password=fleetpulse_dev";
            var datasource = new NpgsqlDataSourceBuilder(connectionString).Build();
            ILogger<DatabaseService> logger = new LoggerFactory().CreateLogger<DatabaseService>();
            return new DatabaseService(datasource, logger);
        }


        [Fact]
        public async Task GetVersionDBConnection_NotNullOrEmpty()
        {
            // Arrange
            var databaseService = CreateDatabaseServiceInstance();
            // Act
            var version = await databaseService.GetVersion(CancellationToken.None);
            // Assert
            Assert.False(string.IsNullOrEmpty(version), "Database version should not be null or empty.");
        }

        [Fact]
        public async Task GetLatestDriverStatesAsync_ReturnsNonEmptyList()
        {
            // Arrange
            var databaseService = CreateDatabaseServiceInstance();
            var after = DateTime.UtcNow.AddHours(-1);

            // Act
            var latestDriverStates = await databaseService.GetLatestDriverStatesAsync(after, CancellationToken.None);

            // Assert
            latestDriverStates.Should().BeEmpty();
        }
    }
}
