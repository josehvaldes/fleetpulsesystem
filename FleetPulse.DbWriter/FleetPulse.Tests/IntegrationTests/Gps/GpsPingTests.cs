using FleetPulse.DbWriter.Configuration;
using FleetPulse.DbWriter.Services;
using FleetPulse.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using Xunit;

namespace FleetPulse.Tests.IntegrationTests.Gps
{
    [Collection("Integration")]
    public class GpsPingTests //: IClassFixture<IntegrationTestFixture>
    {
        private readonly IntegrationTestFixture _fixture;

        public GpsPingTests(IntegrationTestFixture fixture)
        {
            _fixture = fixture;
        }

        private GpsPingDatabaseService CreateDatabaseServiceInstance()
        {
            // Here you would typically set up your database connection string and any other required settings.
            var connectionString = _fixture.ConnectionString;
            var datasource = new NpgsqlDataSourceBuilder(connectionString).Build();
            ILogger<GpsPingDatabaseService> logger = new LoggerFactory().CreateLogger<GpsPingDatabaseService>();
            return new GpsPingDatabaseService(datasource, logger);
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
