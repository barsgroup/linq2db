using System;
using System.Data;
using System.Linq;
using System.Text;

namespace LinqToDB.DataProvider.PostgreSQL
{
	using Common;
	using SqlQuery;
	using SqlProvider;

	class PostgreSQLSqlBuilder : BasicSqlBuilder
	{
		public PostgreSQLSqlBuilder(ISqlOptimizer sqlOptimizer, SqlProviderFlags sqlProviderFlags, ValueToSqlConverter valueToSqlConverter)
			: base(sqlOptimizer, sqlProviderFlags, valueToSqlConverter)
		{
		}

		public override int CommandCount(SelectQuery selectQuery)
		{
			return selectQuery.IsInsert && selectQuery.Insert.WithIdentity ? 2 : 1;
		}

		protected override void BuildCommand(int commandNumber)
		{
			var into = SelectQuery.Insert.Into;
			var attr = GetSequenceNameAttribute(into, false);
			var name =
				attr != null ?
					attr.SequenceName :
					Convert(
						string.Format("{0}_{1}_seq", into.PhysicalName, into.GetIdentityField().PhysicalName),
						ConvertType.NameToQueryField);

			name = Convert(name, ConvertType.NameToQueryTable);

			var database = GetTableDatabaseName(into);
			var owner    = GetTableOwnerName   (into);

			AppendIndent()
				.Append("SELECT currval('");

			BuildTableName(StringBuilder, database, owner, name.ToString());

			StringBuilder.AppendLine("')");
		}

		protected override ISqlBuilder CreateSqlBuilder()
		{
			return new PostgreSQLSqlBuilder(SqlOptimizer, SqlProviderFlags, ValueToSqlConverter);
		}

		protected override string LimitFormat  { get { return "LIMIT {0}";   } }
		protected override string OffsetFormat { get { return "OFFSET {0} "; } }

		protected override void BuildDataType(SqlDataType type, bool createDbType = false)
		{
			switch (type.DataType)
			{
				case DataType.SByte         :
				case DataType.Byte          : StringBuilder.Append("SmallInt");      break;
				case DataType.Money         : StringBuilder.Append("Decimal(19,4)"); break;
				case DataType.SmallMoney    : StringBuilder.Append("Decimal(10,4)"); break;
				case DataType.DateTime2     :
				case DataType.SmallDateTime :
				case DataType.DateTime      : StringBuilder.Append("TimeStamp");     break;
				case DataType.Boolean       : StringBuilder.Append("Boolean");       break;
                case DataType.Binary        :
                case DataType.VarBinary     :
                case DataType.Blob          :
                case DataType.Image         : StringBuilder.Append("Bytea");         break;
				case DataType.NVarChar      :
					StringBuilder.Append("VarChar");
					if (type.Length > 0)
						StringBuilder.Append('(').Append(type.Length).Append(')');
					break;
				case DataType.Undefined      :
					if (type.Type == typeof(string))
						goto case DataType.NVarChar;
					break;
				default                      : base.BuildDataType(type); break;
			}
		}

		protected override void BuildFromClause()
		{
			if (!SelectQuery.IsUpdate)
				base.BuildFromClause();
		}

		public static PostgreSQLIdentifierQuoteMode IdentifierQuoteMode = PostgreSQLIdentifierQuoteMode.Auto;

		public override object Convert(object value, ConvertType convertType)
		{
			switch (convertType)
			{
				case ConvertType.NameToQueryField:
				case ConvertType.NameToQueryFieldAlias:
				case ConvertType.NameToQueryTable:
				case ConvertType.NameToQueryTableAlias:
				case ConvertType.NameToDatabase:
				case ConvertType.NameToOwner:
					if (value != null && IdentifierQuoteMode != PostgreSQLIdentifierQuoteMode.None)
					{
						var name = value.ToString();

						if (name.Length > 0 && name[0] == '"')
							return name;

						if (IdentifierQuoteMode == PostgreSQLIdentifierQuoteMode.Quote ||
							name
#if NETFX_CORE
								.ToCharArray()
#endif
								.Any(c => char.IsUpper(c) || char.IsWhiteSpace(c)))
							return '"' + name + '"';
					}

					break;

				case ConvertType.NameToQueryParameter:
				case ConvertType.NameToCommandParameter:
				case ConvertType.NameToSprocParameter:
					return ":" + value;

				case ConvertType.SprocParameterToName:
					if (value != null)
					{
						var str = value.ToString();
						return (str.Length > 0 && str[0] == ':')? str.Substring(1): str;
					}

					break;
			}

			return value;
		}

		public override ISqlExpression GetIdentityExpression(SqlTable table)
		{
			if (!table.SequenceAttributes.IsNullOrEmpty())
			{
				var attr = GetSequenceNameAttribute(table, false);

				if (attr != null)
				{
					var name     = Convert(attr.SequenceName, ConvertType.NameToQueryTable).ToString();
					var database = GetTableDatabaseName(table);
					var owner    = GetTableOwnerName   (table);

					var sb = BuildTableName(new StringBuilder(), database, owner, name);

					return new SqlExpression("nextval('" + sb + "')", Precedence.Primary);
				}
			}

			return base.GetIdentityExpression(table);
		}

