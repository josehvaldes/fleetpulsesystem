using Dapper;
using FleetPulse.DbWriter.Infrastructure;

namespace FleetPulse.DbWriter.Mappings
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
