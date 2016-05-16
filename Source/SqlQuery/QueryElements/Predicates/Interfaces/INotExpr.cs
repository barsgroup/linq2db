namespace Bars2Db.SqlQuery.QueryElements.Predicates.Interfaces
{
    public interface INotExpr : IExpr
    {
        bool IsNot { get; set; }
    }
}