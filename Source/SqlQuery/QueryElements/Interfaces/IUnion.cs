namespace LinqToDB.SqlQuery.QueryElements.Interfaces
{
    using LinqToDB.SqlQuery.Search;

    public interface IUnion: IQueryElement
    {
        [SearchContainer]
        ISelectQuery SelectQuery { get; }

        bool IsAll { get; }
    }
}