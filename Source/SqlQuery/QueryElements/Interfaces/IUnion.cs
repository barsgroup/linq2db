namespace LinqToDB.SqlQuery.QueryElements.Interfaces
{
    public interface IUnion: IQueryElement
    {
        ISelectQuery SelectQuery { get; }

        bool IsAll { get; }
    }
}