		protected override void BuildCreateTableFieldType(SqlField field)
		{
			if (field.IsIdentity)
			{
				if (field.DataType == DataType.Int32)
				{
					StringBuilder.Append("SERIAL");
					return;
				}

				if (field.DataType == DataType.Int64)
				{
					StringBuilder.Append("BIGSERIAL");
					return;
				}
			}

			base.BuildCreateTableFieldType(field);
		}

	    protected override void BuildLikePredicate(SelectQuery.Predicate.Like predicate)
	    {
	        var expr1 = predicate.Expr1;
	        var sqlField = expr1 as SqlField;
	        if (sqlField == null)
	        {
	            var column = expr1 as SelectQuery.Column;
	            if (column != null)
	            {
	                sqlField = column.Expression as SqlField;
	            }
	        }

	        if (sqlField != null && sqlField.ColumnDescriptor.IsHierarchical)
            {
                var expr2 = predicate.Expr2;

                var sqlValue = expr2 as SqlValue;
                if (sqlValue != null)
                {
                    var str = (string) sqlValue.Value;
                    var vStart = str[0] == '%' ? "%" : string.Empty;
                    var vEnd = str[str.Length - 1] == '%' ? "%" : string.Empty;
                    str = str.Substring(vStart.Length, str.Length - vStart.Length - vEnd.Length);

                    var vNewPredicate = new SelectQuery.Predicate.HierarhicalLike(expr1,
                        new SqlValue(str), vStart, vEnd);

                    base.BuildLikePredicate(vNewPredicate);
                    return;
                }

                var sqlParameter = expr2 as SqlParameter;
                if (sqlParameter != null)
                {
                    var pStart = sqlParameter.LikeStart;
                    var pEnd = sqlParameter.LikeEnd;
                    sqlParameter.LikeStart = string.Empty;
                    sqlParameter.LikeEnd = string.Empty;

                    var pNewPredicate = new SelectQuery.Predicate.HierarhicalLike(expr1, sqlParameter, pStart, pEnd);
                    base.BuildLikePredicate(pNewPredicate);
                    return;
                }

                ISqlExpression value = null;
                var hasLikeStart = false;
                var hasLikeEnd = false;
                var fun = predicate.Expr2 as SqlFunction;
                if (fun != null)
                {
                    value = GetSqlExpressionFromFunction(fun);
                }
                else
                {
                    var sqlBinary = predicate.Expr2 as SqlBinaryExpression;
                    if (sqlBinary != null)
                    {
                        var function = GetFunctionFromBinary(sqlBinary, out hasLikeStart, out hasLikeEnd);
                        if (function != null)
                        {
                            value = GetSqlExpressionFromFunction(function);                            
                        }
                    }
                }

                if (value != null)
                {
                    var ePredicate = new SelectQuery.Predicate.HierarhicalLike(expr1, value, hasLikeStart ? "%" : string.Empty, hasLikeEnd ? "%" : string.Empty);
                    base.BuildLikePredicate(ePredicate);
                    return;
                }
            }
            
            base.BuildLikePredicate(predicate);                
	    }

	    private SqlFunction GetFunctionFromBinary(SqlBinaryExpression sqlBinary, out bool hasLikeStart, out bool hasLikeEnd)
	    {
	        hasLikeStart = false;
	        hasLikeEnd = false;
	        SqlFunction function = null;
	        var list = new [] { sqlBinary.Expr1, sqlBinary.Expr2};
	        foreach (var expression in list)
	        {
	            var fun = expression as SqlFunction;
	            if (fun != null)
	            {
                    function = fun;
	            }

	            var binary = expression as SqlBinaryExpression;
	            if (binary != null)
	            {
                    function = GetFunctionFromBinary(binary, out hasLikeStart, out hasLikeEnd);
	            }

	            var sqlValue = expression as SqlValue;
                if (sqlValue != null)
	            {
	                if (sqlBinary.Expr1 == expression)
	                {
	                    hasLikeStart = true;
	                }
	                else
	                {
	                    hasLikeEnd = true;
	                }
	            }
	        }

	        return function;
	    }

	    private ISqlExpression GetSqlExpressionFromFunction(SqlFunction sqlFunction)
	    {
	        foreach (var parameter in sqlFunction.Parameters)
	        {
	            var fun = parameter as SqlFunction;
	            if (fun != null)
	            {
                    return GetSqlExpressionFromFunction(fun);
	            }

	            var field = parameter as SqlField;
                if (field != null)
                {
                    return field;
                }

                var column = parameter as SelectQuery.Column;
                if (column != null)
                {
                    return column;
                }
	        }

	        return null;
	    }

#if !SILVERLIGHT

		protected override string GetProviderTypeName(IDbDataParameter parameter)
		{
			dynamic p = parameter;
			return p.NpgsqlDbType.ToString();
		}

#endif
	}
}
