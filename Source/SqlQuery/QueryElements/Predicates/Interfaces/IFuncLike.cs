namespace LinqToDB.SqlQuery.QueryElements.Predicates.Interfaces
{
    using LinqToDB.SqlQuery.QueryElements.SqlElements;

    public interface IFuncLike: ISqlPredicate
    {
        SqlFunction Function { get; }
       
    }
}