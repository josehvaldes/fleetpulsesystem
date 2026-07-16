using FleetPulse.DbWriter.Configuration;
using FleetPulse.DbWriter.Models;
using FleetPulse.DbWriter.Services;
using Microsoft.Extensions.Options;
using Xunit;

namespace FleetPulse.Tests
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
            list.ElementAt(0).SpeedKmh = 32.0;
            list.ElementAt(1).SpeedKmh = 32.0;

            list.ElementAt(2).SpeedKmh = 42.0;
            list.ElementAt(3).SpeedKmh = 43.0;
            list.ElementAt(4).SpeedKmh = 44.0;
            list.ElementAt(5).SpeedKmh = 43.0;
            list.ElementAt(6).SpeedKmh = 43.0;

            list.ElementAt(7).SpeedKmh = 33.0;
            list.ElementAt(8).SpeedKmh = 33.0;
            list.ElementAt(9).SpeedKmh = 33.0;

            return list;
        }

        [Fact]
        public async Task ApplyHighSpeedCompression_ShouldCompressPings() 
        {
            var highSpeedPings = GetHighSpeedPings();

            var compressionService = CreateCompressionInstance();

            var compressedPings = await compressionService.ApplyHighSpeedCompression(highSpeedPings);
            Assert.Equal(6, compressedPings.Count);
        }


        [Fact]
        public async Task DropIntermediateStoppedPings_2_items_ShouldCompressStoppedPings()
        {
            // Arrange
            var pings = new List<GpsPing>
            {
                new GpsPing { Id = 1, Status = "moving" , SpeedKmh=50 },
                new GpsPing { Id = 2, Status = "stopped", SpeedKmh=0 },
            };
            var compressionService = CreateCompressionInstance();
            // Act
            var compressedPings = await compressionService.DropIntermediateStoppedPings(pings);
            // Assert
            Assert.Equal(2, compressedPings.Count);
            Assert.Equal("moving", compressedPings[0].Status);
            Assert.Equal("stopped", compressedPings[1].Status);
        }

        [Fact]
        public async Task DropIntermediateStoppedPings_3_items_ShouldCompressStoppedPings()
        {
            // Arrange
            var pings = new List<GpsPing>
            {
                new GpsPing { Id = 1, Status = "moving" , SpeedKmh=50 },
                new GpsPing { Id = 2, Status = "stopped", SpeedKmh=0 },
                new GpsPing { Id = 3, Status = "stopped", SpeedKmh=0 },
            };
            var compressionService = CreateCompressionInstance();
            // Act
            var compressedPings = await compressionService.DropIntermediateStoppedPings(pings);
            // Assert
            Assert.Equal(3, compressedPings.Count);
            Assert.Equal("moving", compressedPings[0].Status);
            Assert.Equal("stopped", compressedPings[1].Status);
            Assert.Equal("stopped", compressedPings[2].Status);
        }

        [Fact]
        public async Task DropIntermediateStoppedPings_ShouldCompressStoppedPings()
        {
            // Arrange
            var pings = new List<GpsPing>
            {
                new GpsPing { Id = 1, Status = "moving" , SpeedKmh=50 },
                new GpsPing { Id = 2, Status = "stopped", SpeedKmh=0 },
                new GpsPing { Id = 3, Status = "stopped", SpeedKmh=0 },
                new GpsPing { Id = 4, Status = "stopped", SpeedKmh=0 },
                new GpsPing { Id = 5, Status = "stopped", SpeedKmh=0 },
                new GpsPing { Id = 6, Status = "moving", SpeedKmh=50 },
                new GpsPing { Id = 7, Status = "stopped", SpeedKmh=0 },
                new GpsPing { Id = 8, Status = "stopped", SpeedKmh=0 },
                new GpsPing { Id = 9, Status = "moving", SpeedKmh=50 }
            };
            var compressionService = CreateCompressionInstance();
            // Act
            var compressedPings = await compressionService.DropIntermediateStoppedPings(pings);
            // Assert
            Assert.Equal(7, compressedPings.Count);
            Assert.Equal("moving", compressedPings[0].Status);
            Assert.Equal("stopped", compressedPings[1].Status);
            Assert.Equal("stopped", compressedPings[2].Status);
            Assert.Equal("moving", compressedPings[3].Status);
            Assert.Equal("stopped", compressedPings[4].Status);
            Assert.Equal("stopped", compressedPings[5].Status);
            Assert.Equal("moving", compressedPings[6].Status);
        }

        [Fact]
        public async Task DropIntermediateStoppedPingsLinq_ShouldCompressStoppedPings()
        {
            // Arrange
            var pings = new List<GpsPing>
            {
                new GpsPing { Id = 1, Status = "moving" , SpeedKmh=50 },
                new GpsPing { Id = 2, Status = "stopped", SpeedKmh=0 },
                new GpsPing { Id = 3, Status = "stopped", SpeedKmh=0 },
                new GpsPing { Id = 4, Status = "stopped", SpeedKmh=0 },
                new GpsPing { Id = 5, Status = "stopped", SpeedKmh=0 },
                new GpsPing { Id = 6, Status = "moving", SpeedKmh=50 },
                new GpsPing { Id = 7, Status = "stopped", SpeedKmh=0 },
                new GpsPing { Id = 8, Status = "stopped", SpeedKmh=0 },
                new GpsPing { Id = 9, Status = "moving", SpeedKmh=50 }
            };
            var compressionService = CreateCompressionInstance();
            // Act
            var compressedPings = await compressionService.DropIntermediateStoppedPingsLinq(pings);
            // Assert
            Assert.Equal(7, compressedPings.Count);
            Assert.Equal("moving", compressedPings[0].Status);
            Assert.Equal("stopped", compressedPings[1].Status);
            Assert.Equal("stopped", compressedPings[2].Status);
            Assert.Equal("moving", compressedPings[3].Status);
            Assert.Equal("stopped", compressedPings[4].Status);
            Assert.Equal("stopped", compressedPings[5].Status);
            Assert.Equal("moving", compressedPings[6].Status);
        }

    }
}
