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

		    var sqlBinaryExpression = expr as ISqlBinaryExpression;
		    if (sqlBinaryExpression != null)
			{
				switch (sqlBinaryExpression.Operation)
				{
					case "+":
						if (sqlBinaryExpression.SystemType == typeof(string))
						{
						    var sqlFunction = sqlBinaryExpression.Expr1 as ISqlFunction;
						    if (sqlFunction != null)
							{
								if (sqlFunction.Name == "Concat")
								{
									var list = new List<IQueryExpression>(sqlFunction.Parameters) { sqlBinaryExpression.Expr2 };
									return new SqlFunction(sqlBinaryExpression.SystemType, "Concat", list.ToArray());
								}
							}
							else
						    {
						        var binaryExpression = sqlBinaryExpression.Expr1 as ISqlBinaryExpression;
						        if (binaryExpression != null && binaryExpression.SystemType == typeof(string) && binaryExpression.Operation == "+")
						        {
						            var list = new List<IQueryExpression> { sqlBinaryExpression.Expr2 };
						            var ex   = sqlBinaryExpression.Expr1;

                                    ISqlBinaryExpression expression;
						            while ((expression = ex as ISqlBinaryExpression) != null && expression.SystemType == typeof(string) && binaryExpression.Operation == "+")
						            {
						                list.Insert(0, expression.Expr2);
						                ex = expression.Expr1;
						            }

						            list.Insert(0, ex);

						            return new SqlFunction(sqlBinaryExpression.SystemType, "Concat", list.ToArray());
						        }
						    }

						    return new SqlFunction(sqlBinaryExpression.SystemType, "Concat", sqlBinaryExpression.Expr1, sqlBinaryExpression.Expr2);
						}

						break;
				}
			}
			else
		    {
		        var sqlFunction = expr as ISqlFunction;
		        if (sqlFunction != null)
		        {
		            if (sqlFunction.Name == "Convert")
		            {
		                var ftype = sqlFunction.SystemType.ToUnderlying();

		                if (ftype == typeof(bool))
		                {
		                    var ex = AlternativeConvertToBoolean(sqlFunction, 1);
		                    if (ex != null)
		                    {
		                        return ex;
		                    }
		                }

		                if ((ftype == typeof(double) || ftype == typeof(float)) && sqlFunction.Parameters[1].SystemType.ToUnderlying() == typeof(decimal))
		                {
		                    return sqlFunction.Parameters[1];
		                }

		                return new SqlExpression(sqlFunction.SystemType, "Cast({0} as {1})", Precedence.Primary, FloorBeforeConvert(sqlFunction), sqlFunction.Parameters[0]);
		            }
		        }
		        else
		        {
		            var sqlExpression = expr as ISqlExpression;
		            if (sqlExpression != null)
		            {
		                if (sqlExpression.Expr.StartsWith("Extract(DayOfYear"))
		                    return new SqlFunction(sqlExpression.SystemType, "DayOfYear", sqlExpression.Parameters);

		                if (sqlExpression.Expr.StartsWith("Extract(WeekDay"))
		                    return Inc(
		                        new SqlFunction(sqlExpression.SystemType,
		                            "WeekDay",
		                            new SqlFunction(
		                                null,
		                                "Date_Add",
		                                sqlExpression.Parameters[0],
		                                new SqlExpression(null, "interval 1 day"))));
		            }
		        }
		    }

		    return expr;
		}
	}
}
