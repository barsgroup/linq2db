namespace LinqToDB.SqlQuery.QueryElements.Conditions.Interfaces
{
    using LinqToDB.SqlQuery.QueryElements.Interfaces;
    using LinqToDB.SqlQuery.Search;

    public interface IHaveSearchCondition: IQueryElement
    {
        [SearchContainer]
        ISearchCondition Search { get; }
        bool IsEmpty { get; }
    }
}