using System.Data;

namespace LinqToDB.DataProvider.SqlCe
{
    using LinqToDB.SqlQuery.QueryElements.Conditions;
    using LinqToDB.SqlQuery.QueryElements.Interfaces;
    using LinqToDB.SqlQuery.QueryElements.SqlElements;
    using LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces;

    using SqlProvider;

	class SqlCeSqlBuilder : BasicSqlBuilder
	{
		public SqlCeSqlBuilder(ISqlOptimizer sqlOptimizer, SqlProviderFlags sqlProviderFlags, ValueToSqlConverter valueToSqlConverter)
			: base(sqlOptimizer, sqlProviderFlags, valueToSqlConverter)
		{
		}

		protected override string FirstFormat  { get { return SelectQuery.Select.SkipValue == null ? "TOP ({0})" :                null; } }
		protected override string LimitFormat  { get { return SelectQuery.Select.SkipValue != null ? "FETCH NEXT {0} ROWS ONLY" : null; } }
		protected override string OffsetFormat { get { return "OFFSET {0} ROWS"; } }
		protected override bool   OffsetFirst  { get { return true;              } }

		public override int CommandCount(ISelectQuery selectQuery)
		{
			return selectQuery.IsInsert && selectQuery.Insert.WithIdentity ? 2 : 1;
		}

		protected override void BuildCommand(int commandNumber)
		{
			StringBuilder.AppendLine("SELECT @@IDENTITY");
		}

		protected override ISqlBuilder CreateSqlBuilder()
		{
			return new SqlCeSqlBuilder(SqlOptimizer, SqlProviderFlags, ValueToSqlConverter);
		}

		protected override void BuildFunction(ISqlFunction func)
		{
			func = ConvertFunctionParameters(func);
			base.BuildFunction(func);
		}

		protected override void BuildDataType(ISqlDataType type, bool createDbType = false)
		{
			switch (type.DataType)
			{
				case DataType.Char          : base.BuildDataType(new SqlDataType(DataType.NChar,    type.Length)); break;
				case DataType.VarChar       : base.BuildDataType(new SqlDataType(DataType.NVarChar, type.Length)); break;
				case DataType.SmallMoney    : StringBuilder.Append("Decimal(10,4)"); break;
				case DataType.DateTime2     :
				case DataType.Time          :
				case DataType.Date          :
				case DataType.SmallDateTime : StringBuilder.Append("DateTime"); break;
				default                     : base.BuildDataType(type);         break;
			}
		}

		protected override void BuildFromClause()
		{
			if (!SelectQuery.IsUpdate)
				base.BuildFromClause();
		}

		protected override void BuildOrderByClause()
		{
			if (SelectQuery.OrderBy.Items.Count == 0 && SelectQuery.Select.SkipValue != null)
			{
				AppendIndent();

				StringBuilder.Append("ORDER BY").AppendLine();

				Indent++;

				AppendIndent();

				BuildExpression(SelectQuery.Select.Columns[0].Expression);
				StringBuilder.AppendLine();

				Indent--;
			}
			else
				base.BuildOrderByClause();
		}

		protected override void BuildColumnExpression(IQueryExpression expr, string alias, ref bool addAlias)
		{
			var wrap = false;

			if (expr.SystemType == typeof(bool))
			{
				if (expr is ISearchCondition)
					wrap = true;
				else
				{
					var ex = expr as ISqlExpression;
					wrap = ex != null && ex.Expr == "{0}" && ex.Parameters.Length == 1 && ex.Parameters[0] is ISearchCondition;
				}
			}

			if (wrap) StringBuilder.Append("CASE WHEN ");
			base.BuildColumnExpression(expr, alias, ref addAlias);
			if (wrap) StringBuilder.Append(" THEN 1 ELSE 0 END");
		}

		public override object Convert(object value, ConvertType convertType)
		{
			switch (convertType)
			{
				case ConvertType.NameToQueryParameter:
				case ConvertType.NameToCommandParameter:
				case ConvertType.NameToSprocParameter:
					return "@" + value;

				case ConvertType.NameToQueryField:
				case ConvertType.NameToQueryFieldAlias:
				case ConvertType.NameToQueryTableAlias:
					{
						var name = value.ToString();

						if (name.Length > 0 && name[0] == '[')
							return value;
					}

					return "[" + value + "]";

				case ConvertType.NameToDatabase:
				case ConvertType.NameToOwner:
				case ConvertType.NameToQueryTable:
					if (value != null)
					{
						var name = value.ToString();

						if (name.Length > 0 && name[0] == '[')
							return value;

						if (name.IndexOf('.') > 0)
							value = string.Join("].[", name.Split('.'));

						return "[" + value + "]";
					}

					break;

				case ConvertType.SprocParameterToName:
					if (value != null)
					{
						var str = value.ToString();
						return str.Length > 0 && str[0] == '@'? str.Substring(1): str;
					}
					break;
			}

			return value;
		}

		protected override void BuildCreateTableIdentityAttribute2(ISqlField field)
		{
			StringBuilder.Append("IDENTITY");
		}

#if !SILVERLIGHT

		protected override string GetProviderTypeName(IDbDataParameter parameter)
		{
			dynamic p = parameter;
			return p.SqlDbType.ToString();
		}

#endif
	}
}
