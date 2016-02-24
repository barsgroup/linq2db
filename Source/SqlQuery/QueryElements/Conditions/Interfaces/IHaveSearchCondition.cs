namespace LinqToDB.SqlQuery.QueryElements.Conditions.Interfaces
{
    public interface IHaveSearchCondition
    {
        ISearchCondition Search { get; }
        bool IsEmpty { get; }
    }
}