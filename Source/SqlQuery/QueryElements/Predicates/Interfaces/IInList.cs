namespace LinqToDB.SqlQuery.QueryElements.Predicates.Interfaces
{
    using System.Collections.Generic;

    using LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces;
    using LinqToDB.SqlQuery.Search;

    public interface IInList: INotExpr
    {
        [SearchContainer]
        List<IQueryExpression> Values { get; }
    }
}