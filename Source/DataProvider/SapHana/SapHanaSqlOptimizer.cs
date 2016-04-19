namespace LinqToDB.DataProvider.SapHana
{
	using Extensions;

	using LinqToDB.SqlQuery.QueryElements.Enums;
	using LinqToDB.SqlQuery.QueryElements.Interfaces;
	using LinqToDB.SqlQuery.QueryElements.SqlElements;
	using LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces;

	using SqlProvider;
	using SqlQuery;

	class SapHanaSqlOptimizer : BasicSqlOptimizer
	{
		public SapHanaSqlOptimizer(SqlProviderFlags sqlProviderFlags) : base(sqlProviderFlags)
		{

		}

		public override ISelectQuery Finalize(ISelectQuery selectQuery)
		{
			selectQuery = base.Finalize(selectQuery);

			switch (selectQuery.EQueryType)
			{
				case EQueryType.Delete:
					selectQuery = GetAlternativeDelete(selectQuery);
					break;
				case EQueryType.Update:
					selectQuery = GetAlternativeUpdate(selectQuery);
					break;
			}

			return selectQuery;
		}

		public override IQueryExpression ConvertExpression(IQueryExpression expr)
		{
			expr = base.ConvertExpression(expr);

		    var function = expr as ISqlFunction;
		    if (function != null)
			{
			    if (function.Name == "Convert")
				{
					var ftype = function.SystemType.ToUnderlying();

					if (ftype == typeof(bool))
					{
						var ex = AlternativeConvertToBoolean(function, 1);
						if (ex != null)
							return ex;
					}
					return new SqlExpression(function.SystemType, "Cast({0} as {1})", Precedence.Primary, FloorBeforeConvert(function), function.Parameters[0]);
				}
			}
			else
			{
			    var sqlBinaryExpression = expr as ISqlBinaryExpression;
			    if (sqlBinaryExpression != null)
			    {
			        switch (sqlBinaryExpression.Operation)
			        {
			            case "%":
			                return new SqlFunction(sqlBinaryExpression.SystemType, "MOD", sqlBinaryExpression.Expr1, sqlBinaryExpression.Expr2);
			            case "&": 
			                return new SqlFunction(sqlBinaryExpression.SystemType, "BITAND", sqlBinaryExpression.Expr1, sqlBinaryExpression.Expr2);
			            case "|":
			                return Sub(
			                    Add(sqlBinaryExpression.Expr1, sqlBinaryExpression.Expr2, sqlBinaryExpression.SystemType),
			                    new SqlFunction(sqlBinaryExpression.SystemType, "BITAND", sqlBinaryExpression.Expr1, sqlBinaryExpression.Expr2),
			                    sqlBinaryExpression.SystemType);
			            case "^": // (a + b) - BITAND(a, b) * 2
			                return Sub(
			                    Add(sqlBinaryExpression.Expr1, sqlBinaryExpression.Expr2, sqlBinaryExpression.SystemType),
			                    Mul(new SqlFunction(sqlBinaryExpression.SystemType, "BITAND", sqlBinaryExpression.Expr1, sqlBinaryExpression.Expr2), 2),
			                    sqlBinaryExpression.SystemType);
			            case "+": 
			                return sqlBinaryExpression.SystemType == typeof(string) ? 
			                           new SqlBinaryExpression(sqlBinaryExpression.SystemType, sqlBinaryExpression.Expr1, "||", sqlBinaryExpression.Expr2, sqlBinaryExpression.Precedence) : 
			                           expr;
			        }
			    }
			}

		    return expr;
		}
	}
}
