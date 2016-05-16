namespace Bars2Db.SqlQuery.QueryElements.Interfaces
{
    public interface IOperation
    {
        int Precedence { get; }
        bool CanBeNull();
    }
}