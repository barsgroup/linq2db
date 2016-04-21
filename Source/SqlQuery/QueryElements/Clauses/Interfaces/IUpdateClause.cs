namespace LinqToDB.SqlQuery.QueryElements.Clauses.Interfaces
{
    using System.Collections.Generic;

    using LinqToDB.SqlQuery.QueryElements.Interfaces;
    using LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces;
    using LinqToDB.SqlQuery.Search;

    public interface IUpdateClause : IQueryElement,
                                     ISqlExpressionWalkable,
                                     ICloneableElement
    {
        [SearchContainer]
        LinkedList<ISetExpression> Items { get; }

        [SearchContainer]
        LinkedList<ISetExpression> Keys { get; }

        [SearchContainer]
        ISqlTable Table { get; set; }
    }
}