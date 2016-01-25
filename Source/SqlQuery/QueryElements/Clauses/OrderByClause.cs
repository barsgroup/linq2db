namespace LinqToDB.SqlQuery.QueryElements.Clauses
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using LinqToDB.SqlQuery.QueryElements.Interfaces;
    using LinqToDB.SqlQuery.SqlElements;
    using LinqToDB.SqlQuery.SqlElements.Interfaces;

    public class OrderByClause : ClauseBase, ISqlExpressionWalkable
    {
        internal OrderByClause(SelectQuery selectQuery) : base(selectQuery)
        {
        }

        internal OrderByClause(
            SelectQuery   selectQuery,
            OrderByClause clone,
            Dictionary<ICloneableElement,ICloneableElement> objectTree,
            Predicate<ICloneableElement> doClone)
            : base(selectQuery)
        {
            _items.AddRange(clone._items.Select(item => (OrderByItem)item.Clone(objectTree, doClone)));
        }

        internal OrderByClause(IEnumerable<OrderByItem> items) : base(null)
        {
            _items.AddRange(items);
        }

        public OrderByClause Expr(ISqlExpression expr, bool isDescending)
        {
            Add(expr, isDescending);
            return this;
        }

        public OrderByClause Expr     (ISqlExpression expr)               { return Expr(expr,  false);        }
        public OrderByClause ExprAsc  (ISqlExpression expr)               { return Expr(expr,  false);        }
        public OrderByClause ExprDesc (ISqlExpression expr)               { return Expr(expr,  true);         }
        public OrderByClause Field    (SqlField field, bool isDescending) { return Expr(field, isDescending); }
        public OrderByClause Field    (SqlField field)                    { return Expr(field, false);        }
        public OrderByClause FieldAsc (SqlField field)                    { return Expr(field, false);        }
        public OrderByClause FieldDesc(SqlField field)                    { return Expr(field, true);         }

        void Add(ISqlExpression expr, bool isDescending)
        {
            foreach (var item in Items)
                if (item.Expression.Equals(expr, (x, y) =>
                {
                    var col = x as Column;
                    return col == null || !col.Parent.HasUnion || x == y;
                }))
                    return;

            Items.Add(new OrderByItem(expr, isDescending));
        }

        readonly List<OrderByItem> _items = new List<OrderByItem>();
        public   List<OrderByItem>  Items => _items;

        public bool IsEmpty => Items.Count == 0;


        #region ISqlExpressionWalkable Members

        ISqlExpression ISqlExpressionWalkable.Walk(bool skipColumns, Func<ISqlExpression,ISqlExpression> func)
        {
            foreach (var t in Items)
                t.Walk(skipColumns, func);
            return null;
        }

        #endregion

        #region IQueryElement Members

        protected override IEnumerable<IQueryElement> GetChildItemsInternal()
        {
            return base.GetChildItemsInternal().Union(Items.SelectMany(i => i.GetChildItems()));
        }

        public override QueryElementType ElementType => QueryElementType.OrderByClause;

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