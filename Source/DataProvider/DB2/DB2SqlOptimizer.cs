namespace LinqToDB.DataProvider.DB2
{
	using Extensions;

	using LinqToDB.SqlEntities;
	using LinqToDB.SqlQuery.QueryElements.Enums;
	using LinqToDB.SqlQuery.QueryElements.Interfaces;
	using LinqToDB.SqlQuery.QueryElements.SqlElements;
	using LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces;

	using SqlProvider;
	using SqlQuery;

	class Db2SqlOptimizer : BasicSqlOptimizer
	{
		public Db2SqlOptimizer(SqlProviderFlags sqlProviderFlags) : base(sqlProviderFlags)
		{
		}

		public override ISelectQuery Finalize(ISelectQuery selectQuery)
		{
		    foreach (var parameter in QueryVisitor.FindOnce<ISqlParameter>(selectQuery.Select) )
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
					case "%":
						{
							var expr1 = !sqlBinaryExpression.Expr1.SystemType.IsIntegerType() ? new SqlFunction(typeof(int), "Int", sqlBinaryExpression.Expr1) : sqlBinaryExpression.Expr1;
							return new SqlFunction(sqlBinaryExpression.SystemType, "Mod", expr1, sqlBinaryExpression.Expr2);
						}
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
		                case "Convert"    :
		                    if (sqlFunction.SystemType.ToUnderlying() == typeof(bool))
		                    {
		                        var ex = AlternativeConvertToBoolean(sqlFunction, 1);
		                        if (ex != null)
		                            return ex;
		                    }

		                    var sqlDataType = sqlFunction.Parameters[0] as ISqlDataType;
		                    if (sqlDataType != null)
		                    {
		                        if (sqlDataType.Type == typeof(string) && sqlFunction.Parameters[1].SystemType != typeof(string))
		                            return new SqlFunction(sqlFunction.SystemType, "RTrim", new SqlFunction(typeof(string), "Char", sqlFunction.Parameters[1]));

		                        if (sqlDataType.Length > 0)
		                            return new SqlFunction(sqlFunction.SystemType, sqlDataType.DataType.ToString(), sqlFunction.Parameters[1], new SqlValue(sqlDataType.Length));

		                        if (sqlDataType.Precision > 0)
		                            return new SqlFunction(sqlFunction.SystemType, sqlDataType.DataType.ToString(), sqlFunction.Parameters[1], new SqlValue(sqlDataType.Precision), new SqlValue(sqlDataType.Scale));

		                        return new SqlFunction(sqlFunction.SystemType, sqlDataType.DataType.ToString(), sqlFunction.Parameters[1]);
		                    }

		                    var function = sqlFunction.Parameters[0] as ISqlFunction;
		                    if (function != null)
		                    {
		                        return
		                            function.Name == "Char" ?
		                                new SqlFunction(sqlFunction.SystemType, function.Name, sqlFunction.Parameters[1]) :
		                                function.Parameters.Length == 1 ?
		                                    new SqlFunction(sqlFunction.SystemType, function.Name, sqlFunction.Parameters[1], function.Parameters[0]) :
		                                    new SqlFunction(sqlFunction.SystemType, function.Name, sqlFunction.Parameters[1], function.Parameters[0], function.Parameters[1]);
		                    }

		                {
		                    var e = (ISqlExpression)sqlFunction.Parameters[0];
		                    return new SqlFunction(sqlFunction.SystemType, e.Expr, sqlFunction.Parameters[1]);
		                }

		                case "Millisecond"   : return Div(new SqlFunction(sqlFunction.SystemType, "Microsecond", sqlFunction.Parameters), 1000);
		                case "SmallDateTime" :
		                case "DateTime"      :
		                case "DateTime2"     : return new SqlFunction(sqlFunction.SystemType, "TimeStamp", sqlFunction.Parameters);
		                case "UInt16"        : return new SqlFunction(sqlFunction.SystemType, "Int",       sqlFunction.Parameters);
		                case "UInt32"        : return new SqlFunction(sqlFunction.SystemType, "BigInt",    sqlFunction.Parameters);
		                case "UInt64"        : return new SqlFunction(sqlFunction.SystemType, "Decimal",   sqlFunction.Parameters);
		                case "Byte"          :
		                case "SByte"         :
		                case "Int16"         : return new SqlFunction(sqlFunction.SystemType, "SmallInt",  sqlFunction.Parameters);
		                case "Int32"         : return new SqlFunction(sqlFunction.SystemType, "Int",       sqlFunction.Parameters);
		                case "Int64"         : return new SqlFunction(sqlFunction.SystemType, "BigInt",    sqlFunction.Parameters);
		                case "Double"        : return new SqlFunction(sqlFunction.SystemType, "Float",     sqlFunction.Parameters);
		                case "Single"        : return new SqlFunction(sqlFunction.SystemType, "Real",      sqlFunction.Parameters);
		                case "Money"         : return new SqlFunction(sqlFunction.SystemType, "Decimal",   sqlFunction.Parameters[0], new SqlValue(19), new SqlValue(4));
		                case "SmallMoney"    : return new SqlFunction(sqlFunction.SystemType, "Decimal",   sqlFunction.Parameters[0], new SqlValue(10), new SqlValue(4));
		                case "VarChar"       :
		                    if (sqlFunction.Parameters[0].SystemType.ToUnderlying() == typeof(decimal))
		                        return new SqlFunction(sqlFunction.SystemType, "Char", sqlFunction.Parameters[0]);
		                    break;

		                case "NChar"         :
		                case "NVarChar"      : return new SqlFunction(sqlFunction.SystemType, "Char",      sqlFunction.Parameters);
		                case "DateDiff"      :
		                    switch ((Sql.DateParts)((ISqlValue)sqlFunction.Parameters[0]).Value)
		                    {
		                        case Sql.DateParts.Day         : return new SqlExpression(typeof(int), "((Days({0}) - Days({1})) * 86400 + (MIDNIGHT_SECONDS({0}) - MIDNIGHT_SECONDS({1}))) / 86400",                                               Precedence.Multiplicative, sqlFunction.Parameters[2], sqlFunction.Parameters[1]);
		                        case Sql.DateParts.Hour        : return new SqlExpression(typeof(int), "((Days({0}) - Days({1})) * 86400 + (MIDNIGHT_SECONDS({0}) - MIDNIGHT_SECONDS({1}))) / 3600",                                                Precedence.Multiplicative, sqlFunction.Parameters[2], sqlFunction.Parameters[1]);
		                        case Sql.DateParts.Minute      : return new SqlExpression(typeof(int), "((Days({0}) - Days({1})) * 86400 + (MIDNIGHT_SECONDS({0}) - MIDNIGHT_SECONDS({1}))) / 60",                                                  Precedence.Multiplicative, sqlFunction.Parameters[2], sqlFunction.Parameters[1]);
		                        case Sql.DateParts.Second      : return new SqlExpression(typeof(int), "(Days({0}) - Days({1})) * 86400 + (MIDNIGHT_SECONDS({0}) - MIDNIGHT_SECONDS({1}))",                                                         Precedence.Additive,       sqlFunction.Parameters[2], sqlFunction.Parameters[1]);
		                        case Sql.DateParts.Millisecond : return new SqlExpression(typeof(int), "((Days({0}) - Days({1})) * 86400 + (MIDNIGHT_SECONDS({0}) - MIDNIGHT_SECONDS({1}))) * 1000 + (MICROSECOND({0}) - MICROSECOND({1})) / 1000", Precedence.Additive,       sqlFunction.Parameters[2], sqlFunction.Parameters[1]);
		                    }

		                    break;
		            }
		        }
		    }

		    return expr;
		}
	}
}
