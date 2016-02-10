using System.Collections.Generic;

namespace LinqToDB.DataProvider.MySql
{
	using Extensions;

	using LinqToDB.SqlQuery.QueryElements.SqlElements;
	using LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces;

	using SqlProvider;
	using SqlQuery;

	class MySqlSqlOptimizer : BasicSqlOptimizer
	{
		public MySqlSqlOptimizer(SqlProviderFlags sqlProviderFlags) : base(sqlProviderFlags)
		{
		}

		public override IQueryExpression ConvertExpression(IQueryExpression expr)
		{
			expr = base.ConvertExpression(expr);

			if (expr is ISqlBinaryExpression)
			{
				var be = (ISqlBinaryExpression)expr;

				switch (be.Operation)
				{
					case "+":
						if (be.SystemType == typeof(string))
						{
							if (be.Expr1 is ISqlFunction)
							{
								var func = (ISqlFunction)be.Expr1;

								if (func.Name == "Concat")
								{
									var list = new List<IQueryExpression>(func.Parameters) { be.Expr2 };
									return new SqlFunction(be.SystemType, "Concat", list.ToArray());
								}
							}
							else if (be.Expr1 is ISqlBinaryExpression && be.Expr1.SystemType == typeof(string) && ((ISqlBinaryExpression)be.Expr1).Operation == "+")
							{
								var list = new List<IQueryExpression> { be.Expr2 };
								var ex   = be.Expr1;

								while (ex is ISqlBinaryExpression && ex.SystemType == typeof(string) && ((ISqlBinaryExpression)be.Expr1).Operation == "+")
								{
									var bex = (ISqlBinaryExpression)ex;

									list.Insert(0, bex.Expr2);
									ex = bex.Expr1;
								}

								list.Insert(0, ex);

								return new SqlFunction(be.SystemType, "Concat", list.ToArray());
							}

							return new SqlFunction(be.SystemType, "Concat", be.Expr1, be.Expr2);
						}

						break;
				}
			}
			else if (expr is ISqlFunction)
			{
				var func = (ISqlFunction) expr;

				switch (func.Name)
				{
					case "Convert" :
						var ftype = func.SystemType.ToUnderlying();

						if (ftype == typeof(bool))
						{
							var ex = AlternativeConvertToBoolean(func, 1);
							if (ex != null)
								return ex;
						}

						if ((ftype == typeof(double) || ftype == typeof(float)) && func.Parameters[1].SystemType.ToUnderlying() == typeof(decimal))
							return func.Parameters[1];

						return new SqlExpression(func.SystemType, "Cast({0} as {1})", Precedence.Primary, FloorBeforeConvert(func), func.Parameters[0]);
				}
			}
			else if (expr is ISqlExpression)
			{
				var e = (ISqlExpression)expr;

				if (e.Expr.StartsWith("Extract(DayOfYear"))
					return new SqlFunction(e.SystemType, "DayOfYear", e.Parameters);

				if (e.Expr.StartsWith("Extract(WeekDay"))
					return Inc(
						new SqlFunction(e.SystemType,
							"WeekDay",
							new SqlFunction(
								null,
								"Date_Add",
								e.Parameters[0],
								new SqlExpression(null, "interval 1 day"))));
			}

			return expr;
		}
	}
}
