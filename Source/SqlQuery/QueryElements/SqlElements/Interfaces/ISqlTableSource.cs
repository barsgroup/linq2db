namespace LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces
{
    using System.Collections.Generic;

    using LinqToDB.SqlQuery.QueryElements.SqlElements.Enums;
    using LinqToDB.SqlQuery.Search;

    public interface ISqlTableSource : IQueryExpression
	{
        [SearchContainer]
        ISqlField All { get; set; }

		int                   SourceID     { get; }

		ESqlTableType          SqlTableType { get; set; }

        IList<IQueryExpression> GetKeys(bool allIfEmpty);
	}
}
