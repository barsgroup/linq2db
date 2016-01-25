namespace LinqToDB.SqlQuery.SqlElements.Interfaces
{
    using System;

    public interface ISqlExpressionWalkable
	{
		ISqlExpression Walk(bool skipColumns, Func<ISqlExpression,ISqlExpression> func);
	}
}
