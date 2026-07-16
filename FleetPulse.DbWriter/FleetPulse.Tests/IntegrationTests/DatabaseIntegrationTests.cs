using FleetPulse.DbWriter.Configuration;
using FleetPulse.DbWriter.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace FleetPulse.Tests.IntegrationTests
{
    public class DatabaseIntegrationTests
    {

        public DatabaseIntegrationTests() { }

        private static DatabaseService CreateDatabaseServiceInstance()
        {
            // Here you would typically set up your database connection string and any other required settings.
            var connectionString = "Host=localhost;Port=5432;Database=fleetpulse;Username=fleetpulse;Password=fleetpulse_dev";
            var datasource = new NpgsqlDataSourceBuilder(connectionString).Build();
            ILogger<DatabaseService> logger = new LoggerFactory().CreateLogger<DatabaseService>();
            return new DatabaseService(datasource, logger);
        }

        private static CompressionService CreateCompressorServiceInstance(KafkaSettings? settings = null)
        {
            ILogger<CompressionService> logger = new LoggerFactory().CreateLogger<CompressionService>();
            return new CompressionService(Options.Create(settings ?? new KafkaSettings()));
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
        public async Task BulkInsertPingsAsync_SuccessfullInsert() 
        {
            var pings = GpsMockData.GetMockGpsPings();
            var compressionService = CreateCompressorServiceInstance();

            var compressedData = await compressionService.ApplyTemporalCompression(pings);

            var databaseService = CreateDatabaseServiceInstance();

            var driverId = pings.First().DriverId;
            await databaseService.DeletePingsForDriverAsync(driverId, CancellationToken.None);

            try
            {
                await databaseService.BulkInsertPingsAsync(compressedData, CancellationToken.None);
            }
            catch (Exception ex) 
            {
                Assert.Fail(ex.Message);
            }

            var retrievedData = await databaseService.GetGpsPingsForDriverAsync(driverId, CancellationToken.None);
            retrievedData.Count().Should().Be(compressedData.Count);
        }

        [Fact]
        public async Task UpsertLatestAsync_SuccessfulUpsert()
        {
            var pings = GpsMockData.GetMockGpsPings();
            var databaseService = CreateDatabaseServiceInstance();
            var driverId = pings.First().DriverId;
            
            try
            {
                await databaseService.UpsertLatestStateAsync(pings, CancellationToken.None);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
            var retrievedData = await databaseService.GetDriverLastState(driverId, CancellationToken.None);
            retrievedData.Should().NotBeNull();
            retrievedData.Driver_Id.Should().Be(driverId);

        }
    }
}
