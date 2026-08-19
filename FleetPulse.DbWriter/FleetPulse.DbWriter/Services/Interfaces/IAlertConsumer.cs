using System;
using System.Collections.Generic;
using System.Text;

namespace FleetPulse.DbWriter.Services.Interfaces
{
    public interface IAlertConsumer
    {
        Task StartConsumingAsync(CancellationToken stoppingToken);
    }
}
