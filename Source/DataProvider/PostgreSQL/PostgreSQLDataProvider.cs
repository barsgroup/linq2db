﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq.Expressions;
using Bars2Db.Data;
using Bars2Db.Expressions;
using Bars2Db.Mapping;
using Bars2Db.SchemaProvider;
using Bars2Db.SqlProvider;

namespace Bars2Db.DataProvider.PostgreSQL
{
    public class PostgreSQLDataProvider : DynamicDataProviderBase
    {
        private static Action<IDbDataParameter> _setVarBinary;
        private static Action<IDbDataParameter> _setBoolean;
        private static Action<IDbDataParameter> _setXml;
        private static Action<IDbDataParameter> _setText;

        private readonly ISqlOptimizer _sqlOptimizer;
        private Type _npgsqlDate;

        private Type _npgsqlTimeStamp;
        private Type _npgsqlTimeStampTZ;

        internal Type BitStringType;
        internal Type NpgsqlBoxType;
        internal Type NpgsqlCircleType;
        internal Type NpgsqlInetType;
        internal Type NpgsqlIntervalType;
        internal Type NpgsqlLSegType;
        internal Type NpgsqlMacAddressType;
        internal Type NpgsqlPathType;
        internal Type NpgsqlPointType;
        internal Type NpgsqlPolygonType;
        internal Type NpgsqlTimeType;
        internal Type NpgsqlTimeTZType;

        public PostgreSQLDataProvider()
            : this(ProviderName.PostgreSQL, new PostgreSQLMappingSchema())
        {
        }

        protected PostgreSQLDataProvider(string name, MappingSchema mappingSchema)
            : base(name, mappingSchema)
        {
            SqlProviderFlags.IsInsertOrUpdateSupported = false;
            SqlProviderFlags.IsUpdateSetTableAliasSupported = false;

            SetCharField("bpchar", (r, i) => r.GetString(i).TrimEnd());

            _sqlOptimizer = new PostgreSQLSqlOptimizer(SqlProviderFlags);
        }

        public override string ConnectionNamespace => "Npgsql";

        protected override string ConnectionTypeName => "Npgsql.NpgsqlConnection, Npgsql";

        protected override string DataReaderTypeName => "Npgsql.NpgsqlDataReader, Npgsql";

        public override bool? IsDBNullAllowed(IDataReader reader, int idx)
        {  
            var st = ((DbDataReader)reader).GetSchemaTable();
            return st == null || st.Rows[idx]["AllowDBNull"] == DBNull.Value;
        }

