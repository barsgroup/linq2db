namespace LinqToDB.SqlQuery.QueryElements.Conditions
{
    using LinqToDB.SqlQuery.SqlElements.Interfaces;

    public class NextCondition
    {
        internal NextCondition(SearchCondition parent)
        {
            _parent = parent;
        }

        readonly SearchCondition _parent;

        public SearchCondition Or => _parent.SetOr(true);

        public SearchCondition And => _parent.SetOr(false);

        public ISqlExpression  ToExpr() { return _parent; }
    }
}