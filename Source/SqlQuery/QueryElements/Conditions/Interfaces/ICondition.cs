using Bars2Db.SqlQuery.QueryElements.Interfaces;
using Bars2Db.SqlQuery.QueryElements.Predicates.Interfaces;
using Bars2Db.SqlQuery.Search;

namespace Bars2Db.SqlQuery.QueryElements.Conditions.Interfaces
{
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