using Dapper;
using FleetPulse.SignalRHub.Model;
using System.Data;

namespace FleetPulse.SignalRHub.Infrastructure
{
    public class AlertStatusTypeHandler : SqlMapper.TypeHandler<AlertStatus>
    {
        public override void SetValue(IDbDataParameter parameter, AlertStatus value)
        {
            parameter.DbType = DbType.String;
            parameter.Value = value.ToString();
        }

        public override AlertStatus Parse(object value)
        {
            return Enum.Parse<AlertStatus>(value.ToString()!, ignoreCase: true);
        }
    }
}
