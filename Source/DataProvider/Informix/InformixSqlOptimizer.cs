using System;

namespace LinqToDB.DataProvider.Informix
{
	using Extensions;

	using LinqToDB.SqlQuery.QueryElements.Enums;
	using LinqToDB.SqlQuery.QueryElements.Interfaces;
	using LinqToDB.SqlQuery.QueryElements.SqlElements;
	using LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces;

	using SqlProvider;
	using SqlQuery;

	class InformixSqlOptimizer : BasicSqlOptimizer
	{
		public InformixSqlOptimizer(SqlProviderFlags sqlProviderFlags) : base(sqlProviderFlags)
		{
		}

		public override ISelectQuery Finalize(ISelectQuery selectQuery)
		{
			CheckAliases(selectQuery, int.MaxValue);


		    foreach (var parameter in QueryVisitor.FindOnce<ISqlParameter>(selectQuery.Select))
		    {
                parameter.IsQueryParameter = false;
		    }

			selectQuery = base.Finalize(selectQuery);

			switch (selectQuery.EQueryType)
			{
				case EQueryType.Delete :
					selectQuery = GetAlternativeDelete(selectQuery);
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
					case "%": return new SqlFunction(sqlBinaryExpression.SystemType, "Mod",    sqlBinaryExpression.Expr1, sqlBinaryExpression.Expr2);
					case "&": return new SqlFunction(sqlBinaryExpression.SystemType, "BitAnd", sqlBinaryExpression.Expr1, sqlBinaryExpression.Expr2);
					case "|": return new SqlFunction(sqlBinaryExpression.SystemType, "BitOr",  sqlBinaryExpression.Expr1, sqlBinaryExpression.Expr2);
					case "^": return new SqlFunction(sqlBinaryExpression.SystemType, "BitXor", sqlBinaryExpression.Expr1, sqlBinaryExpression.Expr2);
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
		                case "Coalesce" : return new SqlFunction(sqlFunction.SystemType, "Nvl", sqlFunction.Parameters);
		                case "Convert"  :
		                {
		                    var par0 = sqlFunction.Parameters[0];
		                    var par1 = sqlFunction.Parameters[1];

		                    switch (Type.GetTypeCode(sqlFunction.SystemType.ToUnderlying()))
		                    {
		                        case TypeCode.String   : return new SqlFunction(sqlFunction.SystemType, "To_Char", sqlFunction.Parameters[1]);
		                        case TypeCode.Boolean  :
		                        {
		                            var ex = AlternativeConvertToBoolean(sqlFunction, 1);
		                            if (ex != null)
		                                return ex;
		                            break;
		                        }

		                        case TypeCode.UInt64:
		                            if (sqlFunction.Parameters[1].SystemType.IsFloatType())
		                                par1 = new SqlFunction(sqlFunction.SystemType, "Floor", sqlFunction.Parameters[1]);
		                            break;

		                        case TypeCode.DateTime :
		                            if (IsDateDataType(sqlFunction.Parameters[0], "Date"))
		                            {
		                                if (sqlFunction.Parameters[1].SystemType == typeof(string))
		                                {
		                                    return new SqlFunction(
		                                        sqlFunction.SystemType,
		                                        "Date",
		                                        new SqlFunction(sqlFunction.SystemType, "To_Date", sqlFunction.Parameters[1], new SqlValue("%Y-%m-%d")));
		                                }

		                                return new SqlFunction(sqlFunction.SystemType, "Date", sqlFunction.Parameters[1]);
		                            }

		                            if (IsTimeDataType(sqlFunction.Parameters[0]))
		                                return new SqlExpression(sqlFunction.SystemType, "Cast(Extend({0}, hour to second) as Char(8))", Precedence.Primary, sqlFunction.Parameters[1]);

		                            return new SqlFunction(sqlFunction.SystemType, "To_Date", sqlFunction.Parameters[1]);

		                        default:
		                            if (sqlFunction.SystemType.ToUnderlying() == typeof(DateTimeOffset))
		                                goto case TypeCode.DateTime;
		                            break;
		                    }

		                    return new SqlExpression(sqlFunction.SystemType, "Cast({0} as {1})", Precedence.Primary, par1, par0);
		                }

		                case "Quarter"  : return Inc(Div(Dec(new SqlFunction(sqlFunction.SystemType, "Month", sqlFunction.Parameters)), 3));
		                case "WeekDay"  : return Inc(new SqlFunction(sqlFunction.SystemType, "weekDay", sqlFunction.Parameters));
		                case "DayOfYear":
		                    return
		                        Inc(Sub<int>(
		                            new SqlFunction(null, "Mdy",
		                                new SqlFunction(null, "Month", sqlFunction.Parameters),
		                                new SqlFunction(null, "Day",   sqlFunction.Parameters),
		                                new SqlFunction(null, "Year",  sqlFunction.Parameters)),
		                            new SqlFunction(null, "Mdy",
		                                new SqlValue(1),
		                                new SqlValue(1),
		                                new SqlFunction(null, "Year", sqlFunction.Parameters))));
		                case "Week"     :
		                    return
		                        new SqlExpression(
		                            sqlFunction.SystemType,
		                            "((Extend({0}, year to day) - (Mdy(12, 31 - WeekDay(Mdy(1, 1, year({0}))), Year({0}) - 1) + Interval(1) day to day)) / 7 + Interval(1) day to day)::char(10)::int",
		                            sqlFunction.Parameters);
		                case "Hour"     :
		                case "Minute"   :
		                case "Second"   : return new SqlExpression(sqlFunction.SystemType, string.Format("({{0}}::datetime {0} to {0})::char(3)::int", sqlFunction.Name), sqlFunction.Parameters);
		            }
		        }
		    }

		    return expr;
		}

	}
}
