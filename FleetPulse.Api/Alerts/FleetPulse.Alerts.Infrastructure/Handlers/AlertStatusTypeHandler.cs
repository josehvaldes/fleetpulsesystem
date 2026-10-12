using Dapper;
using FleetPulse.Alerts.Domain.Enums;
using System.Data;

namespace FleetPulse.Alerts.Infrastructure.Handlers
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
