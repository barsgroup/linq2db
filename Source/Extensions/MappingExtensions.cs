using System;
using Bars2Db.Common;
using Bars2Db.Mapping;
using Bars2Db.SqlEntities;
using Bars2Db.SqlQuery.QueryElements.SqlElements;

namespace Bars2Db.Extensions
{
    internal static class MappingExtensions
    {
        public static ISqlValue GetSqlValue(this MappingSchema mappingSchema, object value)
        {
            if (value == null)
                throw new InvalidOperationException();

            return GetSqlValue(mappingSchema, value.GetType(), value);
        }

        public static ISqlValue GetSqlValue(this MappingSchema mappingSchema, Type systemType, object value)
        {
            var underlyingType = systemType.ToNullableUnderlying();

            if (underlyingType.IsEnumEx() && mappingSchema.GetAttribute<Sql.EnumAttribute>(underlyingType) == null)
            {
                if (value != null || systemType == underlyingType)
                {
                    var type = Converter.GetDefaultMappingFromEnumType(mappingSchema, systemType);

                    return new SqlValue(type, Converter.ChangeType(value, type, mappingSchema));
                }
            }

            if (systemType == typeof(object) && value != null)
                systemType = value.GetType();

            return new SqlValue(systemType, value);
        }
    }
}