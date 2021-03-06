﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Bars2Db.Common;
using Bars2Db.Data;
using Bars2Db.Extensions;
using Bars2Db.Linq;
using Bars2Db.Mapping;
using Bars2Db.SqlProvider;
using Bars2Db.SqlQuery;
using Bars2Db.SqlQuery.QueryElements.Enums;
using Bars2Db.SqlQuery.QueryElements.Interfaces;
using Bars2Db.SqlQuery.QueryElements.SqlElements;
using Bars2Db.SqlQuery.QueryElements.SqlElements.Interfaces;

namespace Bars2Db.DataProvider
{
    internal class BasicMerge
    {
        protected string ByTargetText;
        protected List<ColumnInfo> Columns;
        protected List<DataParameter> Parameters = new List<DataParameter>();

        protected StringBuilder StringBuilder = new StringBuilder();

        protected virtual bool IsIdentitySupported => false;

        public virtual int Merge<T>(DataConnection dataConnection, Expression<Func<T, bool>> predicate, bool delete,
            IEnumerable<T> source,
            string tableName, string databaseName, string schemaName)
            where T : class
        {
            if (!BuildCommand(dataConnection, predicate, delete, source, tableName, databaseName, schemaName))
                return 0;

            return Execute(dataConnection);
        }

        protected virtual bool BuildCommand<T>(
            DataConnection dataConnection, Expression<Func<T, bool>> deletePredicate, bool delete, IEnumerable<T> source,
            string tableName, string databaseName, string schemaName)
            where T : class
        {
            var table = dataConnection.MappingSchema.GetEntityDescriptor(typeof(T));
            var sqlBuilder = dataConnection.DataProvider.CreateSqlBuilder();

            Columns = table.Columns
                .Select(c => new ColumnInfo
                {
                    Column = c,
                    Name = (string) sqlBuilder.Convert(c.ColumnName, ConvertType.NameToQueryField)
                })
                .ToList();

            StringBuilder.Append("MERGE INTO ");
            sqlBuilder.BuildTableName(StringBuilder,
                (string) sqlBuilder.Convert(databaseName ?? table.DatabaseName, ConvertType.NameToDatabase),
                (string) sqlBuilder.Convert(schemaName ?? table.SchemaName, ConvertType.NameToOwner),
                (string) sqlBuilder.Convert(tableName ?? table.TableName, ConvertType.NameToQueryTable));

            StringBuilder
                .AppendLine(" Target")
                ;

            if (!BuildUsing(dataConnection, source))
                return false;

            StringBuilder
                .AppendLine("ON")
                .AppendLine("(")
                ;

            foreach (var column in Columns.Where(c => c.Column.IsPrimaryKey))
            {
                StringBuilder
                    .AppendFormat("\tTarget.{0} = Source.{0} AND", column.Name)
                    .AppendLine()
                    ;
            }

            StringBuilder.Length -= 4 + Environment.NewLine.Length;

            StringBuilder
                .AppendLine()
                .AppendLine(")")
                ;

            var updateColumns =
                Columns.Where(
                    c =>
                        !c.Column.IsPrimaryKey && (IsIdentitySupported && c.Column.IsIdentity || !c.Column.SkipOnUpdate))
                    .ToList();

            if (updateColumns.Count > 0)
            {
                StringBuilder
                    .AppendLine("-- update matched rows")
                    .AppendLine("WHEN MATCHED THEN")
                    .AppendLine("\tUPDATE")
                    .AppendLine("\tSET")
                    ;

                var maxLen = updateColumns.Max(c => c.Name.Length);

                foreach (var column in updateColumns)
                {
                    StringBuilder
                        .AppendFormat("\t\t{0} ", column.Name)
                        ;

                    StringBuilder.Append(' ', maxLen - column.Name.Length);

                    StringBuilder
                        .AppendFormat("= Source.{0},", column.Name)
                        .AppendLine()
                        ;
                }

                StringBuilder.Length -= 1 + Environment.NewLine.Length;
            }

            var insertColumns =
                Columns.Where(c => IsIdentitySupported && c.Column.IsIdentity || !c.Column.SkipOnInsert).ToList();

            StringBuilder
                .AppendLine()
                .AppendLine("-- insert new rows")
                .Append("WHEN NOT MATCHED ").Append(ByTargetText).AppendLine("THEN")
                .AppendLine("\tINSERT")
                .AppendLine("\t(")
                ;

            foreach (var column in insertColumns)
                StringBuilder.AppendFormat("\t\t{0},", column.Name).AppendLine();

            StringBuilder.Length -= 1 + Environment.NewLine.Length;

            StringBuilder
                .AppendLine()
                .AppendLine("\t)")
                .AppendLine("\tVALUES")
                .AppendLine("\t(")
                ;

            foreach (var column in insertColumns)
                StringBuilder.AppendFormat("\t\tSource.{0},", column.Name).AppendLine();

            StringBuilder.Length -= 1 + Environment.NewLine.Length;

            StringBuilder
                .AppendLine()
                .AppendLine("\t)")
                ;

            if (delete)
            {
                var predicate = "";

                if (deletePredicate != null)
                {
                    var inlineParameters = dataConnection.InlineParameters;

                    try
                    {
                        dataConnection.InlineParameters = true;

                        var q = dataConnection.GetTable<T>().Where(deletePredicate);
                        var ctx = q.GetContext();
                        var sql = ctx.Select;

                        var tableSet = new HashSet<ISqlTable>();
                        var tables = new List<ISqlTable>();

                        var fromTable = (ISqlTable) sql.From.Tables.First.Value.Source;

                        foreach (var tableSource in QueryVisitor.FindOnce<ITableSource>(sql.From))
                        {
                            tableSet.Add((ISqlTable) tableSource.Source);
                            tables.Add((ISqlTable) tableSource.Source);
                        }

                        var whereClause = new QueryVisitor().Convert(sql.Where, e =>
                        {
                            if (e.ElementType == EQueryElementType.SqlQuery)
                            {
                            }

                            if (e.ElementType == EQueryElementType.SqlField)
                            {
                                var fld = (ISqlField) e;
                                var tbl = (ISqlTable) fld.Table;

                                if (tbl != fromTable && tableSet.Contains(tbl))
                                {
                                    var tempCopy = sql.Clone();
                                    var tempTables = new List<ITableSource>();

                                    tempTables.AddRange(QueryVisitor.FindOnce<ITableSource>(tempCopy.From));

                                    var tt = tempTables[tables.IndexOf(tbl)];

                                    tempCopy.Select.Columns.Clear();
                                    tempCopy.Select.Add(((SqlTable) tt.Source).Fields[fld.Name]);

                                    tempCopy.Where.Search.Conditions.Clear();

                                    var keys = tempCopy.From.Tables.First.Value.Source.GetKeys(true);

                                    foreach (ISqlField key in keys)
                                        tempCopy.Where.Field(key).Equal.Field(fromTable.Fields[key.Name]);

                                    tempCopy.ParentSelect = sql;

                                    return tempCopy;
                                }
                            }

                            return e;
                        }).Search.Conditions.ToList();

                        sql.Where.Search.Conditions.Clear();
                        sql.Where.Search.Conditions.AddRange(whereClause);

                        sql.From.Tables.First.Value.Alias = "Target";

                        ctx.SetParameters();

                        var pq =
                            (DataConnection.PreparedQuery) ((IDataContext) dataConnection).SetQuery(new QueryContext
                            {
                                SelectQuery = sql,
                                SqlParameters = sql.Parameters.ToArray()
                            });

                        var cmd = pq.Commands[0];

                        predicate = "AND " + cmd.Substring(cmd.IndexOf("WHERE") + "WHERE".Length);
                    }
                    finally
                    {
                        dataConnection.InlineParameters = inlineParameters;
                    }
                }

                StringBuilder
                    .AppendLine("-- delete rows that are in the target but not in the sourse")
                    .AppendLine("WHEN NOT MATCHED BY Source {0}THEN".Args(predicate))
                    .AppendLine("\tDELETE")
                    ;
            }

            return true;
        }

