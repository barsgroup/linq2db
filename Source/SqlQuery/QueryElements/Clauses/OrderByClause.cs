using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bars2Db.SqlQuery.QueryElements.Clauses.Interfaces;
using Bars2Db.SqlQuery.QueryElements.Enums;
using Bars2Db.SqlQuery.QueryElements.Interfaces;
using Bars2Db.SqlQuery.QueryElements.SqlElements.Interfaces;

namespace Bars2Db.SqlQuery.QueryElements.Clauses
{
    public class OrderByClause : ClauseBase,
        IOrderByClause
    {
        internal OrderByClause(ISelectQuery selectQuery) : base(selectQuery)
        {
        }

        internal OrderByClause(
            ISelectQuery selectQuery,
            IOrderByClause clone,
            Dictionary<ICloneableElement, ICloneableElement> objectTree,
            Predicate<ICloneableElement> doClone)
            : base(selectQuery)
        {
            Items.AddRange(clone.Items.Select(item => (IOrderByItem) item.Clone(objectTree, doClone)));
        }

        internal OrderByClause(IEnumerable<IOrderByItem> items) : base(null)
        {
            Items.AddRange(items);
        }

        public IOrderByClause Expr(IQueryExpression expr, bool isDescending)
        {
            Add(expr, isDescending);
            return this;
        }

        public IOrderByClause ExprAsc(IQueryExpression expr)
        {
            return Expr(expr, false);
        }

        public List<IOrderByItem> Items { get; } = new List<IOrderByItem>();

        public bool IsEmpty => Items.Count == 0;

        #region ISqlExpressionWalkable Members

        IQueryExpression ISqlExpressionWalkable.Walk(bool skipColumns, Func<IQueryExpression, IQueryExpression> func)
        {
            foreach (var t in Items)
                t.Walk(skipColumns, func);
            return null;
        }

        #endregion

        private void Add(IQueryExpression expr, bool isDescending)
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

        #region IQueryElement Members

        public override EQueryElementType ElementType => EQueryElementType.OrderByClause;

        public override StringBuilder ToString(StringBuilder sb, Dictionary<IQueryElement, IQueryElement> dic)
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