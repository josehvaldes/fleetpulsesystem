using Dapper;
using FleetPulse.SignalRHub.Model;
using System.Data;

namespace FleetPulse.SignalRHub.Infrastructure
{
    public class RiskLevelTypeHandler : SqlMapper.TypeHandler<RiskLevel>
    {
        public override void SetValue(IDbDataParameter parameter, RiskLevel value)
        {
            parameter.DbType = DbType.String;
            parameter.Value = value.ToString();
        }
        public override RiskLevel Parse(object value)
        {
            return Enum.Parse<RiskLevel>(value.ToString()!, ignoreCase: true);
        }
    }
}
