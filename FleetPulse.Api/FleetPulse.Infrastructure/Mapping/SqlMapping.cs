using Dapper;
using FleetPulse.Infrastructure.Handlers;

namespace FleetPulse.Infrastructure.Mapping
{
    public static class SqlMapping
    {
        public static void RegisterSqlMappings()
        {
            SqlMapper.Settings.PreferTypeHandlersForEnums = true;

            SqlMapper.AddTypeHandler(new AlertStatusTypeHandler());
            SqlMapper.AddTypeHandler(new RiskLevelTypeHandler());
        }
    }
}
