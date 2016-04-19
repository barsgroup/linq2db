namespace LinqToDB.DataProvider.Sybase
{
    using LinqToDB.SqlQuery.QueryElements.SqlElements;
    using LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces;

    using SqlProvider;

    class SybaseSqlOptimizer : BasicSqlOptimizer
	{
		public SybaseSqlOptimizer(SqlProviderFlags sqlProviderFlags) : base(sqlProviderFlags)
		{
		}

		public override IQueryExpression ConvertExpression(IQueryExpression expr)
		{
			expr = base.ConvertExpression(expr);

		    var sqlFunction = expr as ISqlFunction;
		    if (sqlFunction != null)
			{
				switch (sqlFunction.Name)
				{
					case "CharIndex" :
						if (sqlFunction.Parameters.Length == 3)
							return Add<int>(
								ConvertExpression(new SqlFunction(sqlFunction.SystemType, "CharIndex",
									sqlFunction.Parameters[0],
									ConvertExpression(new SqlFunction(typeof(string), "Substring",
										sqlFunction.Parameters[1],
										sqlFunction.Parameters[2], new SqlFunction(typeof(int), "Len", sqlFunction.Parameters[1]))))),
								Sub(sqlFunction.Parameters[2], 1));
						break;

					case "Stuff"     :
				        var sqlValue = sqlFunction.Parameters[3] as ISqlValue;
				        if (sqlValue?.Value is string && string.IsNullOrEmpty((string)sqlValue.Value))
				            return new SqlFunction(
				                sqlFunction.SystemType,
				                sqlFunction.Name,
				                sqlFunction.Precedence,
				                sqlFunction.Parameters[0],
				                sqlFunction.Parameters[1],
				                sqlFunction.Parameters[1],
				                new SqlValue(null));

				        break;
				}
			}

			return expr;
		}
	}
}
