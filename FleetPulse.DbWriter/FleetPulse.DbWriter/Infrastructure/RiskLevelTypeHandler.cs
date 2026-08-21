using Dapper;
using FleetPulse.DbWriter.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace FleetPulse.DbWriter.Infrastructure
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