        protected virtual bool BuildUsing<T>(DataConnection dataConnection, IEnumerable<T> source)
        {
            var table = dataConnection.MappingSchema.GetEntityDescriptor(typeof(T));
            var sqlBuilder = dataConnection.DataProvider.CreateSqlBuilder();
            var pname = sqlBuilder.Convert("p", ConvertType.NameToQueryParameter).ToString();
            var valueConverter = dataConnection.MappingSchema.ValueToSqlConverter;

            StringBuilder
                .AppendLine("USING")
                .AppendLine("(")
                .AppendLine("\tVALUES")
                ;

            var pidx = 0;

            var hasData = false;
            var columnTypes = table.Columns
                .Select(c => new SqlDataType(c.DataType, c.MemberType, c.Length, c.Precision, c.Scale))
                .ToArray();

            foreach (var item in source)
            {
                hasData = true;

                StringBuilder.Append("\t(");

                for (var i = 0; i < table.Columns.Count; i++)
                {
                    var column = table.Columns[i];
                    var value = column.GetValue(item);

                    if (!valueConverter.TryConvert(StringBuilder, columnTypes[i], value))
                    {
                        var name = pname == "?" ? pname : pname + ++pidx;

                        StringBuilder.Append(name);
                        Parameters.Add(new DataParameter(pname == "?" ? pname : "p" + pidx, value,
                            column.DataType));
                    }

                    StringBuilder.Append(",");
                }

                StringBuilder.Length--;
                StringBuilder.AppendLine("),");
            }

            if (hasData)
            {
                var idx = StringBuilder.Length;
                while (StringBuilder[--idx] != ',')
                {
                }
                StringBuilder.Remove(idx, 1);

                StringBuilder
                    .AppendLine(")")
                    .AppendLine("AS Source")
                    .AppendLine("(")
                    ;

                foreach (var column in Columns)
                    StringBuilder.AppendFormat("\t{0},", column.Name).AppendLine();

                StringBuilder.Length -= 1 + Environment.NewLine.Length;

                StringBuilder
                    .AppendLine()
                    .AppendLine(")")
                    ;
            }

            return hasData;
        }