        protected override void OnConnectionTypeCreated(Type connectionType)
        {
            BitStringType = connectionType.Assembly.GetType("NpgsqlTypes.BitString", false);
            NpgsqlIntervalType = connectionType.Assembly.GetType("NpgsqlTypes.NpgsqlInterval", false);
            NpgsqlInetType = connectionType.Assembly.GetType("NpgsqlTypes.NpgsqlInet", true);
            NpgsqlTimeType = connectionType.Assembly.GetType("NpgsqlTypes.NpgsqlTime", false);
            NpgsqlTimeTZType = connectionType.Assembly.GetType("NpgsqlTypes.NpgsqlTimeTZ", false);
            NpgsqlPointType = connectionType.Assembly.GetType("NpgsqlTypes.NpgsqlPoint", true);
            NpgsqlLSegType = connectionType.Assembly.GetType("NpgsqlTypes.NpgsqlLSeg", true);
            NpgsqlBoxType = connectionType.Assembly.GetType("NpgsqlTypes.NpgsqlBox", true);
            NpgsqlPathType = connectionType.Assembly.GetType("NpgsqlTypes.NpgsqlPath", true);
            _npgsqlTimeStamp = connectionType.Assembly.GetType("NpgsqlTypes.NpgsqlTimeStamp", false);
            _npgsqlTimeStampTZ = connectionType.Assembly.GetType("NpgsqlTypes.NpgsqlTimeStampTZ", false);
            _npgsqlDate = connectionType.Assembly.GetType("NpgsqlTypes.NpgsqlDate", true);
            NpgsqlMacAddressType = connectionType.Assembly.GetType("NpgsqlTypes.NpgsqlMacAddress", false);
            NpgsqlCircleType = connectionType.Assembly.GetType("NpgsqlTypes.NpgsqlCircle", true);
            NpgsqlPolygonType = connectionType.Assembly.GetType("NpgsqlTypes.NpgsqlPolygon", true);

            if (BitStringType != null) SetProviderField(BitStringType, BitStringType, "GetBitString");
            if (NpgsqlIntervalType != null) SetProviderField(NpgsqlIntervalType, NpgsqlIntervalType, "GetInterval");
            if (NpgsqlTimeType != null) SetProviderField(NpgsqlTimeType, NpgsqlTimeType, "GetTime");
            if (NpgsqlTimeTZType != null) SetProviderField(NpgsqlTimeTZType, NpgsqlTimeTZType, "GetTimeTZ");
            if (_npgsqlTimeStamp != null) SetProviderField(_npgsqlTimeStamp, _npgsqlTimeStamp, "GetTimeStamp");
            if (_npgsqlTimeStampTZ != null) SetProviderField(_npgsqlTimeStampTZ, _npgsqlTimeStampTZ, "GetTimeStampTZ");
            if (NpgsqlMacAddressType != null)
                SetProviderField(NpgsqlMacAddressType, NpgsqlMacAddressType, "GetProviderSpecificValue");

            SetProviderField(NpgsqlInetType, NpgsqlInetType, "GetProviderSpecificValue");
            SetProviderField(_npgsqlDate, _npgsqlDate, "GetDate");

            if (_npgsqlTimeStampTZ != null)
            {
                // SetProviderField2<NpgsqlDataReader,DateTimeOffset,NpgsqlTimeStampTZ>((r,i) => (NpgsqlTimeStampTZ)r.GetProviderSpecificValue(i));

                var dataReaderParameter = Expression.Parameter(DataReaderType, "r");
                var indexParameter = Expression.Parameter(typeof(int), "i");

                ReaderExpressions[
                    new ReaderInfo {ToType = typeof(DateTimeOffset), ProviderFieldType = _npgsqlTimeStampTZ}] =
                    Expression.Lambda(
                        Expression.Convert(
                            Expression.Call(dataReaderParameter, "GetProviderSpecificValue", null, indexParameter),
                            _npgsqlTimeStampTZ),
                        dataReaderParameter,
                        indexParameter);
            }

            _setVarBinary = GetSetParameter(connectionType, "NpgsqlParameter", "NpgsqlDbType",
                "NpgsqlTypes.NpgsqlDbType", "Bytea");
            _setBoolean = GetSetParameter(connectionType, "NpgsqlParameter", "NpgsqlDbType", "NpgsqlTypes.NpgsqlDbType",
                "Boolean");
            _setXml = GetSetParameter(connectionType, "NpgsqlParameter", "NpgsqlDbType", "NpgsqlTypes.NpgsqlDbType",
                "Xml");
            _setText = GetSetParameter(connectionType, "NpgsqlParameter", "NpgsqlDbType", "NpgsqlTypes.NpgsqlDbType",
                "Text");

            if (BitStringType != null) MappingSchema.AddScalarType(BitStringType);
            if (NpgsqlIntervalType != null) MappingSchema.AddScalarType(NpgsqlIntervalType);
            if (NpgsqlTimeType != null) MappingSchema.AddScalarType(NpgsqlTimeType);
            if (NpgsqlTimeTZType != null) MappingSchema.AddScalarType(NpgsqlTimeTZType);
            if (_npgsqlTimeStamp != null) MappingSchema.AddScalarType(_npgsqlTimeStamp);
            if (_npgsqlTimeStampTZ != null) MappingSchema.AddScalarType(_npgsqlTimeStampTZ);
            if (NpgsqlMacAddressType != null) MappingSchema.AddScalarType(NpgsqlMacAddressType);

            MappingSchema.AddScalarType(NpgsqlInetType);
            MappingSchema.AddScalarType(NpgsqlPointType);
            MappingSchema.AddScalarType(NpgsqlLSegType);
            MappingSchema.AddScalarType(NpgsqlBoxType);
            MappingSchema.AddScalarType(NpgsqlPathType);
            MappingSchema.AddScalarType(NpgsqlCircleType);
            MappingSchema.AddScalarType(_npgsqlDate);
            MappingSchema.AddScalarType(NpgsqlPolygonType);

            if (_npgsqlTimeStampTZ != null)
            {
                // SetConvertExpression<NpgsqlTimeStampTZ,DateTimeOffset>(
                //     d => new DateTimeOffset(d.Year, d.Month, d.Day, d.Hours, d.Minutes, d.Seconds, d.Milliseconds,
                //         new TimeSpan(d.TimeZone.Hours, d.TimeZone.Minutes, d.TimeZone.Seconds)));

                var p = Expression.Parameter(_npgsqlTimeStampTZ, "p");

                MappingSchema.SetConvertExpression(_npgsqlTimeStampTZ, typeof(DateTimeOffset),
                    Expression.Lambda(
                        Expression.New(
                            MemberHelper.ConstructorOf(() => new DateTimeOffset(0L, new TimeSpan())),
                            Expression.PropertyOrField(p, "Ticks"),
                            Expression.New(
                                MemberHelper.ConstructorOf(() => new TimeSpan(0, 0, 0)),
                                Expression.PropertyOrField(Expression.PropertyOrField(p, "TimeZone"), "Hours"),
                                Expression.PropertyOrField(Expression.PropertyOrField(p, "TimeZone"), "Minutes"),
                                Expression.PropertyOrField(Expression.PropertyOrField(p, "TimeZone"), "Seconds"))),
                        p
                        ));
            }
        }

