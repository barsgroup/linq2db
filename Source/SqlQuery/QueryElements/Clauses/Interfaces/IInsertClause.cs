namespace LinqToDB.SqlQuery.QueryElements.Clauses.Interfaces
{
    using System.Collections.Generic;

    using LinqToDB.SqlQuery.QueryElements.Interfaces;
    using LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces;
    using LinqToDB.SqlQuery.Search;

    public interface IInsertClause : IQueryElement,
                                     ISqlExpressionWalkable,
                                     ICloneableElement
    {
        [SearchContainer]
        LinkedList<ISetExpression> Items { get; }

        [SearchContainer]
        ISqlTable Into { get; set; }

        bool WithIdentity { get; set; }
    }
}