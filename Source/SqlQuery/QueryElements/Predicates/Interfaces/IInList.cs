namespace LinqToDB.SqlQuery.QueryElements.Predicates.Interfaces
{
    using System.Collections.Generic;

    using LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces;

    public interface IInList: INotExpr
    {
        List<ISqlExpression> Values { get; }
    }
}