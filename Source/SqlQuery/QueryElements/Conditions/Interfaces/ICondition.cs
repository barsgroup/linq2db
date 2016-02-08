namespace LinqToDB.SqlQuery.QueryElements.Conditions.Interfaces
{
    using LinqToDB.SqlQuery.QueryElements.Interfaces;
    using LinqToDB.SqlQuery.QueryElements.Predicates;

    public interface ICondition : IQueryElement, ICloneableElement
    {
        bool IsNot { get; set; }

        ISqlPredicate Predicate { get; set; }

        bool IsOr { get; set; }

        int Precedence { get; }

        bool CanBeNull();
    }
}