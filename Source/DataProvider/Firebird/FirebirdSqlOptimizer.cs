namespace LinqToDB.DataProvider.Firebird
{
    using System.Linq;

    using Extensions;

    using LinqToDB.SqlEntities;
    using LinqToDB.SqlQuery.QueryElements.Enums;
    using LinqToDB.SqlQuery.QueryElements.Interfaces;
    using LinqToDB.SqlQuery.QueryElements.SqlElements;
    using LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces;

    using SqlProvider;
	using SqlQuery;

	class FirebirdSqlOptimizer : BasicSqlOptimizer
	{
		public FirebirdSqlOptimizer(SqlProviderFlags sqlProviderFlags) : base(sqlProviderFlags)
		{
		}

		
		private bool SearchSelectClause(IQueryElement element)
		{
			if (element.ElementType != EQueryElementType.SelectClause) return true;

			QueryVisitor.FindParentFirst(element, SetNonQueryParameterInSelectClause);

			return false;
		}

		private bool SetNonQueryParameterInSelectClause(IQueryElement element)
		{
			if (element.ElementType == EQueryElementType.SqlParameter)
			{
				((ISqlParameter)element).IsQueryParameter = false;
				return false;
			}

			if (element.ElementType == EQueryElementType.SqlQuery)
			{
				QueryVisitor.FindParentFirst(element, SearchSelectClause);
				return false;
			}

			return true;
		}

		public override ISelectQuery Finalize(ISelectQuery selectQuery)
		{
			CheckAliases(selectQuery, int.MaxValue);

			QueryVisitor.FindParentFirst(selectQuery, SearchSelectClause);

			if (selectQuery.EQueryType == EQueryType.InsertOrUpdate)
			{
			    var param = selectQuery.Insert.Items
                    .Union(selectQuery.Update.Items)
                    .Union(selectQuery.Update.Keys)
                    .Select(i => i.Expression)
                    .ToArray();

			    foreach (var parameter in QueryVisitor.FindOnce<ISqlParameter>(param))
			    {
                    parameter.IsQueryParameter = false;
                }
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
					case "%": return new SqlFunction(sqlBinaryExpression.SystemType, "Mod",     sqlBinaryExpression.Expr1, sqlBinaryExpression.Expr2);
					case "&": return new SqlFunction(sqlBinaryExpression.SystemType, "Bin_And", sqlBinaryExpression.Expr1, sqlBinaryExpression.Expr2);
					case "|": return new SqlFunction(sqlBinaryExpression.SystemType, "Bin_Or",  sqlBinaryExpression.Expr1, sqlBinaryExpression.Expr2);
					case "^": return new SqlFunction(sqlBinaryExpression.SystemType, "Bin_Xor", sqlBinaryExpression.Expr1, sqlBinaryExpression.Expr2);
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
		                case "Convert" :
		                    if (sqlFunction.SystemType.ToUnderlying() == typeof(bool))
		                    {
		                        IQueryExpression ex = AlternativeConvertToBoolean(sqlFunction, 1);
		                        if (ex != null)
		                            return ex;
		                    }

		                    return new SqlExpression(sqlFunction.SystemType, "Cast({0} as {1})", Precedence.Primary, FloorBeforeConvert(sqlFunction), sqlFunction.Parameters[0]);

		                case "DateAdd" :
		                    switch ((Sql.DateParts)((ISqlValue)sqlFunction.Parameters[0]).Value)
		                    {
		                        case Sql.DateParts.Quarter  :
		                            return new SqlFunction(sqlFunction.SystemType, sqlFunction.Name, new SqlValue(Sql.DateParts.Month), Mul(sqlFunction.Parameters[1], 3), sqlFunction.Parameters[2]);
		                        case Sql.DateParts.DayOfYear:
		                        case Sql.DateParts.WeekDay:
		                            return new SqlFunction(sqlFunction.SystemType, sqlFunction.Name, new SqlValue(Sql.DateParts.Day),   sqlFunction.Parameters[1],         sqlFunction.Parameters[2]);
		                        case Sql.DateParts.Week     :
		                            return new SqlFunction(sqlFunction.SystemType, sqlFunction.Name, new SqlValue(Sql.DateParts.Day),   Mul(sqlFunction.Parameters[1], 7), sqlFunction.Parameters[2]);
		                    }

		                    break;
		            }
		        }
		        else
		        {
		            var sqlExpression = expr as ISqlExpression;
		            if (sqlExpression != null)
		            {
		                if (sqlExpression.Expr.StartsWith("Extract(Quarter"))
		                    return Inc(Div(Dec(new SqlExpression(sqlExpression.SystemType, "Extract(Month from {0})", sqlExpression.Parameters)), 3));

		                if (sqlExpression.Expr.StartsWith("Extract(YearDay"))
		                    return Inc(new SqlExpression(sqlExpression.SystemType, sqlExpression.Expr.Replace("Extract(YearDay", "Extract(yearDay"), sqlExpression.Parameters));

		                if (sqlExpression.Expr.StartsWith("Extract(WeekDay"))
		                    return Inc(new SqlExpression(sqlExpression.SystemType, sqlExpression.Expr.Replace("Extract(WeekDay", "Extract(weekDay"), sqlExpression.Parameters));
		            }
		        }
		    }

		    return expr;
		}

	}
}
