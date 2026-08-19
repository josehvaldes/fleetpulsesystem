using FleetPulse.DbWriter.Configuration;
using FleetPulse.DbWriter.Models;
using FleetPulse.DbWriter.Services.Interfaces;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace FleetPulse.DbWriter.Services
{


    public class CompressionService: ICompressionService
    {
        private readonly KafkaSettings _kafkaSettings;

        public CompressionService(IOptions<KafkaSettings> settings)
        {
            _kafkaSettings = settings.Value;
        }

        public async Task<List<GpsPingDto>> ApplyTemporalCompression(IReadOnlyList<GpsPingDto> pings)
        {
            var dropped = await DropIntermediateStoppedPings(pings.ToList());
            var result = await ApplyHighSpeedCompression(dropped);
            return result;
        }

        /// <summary>
        /// Compresses consecutive high-speed pings by keeping only the first ping per time bucket.
        /// Normal-speed pings are always preserved.
        /// For example: [N,N,H1,H1,H1,H2,H2,H2,N,N] -> [N,N,H1,H2,N,N]
        /// where H1/H2 are high-speed pings in different time buckets (e.g. 15s windows).
        /// </summary>
        public async Task<List<GpsPingDto>> ApplyHighSpeedCompression(List<GpsPingDto> pings)
        {
            var seenBuckets = new HashSet<long>();
            var size = pings.Count;
            var compressed = pings
                .Where((ping, index) =>
                {
                    // Always keep the first and last pings in the list
                    if (index == 0 || index == size - 1)
                        return true;

                    if (ping.Speed <= _kafkaSettings.HighSpeedCompressionThreshold)
                        return true;

                    long bucket = ping.Timestamp.ToUnixTimeSeconds() / _kafkaSettings.HighSpeedCompressionBucketKey;
                    
                    return seenBuckets.Add(bucket); // Add returns false if bucket was already seen
                })
                .ToList();
            return compressed;
        }
        public async Task<List<GpsPingDto>> DropIntermediateStoppedPings(IReadOnlyList<GpsPingDto> pings)
        {
            if (pings.Count == 0 || pings.Count <= 2)
            {
                return pings.ToList();
            }

            var response = new List<GpsPingDto>();
            GpsPingDto? firstStopped = null;
            GpsPingDto? lastStopped = null;

            foreach (GpsPingDto ping in pings)
            {
                if (StoppedConditional(ping))
                {
                    if (firstStopped == null)
                    {
                        firstStopped = ping;
                        response.Add(ping);
                    }
                    lastStopped = ping;
                }
                else
                {
                    if (firstStopped != null && lastStopped != firstStopped)
                    {
                        response.Add(lastStopped!);
                    }
                    firstStopped = null;
                    lastStopped = null;
                    response.Add(ping);
                }
            }

            // If the sequence ends on a stopped run, preserve the last stopped ping
            if (firstStopped != null && lastStopped != firstStopped)
            {
                response.Add(lastStopped!);
            }

            return response;
        }

        /// <summary>
        /// Find the first ping with "stopped" status and the last ping with "stopped" status.
        /// Then remove all pings in between those two pings that have "stopped" status.
        /// Keep the first and last ping with "stopped" status.
        /// </summary>
        /// <param name="pings"></param>
        /// <returns></returns>
        public async Task<List<GpsPingDto>> DropIntermediateStoppedPingsLinq(List<GpsPingDto> pings)
        {

            if (pings.Count <= 2)
            {
                return pings.ToList();
            }

            return pings
                .Where((ping, i) =>
                    !StoppedConditional(ping) ||                          // always keep moving pings
                    i == 0 || i == pings.Count - 1 ||                    // always keep first and last
                    !StoppedConditional(pings[i - 1]) ||                  // first in a stopped run
                    !StoppedConditional(pings[i + 1]))                    // last in a stopped run
                .ToList();

        }

        public bool StoppedConditional(GpsPingDto ping)
        {
            return ping.Status == "stopped" || ping.Status == "idle" || ping.Status == "parked" || ping.Speed < 1;
        }




    }
}
