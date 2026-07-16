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


            // ── StoppedConditional ────────────────────────────────────────────────

            [Fact]
            public void StoppedConditional_ReturnsTrue_ForStoppedStatus()
            {
                var service = CreateCompressionInstance();
                service.StoppedConditional(new GpsPing { Status = "stopped", Speed = 0 }).Should().BeTrue();
            }

            [Fact]
            public void StoppedConditional_ReturnsTrue_ForIdleStatus()
            {
                var service = CreateCompressionInstance();
                service.StoppedConditional(new GpsPing { Status = "idle", Speed = 5 }).Should().BeTrue();
            }

            [Fact]
            public void StoppedConditional_ReturnsTrue_ForParkedStatus()
            {
                var service = CreateCompressionInstance();
                service.StoppedConditional(new GpsPing { Status = "parked", Speed = 5 }).Should().BeTrue();
            }

            [Fact]
            public void StoppedConditional_ReturnsTrue_WhenSpeedBelowOne()
            {
                var service = CreateCompressionInstance();
                service.StoppedConditional(new GpsPing { Status = "moving", Speed = 0.5 }).Should().BeTrue();
            }

            [Fact]
            public void StoppedConditional_ReturnsFalse_ForMovingPingWithHighSpeed()
            {
                var service = CreateCompressionInstance();
                service.StoppedConditional(new GpsPing { Status = "moving", Speed = 50 }).Should().BeFalse();
            }

            // ── DropIntermediateStoppedPings ──────────────────────────────────────

            [Fact]
            public async Task DropIntermediateStoppedPings_EmptyList_ReturnsEmpty()
            {
                var service = CreateCompressionInstance();
                var result = await service.DropIntermediateStoppedPings(new List<GpsPing>());
                result.Should().BeEmpty();
            }

            [Fact]
            public async Task DropIntermediateStoppedPings_AllMoving_ReturnsAllPings()
            {
                var pings = new List<GpsPing>
                {
                    new GpsPing { Id = 1, Status = "moving", Speed = 50 },
                    new GpsPing { Id = 2, Status = "moving", Speed = 60 },
                    new GpsPing { Id = 3, Status = "moving", Speed = 55 },
                };
                var service = CreateCompressionInstance();
                var result = await service.DropIntermediateStoppedPings(pings);
                result.Count.Should().Be(3);
            }

            [Fact]
            public async Task DropIntermediateStoppedPings_AllStopped_KeepsFirstAndLast()
            {
                var pings = new List<GpsPing>
                {
                    new GpsPing { Id = 1, Status = "stopped", Speed = 0 },
                    new GpsPing { Id = 2, Status = "stopped", Speed = 0 },
                    new GpsPing { Id = 3, Status = "stopped", Speed = 0 },
                    new GpsPing { Id = 4, Status = "stopped", Speed = 0 },
                };
                var service = CreateCompressionInstance();
                var result = await service.DropIntermediateStoppedPings(pings);
                result.Count.Should().Be(2);
                result[0].Id.Should().Be(1);
                result[1].Id.Should().Be(4);
            }

            [Fact]
            public async Task DropIntermediateStoppedPings_TrailingStoppedRun_KeepsFirstAndLastStopped()
            {
                var pings = new List<GpsPing>
                {
                    new GpsPing { Id = 1, Status = "moving",  Speed = 50 },
                    new GpsPing { Id = 2, Status = "stopped", Speed = 0  },
                    new GpsPing { Id = 3, Status = "stopped", Speed = 0  },
                    new GpsPing { Id = 4, Status = "stopped", Speed = 0  },
                };
                var service = CreateCompressionInstance();
                var result = await service.DropIntermediateStoppedPings(pings);
                result.Count.Should().Be(3);
                result[0].Id.Should().Be(1);
                result[1].Id.Should().Be(2);
                result[2].Id.Should().Be(4);
            }

            // ── DropIntermediateStoppedPingsLinq ──────────────────────────────────

            [Fact]
            public async Task DropIntermediateStoppedPingsLinq_EmptyList_ReturnsEmpty()
            {
                var service = CreateCompressionInstance();
                var result = await service.DropIntermediateStoppedPingsLinq(new List<GpsPing>());
                result.Should().BeEmpty();
            }

            [Fact]
            public async Task DropIntermediateStoppedPingsLinq_AllMoving_ReturnsAllPings()
            {
                var pings = new List<GpsPing>
                {
                    new GpsPing { Id = 1, Status = "moving", Speed = 50 },
                    new GpsPing { Id = 2, Status = "moving", Speed = 60 },
                    new GpsPing { Id = 3, Status = "moving", Speed = 55 },
                };
                var service = CreateCompressionInstance();
                var result = await service.DropIntermediateStoppedPingsLinq(pings);
                result.Count.Should().Be(3);
            }

            // ── ApplyHighSpeedCompression ─────────────────────────────────────────

            [Fact]
            public async Task ApplyHighSpeedCompression_EmptyList_ReturnsEmpty()
            {
                var service = CreateCompressionInstance();
                var result = await service.ApplyHighSpeedCompression(new List<GpsPing>());
                result.Should().BeEmpty();
            }

            [Fact]
            public async Task ApplyHighSpeedCompression_AllNormalSpeed_ReturnsAllPings()
            {
                var pings = new List<GpsPing>
                {
                    new GpsPing { Id = 1, Speed = 20 },
                    new GpsPing { Id = 2, Speed = 30 },
                    new GpsPing { Id = 3, Speed = 40 }, // equal to threshold — kept
                };
                var service = CreateCompressionInstance();
                var result = await service.ApplyHighSpeedCompression(pings);
                result.Count.Should().Be(3);
            }

            [Fact]
            public async Task ApplyHighSpeedCompression_AlwaysKeepsFirstAndLastPings()
            {
                // All pings are high-speed and fall in the same 15-second bucket,
                // so only first + one bucket representative + last should survive.
                var baseTime = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero);
                var pings = new List<GpsPing>
                {
                    new GpsPing { Id = 1, Speed = 80, Timestamp = baseTime },
                    new GpsPing { Id = 2, Speed = 80, Timestamp = baseTime.AddSeconds(1) },
                    new GpsPing { Id = 3, Speed = 80, Timestamp = baseTime.AddSeconds(2) },
                    new GpsPing { Id = 4, Speed = 80, Timestamp = baseTime.AddSeconds(3) },
                    new GpsPing { Id = 5, Speed = 80, Timestamp = baseTime.AddSeconds(4) },
                };
                var service = CreateCompressionInstance();
                var result = await service.ApplyHighSpeedCompression(pings);
                result.First().Id.Should().Be(1);
                result.Last().Id.Should().Be(5);
            }

            [Fact]
            public async Task ApplyHighSpeedCompression_RespectsCustomThreshold()
            {
                var settings = new KafkaSettings { HighSpeedCompressionThreshold = 60, HighSpeedCompressionBucketKey = 15 };
                var baseTime = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero);
                // Speed 50 is below the custom threshold of 60, so all pings should be kept.
                var pings = new List<GpsPing>
                {
                    new GpsPing { Id = 1, Speed = 50, Timestamp = baseTime },
                    new GpsPing { Id = 2, Speed = 50, Timestamp = baseTime.AddSeconds(1) },
                    new GpsPing { Id = 3, Speed = 50, Timestamp = baseTime.AddSeconds(2) },
                };
                var service = CreateCompressionInstance(settings);
                var result = await service.ApplyHighSpeedCompression(pings);
                result.Count.Should().Be(3);
            }

            // ── ApplyTemporalCompression ──────────────────────────────────────────

            [Fact]
            public async Task ApplyTemporalCompression_EmptyList_ReturnsEmpty()
            {
                var service = CreateCompressionInstance();
                var result = await service.ApplyTemporalCompression(new List<GpsPing>());
                result.Should().BeEmpty();
            }

            [Fact]
            public async Task ApplyTemporalCompression_AllMovingNormalSpeed_ReturnsAllPings()
            {
                var pings = new List<GpsPing>
                {
                    new GpsPing { Id = 1, Status = "moving", Speed = 30 },
                    new GpsPing { Id = 2, Status = "moving", Speed = 35 },
                    new GpsPing { Id = 3, Status = "moving", Speed = 40 },
                };
                var service = CreateCompressionInstance();
                var result = await service.ApplyTemporalCompression(pings);
                result.Count.Should().Be(3);
            }
        }
    }
