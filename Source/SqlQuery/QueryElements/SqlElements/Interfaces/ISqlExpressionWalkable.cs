namespace LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces
{
    using System;

    public interface ISqlExpressionWalkable
    {
        IQueryExpression Walk(bool skipColumns, Func<IQueryExpression,IQueryExpression> func);
    }
}
