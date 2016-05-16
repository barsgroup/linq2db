using System;
using System.Text;
using Bars2Db.Mapping;

namespace Bars2Db.DataProvider.PostgreSQL
{
    public class PostgreSQLMappingSchema : MappingSchema
    {
        public PostgreSQLMappingSchema() : this(ProviderName.PostgreSQL)
        {
        }

        protected PostgreSQLMappingSchema(string configuration) : base(configuration)
        {
            ColumnComparisonOption = StringComparison.OrdinalIgnoreCase;

            SetDataType(typeof(string), DataType.Undefined);

            SetValueToSqlConverter(typeof(bool), (sb, dt, v) => sb.Append(v));

            SetValueToSqlConverter(typeof(string), (sb, dt, v) => ConvertStringToSql(sb, v.ToString()));
            SetValueToSqlConverter(typeof(char), (sb, dt, v) => ConvertCharToSql(sb, (char) v));
        }

        private static void AppendConversion(StringBuilder stringBuilder, int value)
        {
            stringBuilder
                .Append("chr(")
                .Append(value)
                .Append(")")
                ;
        }

        private static void ConvertStringToSql(StringBuilder stringBuilder, string value)
        {
            DataTools.ConvertStringToSql(stringBuilder, "||", "'", AppendConversion, value);
        }

        private static void ConvertCharToSql(StringBuilder stringBuilder, char value)
        {
            DataTools.ConvertCharToSql(stringBuilder, "'", AppendConversion, value);
        }
    }
}