        protected bool BuildUsing2<T>(DataConnection dataConnection, IEnumerable<T> source, string top,
            string fromDummyTable)
        {
            var table = dataConnection.MappingSchema.GetEntityDescriptor(typeof(T));
            var sqlBuilder = dataConnection.DataProvider.CreateSqlBuilder();
            var pname = sqlBuilder.Convert("p", ConvertType.NameToQueryParameter).ToString();
            var valueConverter = dataConnection.MappingSchema.ValueToSqlConverter;

            StringBuilder
                .AppendLine("USING")
                .AppendLine("(")
                ;

            var pidx = 0;

            var hasData = false;
            var columnTypes = table.Columns
                .Select(c => new SqlDataType(c.DataType, c.MemberType, c.Length, c.Precision, c.Scale))
                .ToArray();

            foreach (var item in source)
            {
                if (hasData)
                    StringBuilder.Append(" UNION ALL").AppendLine();

                StringBuilder.Append("\tSELECT ");

                if (top != null)
                    StringBuilder.Append(top);

                for (var i = 0; i < Columns.Count; i++)
                {
                    var column = Columns[i];
                    var value = column.Column.GetValue(item);

                    if (!valueConverter.TryConvert(StringBuilder, columnTypes[i], value))
                    {
                        var name = pname == "?" ? pname : pname + ++pidx;

                        StringBuilder.Append(name);
                        Parameters.Add(new DataParameter(pname == "?" ? pname : "p" + pidx, value,
                            column.Column.DataType));
                    }

                    if (!hasData)
                        StringBuilder.Append(" as ").Append(column.Name);

                    StringBuilder.Append(",");
                }

                StringBuilder.Length--;
                StringBuilder.Append(' ').Append(fromDummyTable);

                hasData = true;
            }

            if (hasData)
            {
                StringBuilder.AppendLine();

                StringBuilder
                    .AppendLine(")")
                    .AppendLine("Source")
                    ;
            }

            return hasData;
        }

        protected virtual int Execute(DataConnection dataConnection)
        {
            var cmd = StringBuilder.AppendLine().ToString();

            return dataConnection.Execute(cmd, Parameters.ToArray());
        }

        protected class ColumnInfo
        {
            public ColumnDescriptor Column;
            public string Name;
        }

        private class QueryContext : IQueryContext
        {
            public ISqlParameter[] SqlParameters;
            public ISelectQuery SelectQuery { get; set; }
            public object Context { get; set; }
            public List<string> QueryHints { get; set; }

            public ISqlParameter[] GetParameters()
            {
                return SqlParameters;
            }
        }
    }
}