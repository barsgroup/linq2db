namespace LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces
{
    using System;

    using LinqToDB.SqlQuery.QueryElements.Interfaces;

    public interface ISqlExpression : IQueryElement, IEquatable<ISqlExpression>, ISqlExpressionWalkable, ICloneableElement, IOperation
    {
		bool Equals   (ISqlExpression other, Func<ISqlExpression,ISqlExpression,bool> comparer);
	
		Type SystemType { get; }
	}
}
