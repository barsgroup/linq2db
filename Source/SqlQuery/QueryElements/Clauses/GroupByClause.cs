namespace LinqToDB.SqlQuery.QueryElements.Clauses
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using LinqToDB.SqlQuery.QueryElements.Enums;
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
            _items.AddRange(clone._items.Select(e => (IQueryExpression)e.Clone(objectTree, doClone)));
        }

        internal GroupByClause(IEnumerable<IQueryExpression> items) : base(null)
        {
            _items.AddRange(items);
        }

        public GroupByClause Expr(IQueryExpression expr)
        {
            Add(expr);
            return this;
        }

        public GroupByClause Field(ISqlField field)
        {
            return Expr(field);
        }

        void Add(IQueryExpression expr)
        {
            foreach (var e in Items)
                if (e.Equals(expr))
                    return;

            Items.Add(expr);
        }

        readonly List<IQueryExpression> _items = new List<IQueryExpression>();
        public   List<IQueryExpression>  Items => _items;

        public bool IsEmpty => Items.Count == 0;

        #region ISqlExpressionWalkable Members

        IQueryExpression ISqlExpressionWalkable.Walk(bool skipColumns, Func<IQueryExpression,IQueryExpression> func)
        {
            for (var i = 0; i < Items.Count; i++)
                Items[i] = Items[i].Walk(skipColumns, func);

            return null;
        }

        #endregion

        #region IQueryElement Members

        public override void GetChildren(LinkedList<IQueryElement> list)
        {
            FillList(Items, list);
        }

        public override EQueryElementType ElementType => EQueryElementType.GroupByClause;

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