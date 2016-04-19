using System;

namespace LinqToDB.DataProvider.SQLite
{
	using Extensions;

	using LinqToDB.SqlQuery.QueryElements.Enums;
	using LinqToDB.SqlQuery.QueryElements.Interfaces;
	using LinqToDB.SqlQuery.QueryElements.SqlElements;
	using LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces;

	using SqlProvider;
	using SqlQuery;

	class SQLiteSqlOptimizer : BasicSqlOptimizer
	{
		public SQLiteSqlOptimizer(SqlProviderFlags sqlProviderFlags)
			: base(sqlProviderFlags)
		{
		}

		public override ISelectQuery Finalize(ISelectQuery selectQuery)
		{
			selectQuery = base.Finalize(selectQuery);

			switch (selectQuery.EQueryType)
			{
				case EQueryType.Delete :
					selectQuery = GetAlternativeDelete(base.Finalize(selectQuery));
					selectQuery.From.Tables.First.Value.Alias = "$";
					break;

				case EQueryType.Update :
					selectQuery = GetAlternativeUpdate(selectQuery);
					break;
			}

			return selectQuery;
		}

		public override IQueryExpression ConvertExpression(IQueryExpression expr)
		{
			expr = base.ConvertExpression(expr);

		    var sqlBinaryExpression = expr as ISqlBinaryExpression;
		    if (sqlBinaryExpression != null)
			{
				switch (sqlBinaryExpression.Operation)
				{
					case "+": return sqlBinaryExpression.SystemType == typeof(string)? new SqlBinaryExpression(sqlBinaryExpression.SystemType, sqlBinaryExpression.Expr1, "||", sqlBinaryExpression.Expr2, sqlBinaryExpression.Precedence): expr;
					case "^": // (a + b) - (a & b) * 2
						return Sub(
							Add(sqlBinaryExpression.Expr1, sqlBinaryExpression.Expr2, sqlBinaryExpression.SystemType),
							Mul(new SqlBinaryExpression(sqlBinaryExpression.SystemType, sqlBinaryExpression.Expr1, "&", sqlBinaryExpression.Expr2), 2), sqlBinaryExpression.SystemType);
				}
			}
			else
		    {
		        var sqlFunction = expr as ISqlFunction;
		        if (sqlFunction != null)
		        {
		            switch (sqlFunction.Name)
		            {
		                case "Space"   : return new SqlFunction(sqlFunction.SystemType, "PadR", new SqlValue(" "), sqlFunction.Parameters[0]);
		                case "Convert" :
		                {
		                    var ftype = sqlFunction.SystemType.ToUnderlying();

		                    if (ftype == typeof(bool))
		                    {
		                        var ex = AlternativeConvertToBoolean(sqlFunction, 1);
		                        if (ex != null)
		                            return ex;
		                    }

		                    if (ftype == typeof(DateTime) || ftype == typeof(DateTimeOffset))
		                    {
		                        if (IsDateDataType(sqlFunction.Parameters[0], "Date"))
		                            return new SqlFunction(sqlFunction.SystemType, "Date", sqlFunction.Parameters[1]);
		                        return new SqlFunction(sqlFunction.SystemType, "DateTime", sqlFunction.Parameters[1]);
		                    }

		                    return new SqlExpression(sqlFunction.SystemType, "Cast({0} as {1})", Precedence.Primary, sqlFunction.Parameters[1], sqlFunction.Parameters[0]);
		                }
		            }
		        }
		        else
		        {
		            var sqlExpression = expr as ISqlExpression;
		            if (sqlExpression != null)
		            {
		                if (sqlExpression.Expr.StartsWith("Cast(StrFTime(Quarter"))
		                    return Inc(Div(Dec(new SqlExpression(sqlExpression.SystemType, sqlExpression.Expr.Replace("Cast(StrFTime(Quarter", "Cast(StrFTime('%m'"), sqlExpression.Parameters)), 3));

		                if (sqlExpression.Expr.StartsWith("Cast(StrFTime('%w'"))
		                    return Inc(new SqlExpression(sqlExpression.SystemType, sqlExpression.Expr.Replace("Cast(StrFTime('%w'", "Cast(strFTime('%w'"), sqlExpression.Parameters));

		                if (sqlExpression.Expr.StartsWith("Cast(StrFTime('%f'"))
		                    return new SqlExpression(sqlExpression.SystemType, "Cast(strFTime('%f', {0}) * 1000 as int) % 1000", Precedence.Multiplicative, sqlExpression.Parameters);

		                if (sqlExpression.Expr.StartsWith("DateTime"))
		                {
		                    if (sqlExpression.Expr.EndsWith("Quarter')"))
		                        return new SqlExpression(sqlExpression.SystemType, "DateTime({1}, '{0} Month')", Precedence.Primary, Mul(sqlExpression.Parameters[0], 3), sqlExpression.Parameters[1]);

		                    if (sqlExpression.Expr.EndsWith("Week')"))
		                        return new SqlExpression(sqlExpression.SystemType, "DateTime({1}, '{0} Day')",   Precedence.Primary, Mul(sqlExpression.Parameters[0], 7), sqlExpression.Parameters[1]);
		                }
		            }
		        }
		    }

		    return expr;
		}
	}
}
