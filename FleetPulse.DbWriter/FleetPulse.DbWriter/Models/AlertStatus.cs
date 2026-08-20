using System;
using System.Collections.Generic;
using System.Text;

namespace FleetPulse.DbWriter.Models
{
    public enum AlertStatus
    {
        New,
        InProgress,
        Resolved,
        Closed,
        OnError
    }
}
