namespace LinqToDB.SqlQuery.QueryElements.Clauses
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using LinqToDB.SqlQuery.QueryElements.Interfaces;
    using LinqToDB.SqlQuery.QueryElements.SqlElements;
    using LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces;

    public class GroupByClause : ClauseBase, ISqlExpressionWalkable
    {
        internal GroupByClause(ISelectQuery selectQuery) : base(selectQuery)
        {
        }

        internal GroupByClause(
            ISelectQuery selectQuery,
            GroupByClause clone,
            Dictionary<ICloneableElement,ICloneableElement> objectTree,
            Predicate<ICloneableElement> doClone)
            : base(selectQuery)
        {
            _items.AddRange(clone._items.Select(e => (ISqlExpression)e.Clone(objectTree, doClone)));
        }

        internal GroupByClause(IEnumerable<ISqlExpression> items) : base(null)
        {
            _items.AddRange(items);
        }

        public GroupByClause Expr(ISqlExpression expr)
        {
            Add(expr);
            return this;
        }

        public GroupByClause Field(SqlField field)
        {
            return Expr(field);
        }

        void Add(ISqlExpression expr)
        {
            foreach (var e in Items)
                if (e.Equals(expr))
                    return;

            Items.Add(expr);
        }

        readonly List<ISqlExpression> _items = new List<ISqlExpression>();
        public   List<ISqlExpression>  Items => _items;

        public bool IsEmpty => Items.Count == 0;

        #region ISqlExpressionWalkable Members

        ISqlExpression ISqlExpressionWalkable.Walk(bool skipColumns, Func<ISqlExpression,ISqlExpression> func)
        {
            for (var i = 0; i < Items.Count; i++)
                Items[i] = Items[i].Walk(skipColumns, func);

            return null;
        }

        #endregion

        #region IQueryElement Members

        protected override void GetChildrenInternal(List<IQueryElement> list)
        {
            list.AddRange(Items);
        }

        public override QueryElementType ElementType => QueryElementType.GroupByClause;

        public override StringBuilder ToString(StringBuilder sb, Dictionary<IQueryElement,IQueryElement> dic)
        {
            if (Items.Count == 0)
                return sb;

            sb.Append(" \nGROUP BY \n");

            foreach (var item in Items)
            {
                sb.Append('\t');
                item.ToString(sb, dic);
                sb.Append(",");
            }

            sb.Length--;

            return sb;
        }

        #endregion
    }
}