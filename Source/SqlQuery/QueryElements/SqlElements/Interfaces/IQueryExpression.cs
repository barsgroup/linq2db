namespace LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces
{
    using System;

    using LinqToDB.SqlQuery.QueryElements.Interfaces;

    public interface IQueryExpression : IQueryElement, IEquatable<IQueryExpression>, ISqlExpressionWalkable, ICloneableElement, IOperation
    {
		bool Equals   (IQueryExpression other, Func<IQueryExpression,IQueryExpression,bool> comparer);
	
		Type SystemType { get; }

    }
}
