namespace LinqToDB.SqlQuery.QueryElements.Clauses
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using LinqToDB.SqlQuery.QueryElements.Clauses.Interfaces;
    using LinqToDB.SqlQuery.QueryElements.Enums;
    using LinqToDB.SqlQuery.QueryElements.Interfaces;
    using LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces;

    public class OrderByClause : ClauseBase,
                                 IOrderByClause
    {
        internal OrderByClause(ISelectQuery selectQuery) : base(selectQuery)
        {
        }

        internal OrderByClause(
            ISelectQuery selectQuery,
            IOrderByClause clone,
            Dictionary<ICloneableElement,ICloneableElement> objectTree,
            Predicate<ICloneableElement> doClone)
            : base(selectQuery)
        {
            _items.AddRange(clone.Items.Select(item => (IOrderByItem)item.Clone(objectTree, doClone)));
        }

        internal OrderByClause(IEnumerable<IOrderByItem> items) : base(null)
        {
            _items.AddRange(items);
        }

        public IOrderByClause Expr(IQueryExpression expr, bool isDescending)
        {
            Add(expr, isDescending);
            return this;
        }

        public IOrderByClause ExprAsc  (IQueryExpression expr)               { return Expr(expr,  false);        }

        void Add(IQueryExpression expr, bool isDescending)
        {
            foreach (var item in Items)
                if (item.Expression.Equals(expr, (x, y) =>
                {
                    var col = x as IColumn;
                    return col == null || !col.Parent.HasUnion || x == y;
                }))
                    return;

            Items.Add(new OrderByItem(expr, isDescending));
        }

        readonly List<IOrderByItem> _items = new List<IOrderByItem>();

        public List<IOrderByItem>  Items => _items;

        public bool IsEmpty => Items.Count == 0;


        #region ISqlExpressionWalkable Members

        IQueryExpression ISqlExpressionWalkable.Walk(bool skipColumns, Func<IQueryExpression,IQueryExpression> func)
        {
            foreach (var t in Items)
                t.Walk(skipColumns, func);
            return null;
        }

        #endregion

        #region IQueryElement Members

        public override EQueryElementType ElementType => EQueryElementType.OrderByClause;

        public override StringBuilder ToString(StringBuilder sb, Dictionary<IQueryElement,IQueryElement> dic)
        {
            if (Items.Count == 0)
                return sb;

            sb.Append(" \nORDER BY \n");

            foreach (IQueryElement item in Items)
            {
                sb.Append('\t');
                item.ToString(sb, dic);
                sb.Append(", ");
            }

            sb.Length -= 2;

            return sb;
        }

        #endregion
    }
}