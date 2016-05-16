using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bars2Db.Data;
using Bars2Db.Mapping;
using Bars2Db.SqlProvider;
using Bars2Db.SqlQuery.QueryElements.SqlElements;
using Bars2Db.SqlQuery.QueryElements.SqlElements.Interfaces;

namespace Bars2Db.DataProvider
{
    internal class MultipleRowsHelper<T>
    {
        public readonly ColumnDescriptor[] Columns;
        public readonly ISqlDataType[] ColumnTypes;
        public readonly DataConnection DataConnection;
        public readonly EntityDescriptor Descriptor;
        public readonly BulkCopyOptions Options;
        public readonly string ParameterName;

        public readonly List<DataParameter> Parameters = new List<DataParameter>();
        public readonly BulkCopyRowsCopied RowsCopied = new BulkCopyRowsCopied();

        public readonly ISqlBuilder SqlBuilder;
        public readonly StringBuilder StringBuilder = new StringBuilder();
        public readonly string TableName;
        public readonly ValueToSqlConverter ValueConverter;
        public int BatchSize;

        public int CurrentCount;
        public int HeaderSize;
        public int ParameterIndex;

        public MultipleRowsHelper(DataConnection dataConnection, BulkCopyOptions options, bool enforceKeepIdentity)
        {
            DataConnection = dataConnection;
            Options = options;
            SqlBuilder = dataConnection.DataProvider.CreateSqlBuilder();
            ValueConverter = dataConnection.MappingSchema.ValueToSqlConverter;
            Descriptor = dataConnection.MappingSchema.GetEntityDescriptor(typeof(T));
            Columns = Descriptor.Columns
                .Where(c => !c.SkipOnInsert || enforceKeepIdentity && options.KeepIdentity == true && c.IsIdentity)
                .ToArray();
            ColumnTypes =
                Columns.Select(c => new SqlDataType(c.DataType, c.MemberType, c.Length, c.Precision, c.Scale)).ToArray();
            ParameterName = SqlBuilder.Convert("p", ConvertType.NameToQueryParameter).ToString();
            TableName = BasicBulkCopy.GetTableName(SqlBuilder, options, Descriptor);
            BatchSize = Math.Max(10, Options.MaxBatchSize ?? 1000);
        }

        public void SetHeader()
        {
            HeaderSize = StringBuilder.Length;
        }

        public void BuildColumns(object item)
        {
            for (var i = 0; i < Columns.Length; i++)
            {
                var column = Columns[i];
                var value = column.GetValue(item);

                if (!ValueConverter.TryConvert(StringBuilder, ColumnTypes[i], value))
                {
                    var name = ParameterName == "?" ? ParameterName : ParameterName + ++ParameterIndex;

                    StringBuilder.Append(name);

                    if (value is DataParameter)
                    {
                        value = ((DataParameter) value).Value;
                    }

                    Parameters.Add(new DataParameter(ParameterName == "?" ? ParameterName : "p" + ParameterIndex, value,
                        column.DataType));
                }

                StringBuilder.Append(",");
            }

            StringBuilder.Length--;
        }

        public bool Execute()
        {
            DataConnection.Execute(StringBuilder.AppendLine().ToString(), Parameters.ToArray());

            if (Options.RowsCopiedCallback != null)
            {
                Options.RowsCopiedCallback(RowsCopied);

                if (RowsCopied.Abort)
                    return false;
            }

            Parameters.Clear();
            ParameterIndex = 0;
            CurrentCount = 0;
            StringBuilder.Length = HeaderSize;

            return true;
        }
    }
}