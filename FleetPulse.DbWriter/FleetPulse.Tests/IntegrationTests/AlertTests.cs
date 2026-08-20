
using Dapper;
using FleetPulse.DbWriter.Infrastructure;
using FleetPulse.DbWriter.Mappings;
using FleetPulse.DbWriter.Models;
using FleetPulse.DbWriter.Models.DB;
using FleetPulse.DbWriter.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Npgsql;
using Xunit;

namespace FleetPulse.Tests.IntegrationTests
{
    public class AlertTests
    {

        public AlertTests() 
        {
            // Register SQL mappings for Dapper
            SqlMapping.RegisterSqlMappings();
        }

        private static AlertDatabaseService CreateDatabaseServiceInstance()
        {
            // Here you would typically set up your database connection string and any other required settings.
            var connectionString = "Host=localhost;Port=5432;Database=fleetpulse;Username=fleetpulse;Password=fleetpulse_dev";
            var datasource = new NpgsqlDataSourceBuilder(connectionString).Build();
            ILogger<AlertDatabaseService> logger = new LoggerFactory().CreateLogger<AlertDatabaseService>();
            return new AlertDatabaseService(datasource, logger);
        }

        private static AlertDb CreateAlert(string driverId, string riskLevel) 
        {

            var alert = new AlertDb
            {
                id = Guid.NewGuid(),
                driver_id = driverId,
                event_latitude = 40.7128,
                event_longitude = -74.0060,
                exit_speed = 60.0,
                exit_time = DateTime.UtcNow,
                zone_name = $"Test Zone: {driverId}",
                zone_type = $"Test Type: {driverId}",
                risk_level = riskLevel,
                assessment = $"Test Assessment: {driverId}",
                recommendation = $"Test Recommendation: {driverId}",
                autoscale = false,
                status = AlertStatus.New,
                raised_at = DateTime.UtcNow
            };

            return alert;
        }

        [Fact]
        public async Task CreateAlert_SuccessfulInsert()
        {
            // Arrange
            var databaseService = CreateDatabaseServiceInstance();
            
            var alert = CreateAlert("driver123", "High");
            try 
            {
                var alertId = await databaseService.AddAlert(alert);
                alertId.Should().NotBe(Guid.Empty);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.ToString());
            }
        }

        [Fact]
        public async Task GetAlert_StatusEnum_InProgress() 
        {
            // Arrange
            var databaseService = CreateDatabaseServiceInstance();

            //random number
            var driverId = $"driver_{Random.Shared.Next(100, 999)}";
            var alert = CreateAlert(driverId, "High");
            alert.status = AlertStatus.InProgress;
            try
            {
                var alertId = await databaseService.AddAlert(alert);
                alertId.Should().NotBe(Guid.Empty);

                var alerts = await databaseService.GetAlertsByDriverId(driverId);
                alerts.Should().NotBeEmpty();
                var recovered = alerts.Where(a => a.id == alertId && a.status == AlertStatus.InProgress).FirstOrDefault();
                recovered.Should().NotBeNull();
                recovered.status.Should().Be(AlertStatus.InProgress);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.ToString());
            }
        }

        [Fact]
        public async Task GetAlertsByDriverId_ReturnsAlerts()
        {
            // Arrange
            var databaseService = CreateDatabaseServiceInstance();
            var driverId = "driver123";
            var alert = CreateAlert(driverId, "Medium");
            await databaseService.AddAlert(alert);
            // Act
            var alerts = await databaseService.GetAlertsByDriverId(driverId);
            // Assert
            alerts.Should().NotBeNull();
            alerts.Should().Contain(a => a.id == alert.id);
        }

        [Fact]
        public async Task GetAlertsByDateRange_ReturnsAlerts()
        {
            // Arrange
            var databaseService = CreateDatabaseServiceInstance();
            var alert = CreateAlert("driver456", "Low");
            await databaseService.AddAlert(alert);
            var startDate = DateTime.UtcNow.AddMinutes(-5);
            var endDate = DateTime.UtcNow.AddMinutes(5);
            // Act
            var alerts = await databaseService.GetAlertsByDateRange(startDate, endDate);
            // Assert
            alerts.Should().NotBeNull();
            alerts.Should().Contain(a => a.id == alert.id);
        }

        [Fact]
        public async Task GetAlertsByDateRange_NoAlerts_ReturnsEmpty()
        {
            // Arrange
            var databaseService = CreateDatabaseServiceInstance();
            var startDate = DateTime.UtcNow.AddDays(-10);
            var endDate = DateTime.UtcNow.AddDays(-5);
            // Act
            var alerts = await databaseService.GetAlertsByDateRange(startDate, endDate);
            // Assert
            alerts.Should().NotBeNull();
            alerts.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAlertsByStatusDateRange_ReturnsAlerts()
        {
            // Arrange
            var databaseService = CreateDatabaseServiceInstance();
            var alert = CreateAlert("driver789", "Critical");
            await databaseService.AddAlert(alert);
            var startDate = DateTime.UtcNow.AddMinutes(-5);
            var endDate = DateTime.UtcNow.AddMinutes(5);
            // Act
            var alerts = await databaseService.GetAlertsByStatusDateRange(AlertStatus.New, startDate, endDate);
            // Assert
            alerts.Should().NotBeNull();
            alerts.Should().Contain(a => a.id == alert.id);
        }
    }
}
