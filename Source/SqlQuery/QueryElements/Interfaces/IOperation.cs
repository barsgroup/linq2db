namespace LinqToDB.SqlQuery.QueryElements.Interfaces
{
    public interface IOperation
    {
        bool CanBeNull();

        int Precedence { get; }
    }
}