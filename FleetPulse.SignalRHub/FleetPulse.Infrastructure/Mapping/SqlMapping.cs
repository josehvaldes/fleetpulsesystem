using Dapper;
using FleetPulse.Infrastructure.Handlers;

namespace FleetPulse.SignalRHub.Mapping
{
    public static class SqlMapping
    {
        public static void RegisterSqlMappings()
        {
            // Set the preference for using type handlers for enums
            SqlMapper.Settings.PreferTypeHandlersForEnums = true;

            // SQL mappings
            SqlMapper.AddTypeHandler(new AlertStatusTypeHandler());
            SqlMapper.AddTypeHandler(new RiskLevelTypeHandler());
        }
    }
}
