using System;

namespace LinqToDB.DataProvider.SqlServer
{
    using LinqToDB.SqlQuery.QueryElements.Conditions;
    using LinqToDB.SqlQuery.QueryElements.Enums;
    using LinqToDB.SqlQuery.QueryElements.Predicates;
    using LinqToDB.SqlQuery.QueryElements.SqlElements;
    using LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces;

    using SqlProvider;

	class SqlServer2012SqlBuilder : SqlServerSqlBuilder
	{
		public SqlServer2012SqlBuilder(ISqlOptimizer sqlOptimizer, SqlProviderFlags sqlProviderFlags, ValueToSqlConverter valueToSqlConverter)
			: base(sqlOptimizer, sqlProviderFlags, valueToSqlConverter)
		{
		}

		protected override string LimitFormat => SelectQuery.Select.SkipValue != null ? "FETCH NEXT {0} ROWS ONLY" : null;

	    protected override string OffsetFormat => "OFFSET {0} ROWS";

	    protected override bool   OffsetFirst => true;

	    protected override bool   BuildAlternativeSql => false;

	    protected override ISqlBuilder CreateSqlBuilder()
		{
			return new SqlServer2012SqlBuilder(SqlOptimizer, SqlProviderFlags, ValueToSqlConverter);
		}

		protected override void BuildSql()
		{
			if (NeedSkip && SelectQuery.OrderBy.IsEmpty)
			{
				for (var i = 0; i < SelectQuery.Select.Columns.Count; i++)
					SelectQuery.OrderBy.ExprAsc(new SqlValue(i + 1));
			}

			base.BuildSql();
		}

		protected override void BuildInsertOrUpdateQuery()
		{
			BuildInsertOrUpdateQueryAsMerge(null);
			StringBuilder.AppendLine(";");
		}

		public override string  Name => ProviderName.SqlServer2012;

	    protected override void BuildFunction(ISqlFunction func)
		{
			func = ConvertFunctionParameters(func);

			switch (func.Name)
			{
				case "CASE"     :

					if (func.Parameters.Length <= 5)
						func = ConvertCase(func.SystemType, func.Parameters, 0);

					break;

				case "Coalesce" :

					if (func.Parameters.Length > 2)
					{
						var parms = new IQueryExpression[func.Parameters.Length - 1];

						Array.Copy(func.Parameters, 1, parms, 0, parms.Length);
						BuildFunction(new SqlFunction(func.SystemType, func.Name, func.Parameters[0],
						              new SqlFunction(func.SystemType, func.Name, parms)));
						return;
					}

					var sc = new SearchCondition();

					sc.Conditions.AddLast(new Condition(false, new IsNull(func.Parameters[0], false)));

					func = new SqlFunction(func.SystemType, "IIF", sc, func.Parameters[1], func.Parameters[0]);

					break;
			}

			base.BuildFunction(func);
		}

		static ISqlFunction ConvertCase(Type systemType, IQueryExpression[] parameters, int start)
		{
			var len  = parameters.Length - start;
			var name = start == 0 ? "IIF" : "CASE";
			var cond = parameters[start];

			if (start == 0 && SqlExpression.NeedsEqual(cond))
			{
				cond = new SearchCondition(
					new Condition(
						false,
						new ExprExpr(cond, EOperator.Equal, new SqlValue(1))));
			}

			if (len == 3)
				return new SqlFunction(systemType, name, cond, parameters[start + 1], parameters[start + 2]);

			return new SqlFunction(systemType, name,
				cond,
				parameters[start + 1],
				ConvertCase(systemType, parameters, start + 2));
		}
	}
}
