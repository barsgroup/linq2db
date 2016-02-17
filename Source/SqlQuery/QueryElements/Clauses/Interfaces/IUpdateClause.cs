namespace LinqToDB.SqlQuery.QueryElements.Interfaces
{
    using System.Collections.Generic;

    using LinqToDB.SqlQuery.QueryElements.SqlElements;
    using LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces;

    public interface IUpdateClause : IQueryElement,
                                     ISqlExpressionWalkable,
                                     ICloneableElement
    {
        LinkedList<ISetExpression> Items { get; }

        LinkedList<ISetExpression> Keys { get; }

        ISqlTable Table { get; set; }
    }
}