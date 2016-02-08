namespace LinqToDB.SqlQuery.QueryElements.Interfaces
{
    using System.Collections.Generic;

    using LinqToDB.SqlQuery.QueryElements.SqlElements;
    using LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces;

    public interface IUpdateClause : IQueryElement,
                                     ISqlExpressionWalkable,
                                     ICloneableElement
    {
        List<ISetExpression> Items { get; }

        List<ISetExpression> Keys { get; }

        SqlTable Table { get; set; }
    }
}