namespace LinqToDB.DataProvider.Access
{
    using LinqToDB.SqlQuery.QueryElements.Enums;
    using LinqToDB.SqlQuery.QueryElements.Interfaces;

    using SqlProvider;

    class AccessSqlOptimizer : BasicSqlOptimizer
    {
        public AccessSqlOptimizer(SqlProviderFlags sqlProviderFlags) : base(sqlProviderFlags)
        {
        }

        public override ISelectQuery Finalize(ISelectQuery selectQuery)
        {
            selectQuery = base.Finalize(selectQuery);

            switch (selectQuery.EQueryType)
            {
                case EQueryType.Delete : return GetAlternativeDelete(selectQuery);
                default               : return selectQuery;
            }
        }

        public override bool ConvertCountSubQuery(ISelectQuery subQuery)
        {
            return !subQuery.Where.IsEmpty;
        }
    }
}
