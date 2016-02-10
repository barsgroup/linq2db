namespace LinqToDB.SqlQuery.QueryElements.Predicates.Interfaces
{
    using LinqToDB.SqlQuery.QueryElements.SqlElements;
    using LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces;

    public interface IFuncLike: ISqlPredicate
    {
        ISqlFunction Function { get; }
       
    }
}