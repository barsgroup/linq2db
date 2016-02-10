namespace LinqToDB.SqlQuery.QueryElements.Interfaces
{
    using System.Collections.Generic;

    using LinqToDB.SqlQuery.QueryElements.SqlElements;
    using LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces;

    public interface IInsertClause : IQueryElement, 
                                     ISqlExpressionWalkable,
                                     ICloneableElement
    {
        List<ISetExpression> Items { get; }

        ISqlTable Into { get; set; }

        bool WithIdentity { get; set; }
    }
}