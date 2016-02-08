namespace LinqToDB.DataProvider.Access
{
    using LinqToDB.SqlQuery.QueryElements;

    using SqlProvider;
	using SqlQuery;

	class AccessSqlOptimizer : BasicSqlOptimizer
	{
		public AccessSqlOptimizer(SqlProviderFlags sqlProviderFlags) : base(sqlProviderFlags)
		{
		}

		public override ISelectQuery Finalize(ISelectQuery selectQuery)
		{
			selectQuery = base.Finalize(selectQuery);

			switch (selectQuery.QueryType)
			{
				case QueryType.Delete : return GetAlternativeDelete(selectQuery);
				default               : return selectQuery;
			}
		}

		public override bool ConvertCountSubQuery(ISelectQuery subQuery)
		{
			return !subQuery.Where.IsEmpty;
		}
	}
}
