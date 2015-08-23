namespace LinqToDB.Linq
{
    public interface IExpressionQuery
    {
        string SqlText { get; }

        Query GetQuery();
    }
}
