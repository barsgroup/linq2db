namespace LinqToDB.SqlQuery.QueryElements.Conditions.Interfaces
{
    using LinqToDB.SqlQuery.QueryElements.Interfaces;
    using LinqToDB.SqlQuery.QueryElements.Predicates;
    using LinqToDB.SqlQuery.QueryElements.Predicates.Interfaces;
    using LinqToDB.SqlQuery.Search;

    public interface ICondition : IQueryElement, ICloneableElement
    {
        bool IsNot { get; set; }

        [SearchContainer]
        ISqlPredicate Predicate { get; set; }

        bool IsOr { get; set; }

        int Precedence { get; }

        bool CanBeNull();
    }
}