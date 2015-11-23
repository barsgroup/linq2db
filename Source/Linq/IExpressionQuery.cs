namespace LinqToDB.Linq
{
    using System.Collections;
    using System.Linq.Expressions;

    public interface IExpressionQuery
    {
        string SqlText { get; }

        Query GetQuery();

        IEnumerable Execute(Expression expression);
    }
}
