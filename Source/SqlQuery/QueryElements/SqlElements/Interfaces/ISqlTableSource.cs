namespace LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces
{
    using System.Collections.Generic;

    using LinqToDB.SqlQuery.QueryElements.SqlElements.Enums;

    public interface ISqlTableSource : IQueryExpression
    {
        ISqlField All { get; }

        int SourceID { get; }

        ESqlTableType SqlTableType { get; set; }

        IList<IQueryExpression> GetKeys(bool allIfEmpty);
    }
}
