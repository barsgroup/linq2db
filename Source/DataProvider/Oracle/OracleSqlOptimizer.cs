using System;

namespace LinqToDB.DataProvider.Oracle
{
	using Extensions;

	using LinqToDB.SqlQuery.QueryElements.Enums;
	using LinqToDB.SqlQuery.QueryElements.Interfaces;
	using LinqToDB.SqlQuery.QueryElements.SqlElements;
	using LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces;

	using SqlProvider;
	using SqlQuery;

	public class OracleSqlOptimizer : BasicSqlOptimizer
	{
		public OracleSqlOptimizer(SqlProviderFlags sqlProviderFlags) : base(sqlProviderFlags)
		{
		}

		public override ISelectQuery Finalize(ISelectQuery selectQuery)
		{
			CheckAliases(selectQuery, 30);

		    foreach (var parameter in QueryVisitor.FindOnce<ISqlParameter>(selectQuery.Select))
		    {
		        parameter.IsQueryParameter = false;
		    }

		    selectQuery = base.Finalize(selectQuery);

			switch (selectQuery.EQueryType)
			{
				case EQueryType.Delete : return GetAlternativeDelete(selectQuery);
				case EQueryType.Update : return GetAlternativeUpdate(selectQuery);
				default               : return selectQuery;
			}
		}

		public override IQueryExpression ConvertExpression(IQueryExpression expr)
		{
			expr = base.ConvertExpression(expr);

		    var sqlBinaryExpression = expr as ISqlBinaryExpression;
		    if (sqlBinaryExpression != null)
			{
				switch (sqlBinaryExpression.Operation)
				{
					case "%": return new SqlFunction(sqlBinaryExpression.SystemType, "MOD",    sqlBinaryExpression.Expr1, sqlBinaryExpression.Expr2);
					case "&": return new SqlFunction(sqlBinaryExpression.SystemType, "BITAND", sqlBinaryExpression.Expr1, sqlBinaryExpression.Expr2);
					case "|": // (a + b) - BITAND(a, b)
						return Sub(
							Add(sqlBinaryExpression.Expr1, sqlBinaryExpression.Expr2, sqlBinaryExpression.SystemType),
							new SqlFunction(sqlBinaryExpression.SystemType, "BITAND", sqlBinaryExpression.Expr1, sqlBinaryExpression.Expr2),
							sqlBinaryExpression.SystemType);

					case "^": // (a + b) - BITAND(a, b) * 2
						return Sub(
							Add(sqlBinaryExpression.Expr1, sqlBinaryExpression.Expr2, sqlBinaryExpression.SystemType),
							Mul(new SqlFunction(sqlBinaryExpression.SystemType, "BITAND", sqlBinaryExpression.Expr1, sqlBinaryExpression.Expr2), 2),
							sqlBinaryExpression.SystemType);
					case "+": return sqlBinaryExpression.SystemType == typeof(string)? new SqlBinaryExpression(sqlBinaryExpression.SystemType, sqlBinaryExpression.Expr1, "||", sqlBinaryExpression.Expr2, sqlBinaryExpression.Precedence): expr;
				}
			}
			else
		    {
		        var sqlFunction = expr as ISqlFunction;
		        if (sqlFunction != null)
		        {
		            switch (sqlFunction.Name)
		            {
		                case "Coalesce"       : return new SqlFunction(sqlFunction.SystemType, "Nvl", sqlFunction.Parameters);
		                case "Convert"        :
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
		                        if (IsTimeDataType(sqlFunction.Parameters[0]))
		                        {
		                            if (sqlFunction.Parameters[1].SystemType == typeof(string))
		                                return sqlFunction.Parameters[1];

		                            return new SqlFunction(sqlFunction.SystemType, "To_Char", sqlFunction.Parameters[1], new SqlValue("HH24:MI:SS"));
		                        }

		                        if (sqlFunction.Parameters[1].SystemType.ToUnderlying() == typeof(DateTime) &&
		                            IsDateDataType(sqlFunction.Parameters[0], "Date"))
		                        {
		                            return new SqlFunction(sqlFunction.SystemType, "Trunc", sqlFunction.Parameters[1], new SqlValue("DD"));
		                        }

		                        return new SqlFunction(sqlFunction.SystemType, "To_Timestamp", sqlFunction.Parameters[1], new SqlValue("YYYY-MM-DD HH24:MI:SS"));
		                    }

		                    return new SqlExpression(sqlFunction.SystemType, "Cast({0} as {1})", Precedence.Primary, FloorBeforeConvert(sqlFunction), sqlFunction.Parameters[0]);
		                }

		                case "CharIndex"      :
		                    return sqlFunction.Parameters.Length == 2?
		                               new SqlFunction(sqlFunction.SystemType, "InStr", sqlFunction.Parameters[1], sqlFunction.Parameters[0]):
		                               new SqlFunction(sqlFunction.SystemType, "InStr", sqlFunction.Parameters[1], sqlFunction.Parameters[0], sqlFunction.Parameters[2]);
		                case "AddYear"        : return new SqlFunction(sqlFunction.SystemType, "Add_Months", sqlFunction.Parameters[0], Mul(sqlFunction.Parameters[1], 12));
		                case "AddQuarter"     : return new SqlFunction(sqlFunction.SystemType, "Add_Months", sqlFunction.Parameters[0], Mul(sqlFunction.Parameters[1],  3));
		                case "AddMonth"       : return new SqlFunction(sqlFunction.SystemType, "Add_Months", sqlFunction.Parameters[0],     sqlFunction.Parameters[1]);
		                case "AddDayOfYear"   :
		                case "AddWeekDay"     :
		                case "AddDay"         : return Add<DateTime>(sqlFunction.Parameters[0],     sqlFunction.Parameters[1]);
		                case "AddWeek"        : return Add<DateTime>(sqlFunction.Parameters[0], Mul(sqlFunction.Parameters[1], 7));
		                case "AddHour"        : return Add<DateTime>(sqlFunction.Parameters[0], Div(sqlFunction.Parameters[1],                  24));
		                case "AddMinute"      : return Add<DateTime>(sqlFunction.Parameters[0], Div(sqlFunction.Parameters[1],             60 * 24));
		                case "AddSecond"      : return Add<DateTime>(sqlFunction.Parameters[0], Div(sqlFunction.Parameters[1],        60 * 60 * 24));
		                case "AddMillisecond" : return Add<DateTime>(sqlFunction.Parameters[0], Div(sqlFunction.Parameters[1], 1000 * 60 * 60 * 24));
		                case "Avg"            : 
		                    return new SqlFunction(
		                        sqlFunction.SystemType,
		                        "Round",
		                        new SqlFunction(sqlFunction.SystemType, "AVG", sqlFunction.Parameters[0]),
		                        new SqlValue(27));
		            }
		        }
		        else
		        {
		            var sqlExpression = expr as ISqlExpression;
		            if (sqlExpression != null)
		            {
		                if (sqlExpression.Expr.StartsWith("To_Number(To_Char(") && sqlExpression.Expr.EndsWith(", 'FF'))"))
		                    return Div(new SqlExpression(sqlExpression.SystemType, sqlExpression.Expr.Replace("To_Number(To_Char(", "to_Number(To_Char("), sqlExpression.Parameters), 1000);
		            }
		        }
		    }

		    return expr;
		}

	}
}
