namespace LinqToDB.SqlQuery.QueryElements.Predicates.Interfaces
{
    public interface INotExpr: IExpr
    {
        bool IsNot { get; set; }
    }
}