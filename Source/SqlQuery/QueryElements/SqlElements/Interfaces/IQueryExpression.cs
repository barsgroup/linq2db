using System;
using Bars2Db.SqlQuery.QueryElements.Interfaces;

namespace Bars2Db.SqlQuery.QueryElements.SqlElements.Interfaces
{
    public interface IQueryExpression : IQueryElement, IEquatable<IQueryExpression>, ISqlExpressionWalkable,
        ICloneableElement, IOperation
    {
        Type SystemType { get; }
        bool Equals(IQueryExpression other, Func<IQueryExpression, IQueryExpression, bool> comparer);
    }
}