using System;

namespace LinqToDB.Linq
{
    public interface IExpressionQuery
    {
        string SqlText { get; }
    }
}
