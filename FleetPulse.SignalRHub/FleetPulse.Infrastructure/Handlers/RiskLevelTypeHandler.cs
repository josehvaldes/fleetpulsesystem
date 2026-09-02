using Dapper;
using FleetPulse.Domain.Enums;
using System.Data;

namespace FleetPulse.Infrastructure.Handlers
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
