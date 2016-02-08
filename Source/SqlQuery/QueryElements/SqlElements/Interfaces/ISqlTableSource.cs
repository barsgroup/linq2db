namespace LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces
{
    using System.Collections.Generic;

    public interface ISqlTableSource : ISqlExpression
	{
		SqlField              All          { get; set; }
		int                   SourceID     { get; }
		SqlTableType          SqlTableType { get; }
		IList<ISqlExpression> GetKeys(bool allIfEmpty);
	}
}
