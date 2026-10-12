using Dapper;
using FleetPulse.Alerts.Infrastructure.Handlers;

namespace FleetPulse.Alerts.Infrastructure.Mapping
{
    public static class AlertsSqlMapping
    {
        public static void RegisterSqlMappings()
        {
            SqlMapper.Settings.PreferTypeHandlersForEnums = true;

            SqlMapper.AddTypeHandler(new AlertStatusTypeHandler());
            SqlMapper.AddTypeHandler(new RiskLevelTypeHandler());
        }
    }
}
