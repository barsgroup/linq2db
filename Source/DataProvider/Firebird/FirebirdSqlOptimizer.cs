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

			new QueryVisitor().VisitParentFirst(element, SetNonQueryParameterInSelectClause);

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
				new QueryVisitor().VisitParentFirst(element, SearchSelectClause);
				return false;
			}

			return true;
		}

		public override ISelectQuery Finalize(ISelectQuery selectQuery)
		{
			CheckAliases(selectQuery, int.MaxValue);

			new QueryVisitor().VisitParentFirst(selectQuery, SearchSelectClause);

			if (selectQuery.EQueryType == EQueryType.InsertOrUpdate)
			{
			    var param = selectQuery.Insert.Items.Union(selectQuery.Update.Items).Union(selectQuery.Update.Keys).Select(i => i.Expression).ToArray();

			    foreach (var parameter in QueryVisitor.FindAll<ISqlParameter>(param))
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

			if (expr is ISqlBinaryExpression)
			{
				var be = (ISqlBinaryExpression)expr;

				switch (be.Operation)
				{
					case "%": return new SqlFunction(be.SystemType, "Mod",     be.Expr1, be.Expr2);
					case "&": return new SqlFunction(be.SystemType, "Bin_And", be.Expr1, be.Expr2);
					case "|": return new SqlFunction(be.SystemType, "Bin_Or",  be.Expr1, be.Expr2);
					case "^": return new SqlFunction(be.SystemType, "Bin_Xor", be.Expr1, be.Expr2);
					case "+": return be.SystemType == typeof(string)? new SqlBinaryExpression(be.SystemType, be.Expr1, "||", be.Expr2, be.Precedence): expr;
				}
			}
			else if (expr is ISqlFunction)
			{
                ISqlFunction func = (ISqlFunction)expr;

				switch (func.Name)
				{
					case "Convert" :
						if (func.SystemType.ToUnderlying() == typeof(bool))
						{
							IQueryExpression ex = AlternativeConvertToBoolean(func, 1);
							if (ex != null)
								return ex;
						}

						return new SqlExpression(func.SystemType, "Cast({0} as {1})", Precedence.Primary, FloorBeforeConvert(func), func.Parameters[0]);

					case "DateAdd" :
						switch ((Sql.DateParts)((ISqlValue)func.Parameters[0]).Value)
						{
							case Sql.DateParts.Quarter  :
								return new SqlFunction(func.SystemType, func.Name, new SqlValue(Sql.DateParts.Month), Mul(func.Parameters[1], 3), func.Parameters[2]);
							case Sql.DateParts.DayOfYear:
							case Sql.DateParts.WeekDay:
								return new SqlFunction(func.SystemType, func.Name, new SqlValue(Sql.DateParts.Day),   func.Parameters[1],         func.Parameters[2]);
							case Sql.DateParts.Week     :
								return new SqlFunction(func.SystemType, func.Name, new SqlValue(Sql.DateParts.Day),   Mul(func.Parameters[1], 7), func.Parameters[2]);
						}

						break;
				}
			}
			else if (expr is ISqlExpression)
			{
				var e = (ISqlExpression)expr;

				if (e.Expr.StartsWith("Extract(Quarter"))
					return Inc(Div(Dec(new SqlExpression(e.SystemType, "Extract(Month from {0})", e.Parameters)), 3));

				if (e.Expr.StartsWith("Extract(YearDay"))
					return Inc(new SqlExpression(e.SystemType, e.Expr.Replace("Extract(YearDay", "Extract(yearDay"), e.Parameters));

				if (e.Expr.StartsWith("Extract(WeekDay"))
					return Inc(new SqlExpression(e.SystemType, e.Expr.Replace("Extract(WeekDay", "Extract(weekDay"), e.Parameters));
			}

			return expr;
		}

	}
}
