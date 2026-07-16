using FleetPulse.DbWriter.Configuration;
using FleetPulse.DbWriter.Models;
using FleetPulse.DbWriter.Services;
using FluentAssertions;
using Microsoft.Extensions.Options;
using System.Text;
using Xunit;

namespace FleetPulse.Tests.UnitTests
{
    public class CompressorTests
    {
        private static CompressionService CreateCompressionInstance(KafkaSettings? settings = null) 
        {
            return new CompressionService(Options.Create(settings ?? new KafkaSettings()));
        }

        private static List<GpsPing> GetHighSpeedPings()
        {
            var list = GpsMockData.GetMockGpsPings();
            list = list.GetRange(0, 10);
            list.ElementAt(0).Speed = 32.0;
            list.ElementAt(1).Speed = 32.0;

            list.ElementAt(2).Speed = 42.0;
            list.ElementAt(3).Speed = 43.0;
            list.ElementAt(4).Speed = 44.0;
            list.ElementAt(5).Speed = 43.0;
            list.ElementAt(6).Speed = 43.0;

            list.ElementAt(7).Speed = 33.0;
            list.ElementAt(8).Speed = 33.0;
            list.ElementAt(9).Speed = 33.0;

            return list;
        }

        [Fact]
        public async Task ApplyCompressionToRecoletaData_ShouldCompressPings() 
        {

            var data = GpsMockData.GetMockGpsPings();

            var compressionService = CreateCompressionInstance();
            var compressData = await compressionService.ApplyTemporalCompression(data);

            var stoppedPings = compressData.Where(p => p.Status == "stopped" || p.Speed == 0);
            stoppedPings.Count().Should().Be(6);
            compressData.Count.Should().Be(24);
        }


        [Fact]
        public async Task ApplyHighSpeedCompression_ShouldCompressPings() 
        {
            var highSpeedPings = GetHighSpeedPings();

            var compressionService = CreateCompressionInstance();

            var compressedPings = await compressionService.ApplyHighSpeedCompression(highSpeedPings);
            compressedPings.Count.Should().Be(6);
        }


        [Fact]
        public async Task DropIntermediateStoppedPings_2_items_ShouldCompressStoppedPings()
        {
            // Arrange
            var pings = new List<GpsPing>
            {
                new GpsPing { Id = 1, Status = "moving" , Speed=50 },
                new GpsPing { Id = 2, Status = "stopped", Speed=0 },
            };
            var compressionService = CreateCompressionInstance();
            // Act
            var compressedPings = await compressionService.DropIntermediateStoppedPings(pings);
            // Assert
            compressedPings.Count.Should().Be(2);
            compressedPings[0].Status.Should().Be("moving");
            compressedPings[1].Status.Should().Be("stopped");
        }

        [Fact]
        public async Task DropIntermediateStoppedPings_3_items_ShouldCompressStoppedPings()
        {
            // Arrange
            var pings = new List<GpsPing>
            {
                new GpsPing { Id = 1, Status = "moving" , Speed=50 },
                new GpsPing { Id = 2, Status = "stopped", Speed=0 },
                new GpsPing { Id = 3, Status = "stopped", Speed=0 },
            };
            var compressionService = CreateCompressionInstance();
            // Act
            var compressedPings = await compressionService.DropIntermediateStoppedPings(pings);
            // Assert
            compressedPings.Count.Should().Be(3);
            compressedPings[0].Status.Should().Be("moving");
            compressedPings[1].Status.Should().Be("stopped");
            compressedPings[2].Status.Should().Be("stopped");
        }

        [Fact]
        public async Task DropIntermediateStoppedPings_ShouldCompressStoppedPings()
        {
            // Arrange
            var pings = new List<GpsPing>
            {
                new GpsPing { Id = 1, Status = "moving" , Speed=50 },
                new GpsPing { Id = 2, Status = "stopped", Speed=0 },
                new GpsPing { Id = 3, Status = "stopped", Speed=0 },
                new GpsPing { Id = 4, Status = "stopped", Speed=0 },
                new GpsPing { Id = 5, Status = "stopped", Speed=0 },
                new GpsPing { Id = 6, Status = "moving", Speed=50 },
                new GpsPing { Id = 7, Status = "stopped", Speed=0 },
                new GpsPing { Id = 8, Status = "stopped", Speed=0 },
                new GpsPing { Id = 9, Status = "moving", Speed=50 }
            };
            var compressionService = CreateCompressionInstance();
            // Act
            var compressedPings = await compressionService.DropIntermediateStoppedPings(pings);
            // Assert
            compressedPings.Count.Should().Be(7);
            compressedPings[0].Status.Should().Be("moving");
            compressedPings[1].Status.Should().Be("stopped");
            compressedPings[2].Status.Should().Be("stopped");
            compressedPings[3].Status.Should().Be("moving");
            compressedPings[4].Status.Should().Be("stopped");
            compressedPings[5].Status.Should().Be("stopped");
            compressedPings[6].Status.Should().Be("moving");
        }

        [Fact]
        public async Task DropIntermediateStoppedPingsLinq_ShouldCompressStoppedPings()
        {
            // Arrange
            var pings = new List<GpsPing>
            {
                new GpsPing { Id = 1, Status = "moving" , Speed=50 },
                new GpsPing { Id = 2, Status = "stopped", Speed=0 },
                new GpsPing { Id = 3, Status = "stopped", Speed=0 },
                new GpsPing { Id = 4, Status = "stopped", Speed=0 },
                new GpsPing { Id = 5, Status = "stopped", Speed=0 },
                new GpsPing { Id = 6, Status = "moving", Speed=50 },
                new GpsPing { Id = 7, Status = "stopped", Speed=0 },
                new GpsPing { Id = 8, Status = "stopped", Speed=0 },
                new GpsPing { Id = 9, Status = "moving", Speed=50 }
            };
            var compressionService = CreateCompressionInstance();
            // Act
            var compressedPings = await compressionService.DropIntermediateStoppedPingsLinq(pings);
            // Assert
            compressedPings.Count.Should().Be(7);
            compressedPings[0].Status.Should().Be("moving");
            compressedPings[1].Status.Should().Be("stopped");
            compressedPings[2].Status.Should().Be("stopped");
            compressedPings[3].Status.Should().Be("moving");
            compressedPings[4].Status.Should().Be("stopped");
            compressedPings[5].Status.Should().Be("stopped");
            compressedPings[6].Status.Should().Be("moving");
        }

    }
}
