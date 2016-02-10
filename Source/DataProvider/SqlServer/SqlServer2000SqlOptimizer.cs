namespace LinqToDB.DataProvider.SqlServer
{
    using LinqToDB.SqlQuery.QueryElements.SqlElements;
    using LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces;

    using SqlProvider;

    class SqlServer2000SqlOptimizer : SqlServerSqlOptimizer
	{
		public SqlServer2000SqlOptimizer(SqlProviderFlags sqlProviderFlags) : base(sqlProviderFlags)
		{
		}

		public override IQueryExpression ConvertExpression(IQueryExpression expr)
		{
			expr = base.ConvertExpression(expr);

		    var sqlFunction = expr as ISqlFunction;
		    return sqlFunction != null
		               ? ConvertConvertFunction(sqlFunction)
		               : expr;
		}
	}
}