        public override ISqlBuilder CreateSqlBuilder()
        {
            return new PostgreSQLSqlBuilder(GetSqlOptimizer(), SqlProviderFlags, MappingSchema.ValueToSqlConverter);
        }

        public override ISqlOptimizer GetSqlOptimizer()
        {
            return _sqlOptimizer;
        }

        public override ISchemaProvider GetSchemaProvider()
        {
            return new PostgreSQLSchemaProvider();
        }

        public override void SetParameter(IDbDataParameter parameter, string name, DataType dataType, object value)
        {
//			if (BitStringType == null && value == null && dataType == DataType.Undefined)
//			{
//				dataType = DataType.Char;
//			}

            base.SetParameter(parameter, name, dataType, value);
        }

        protected override void SetParameterType(IDbDataParameter parameter, DataType dataType)
        {
            switch (dataType)
            {
                case DataType.SByte:
                    parameter.DbType = DbType.Int16;
                    break;
                case DataType.UInt16:
                    parameter.DbType = DbType.Int32;
                    break;
                case DataType.UInt32:
                    parameter.DbType = DbType.Int64;
                    break;
                case DataType.UInt64:
                    parameter.DbType = DbType.Decimal;
                    break;
                case DataType.DateTime2:
                    parameter.DbType = DbType.DateTime;
                    break;
                case DataType.VarNumeric:
                    parameter.DbType = DbType.Decimal;
                    break;
                case DataType.Decimal:
                case DataType.Money:
                    break;
                case DataType.Image:
                case DataType.Binary:
                case DataType.VarBinary:
                    _setVarBinary(parameter);
                    break;
                case DataType.Boolean:
                    _setBoolean(parameter);
                    break;
                case DataType.Xml:
                    _setXml(parameter);
                    break;
                case DataType.Text:
                case DataType.NText:
                    _setText(parameter);
                    break;
                case DataType.Hierarchical:
                    _setText(parameter);
                    break;
                case DataType.Undefined:
                    break;
                case DataType.Char:
                    break;
                case DataType.VarChar:
                    break;
                case DataType.NChar:
                    break;
                case DataType.NVarChar:
                    break;
                case DataType.Blob:
                    break;
                case DataType.Guid:
                    break;
                case DataType.Int16:
                    break;
                case DataType.Int32:
                    break;
                case DataType.Int64:
                    break;
                case DataType.Byte:
                    break;
                case DataType.Single:
                    break;
                case DataType.Double:
                    break;
                case DataType.SmallMoney:
                    break;
                case DataType.Date:
                    break;
                case DataType.Time:
                    break;
                case DataType.DateTime:
                    break;
                case DataType.SmallDateTime:
                    break;
                case DataType.DateTimeOffset:
                    break;
                case DataType.Timestamp:
                    break;
                case DataType.Variant:
                    break;
                case DataType.Udt:
                    break;
                default:
                    base.SetParameterType(parameter, dataType);
                    break;
            }
        }

        public override void InitCommand(DataConnection dataConnection, CommandType commandType, string commandText,
            DataParameter[] parameters)
        {
            EnsureConnection();

            base.InitCommand(dataConnection, commandType, commandText, parameters);
        }

        #region BulkCopy

        public override BulkCopyRowsCopied BulkCopy<T>(
            [Properties.NotNull] DataConnection dataConnection, BulkCopyOptions options, IEnumerable<T> source)
        {
            return new PostgreSQLBulkCopy().BulkCopy(
                options.BulkCopyType == BulkCopyType.Default
                    ? PostgreSQLTools.DefaultBulkCopyType
                    : options.BulkCopyType,
                dataConnection,
                options,
                source);
        }

        #endregion
    }
}