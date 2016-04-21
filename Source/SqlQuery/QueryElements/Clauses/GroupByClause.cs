namespace LinqToDB.SqlQuery.QueryElements.Clauses
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    using LinqToDB.Extensions;
    using LinqToDB.SqlQuery.QueryElements.Clauses.Interfaces;
    using LinqToDB.SqlQuery.QueryElements.Enums;
    using LinqToDB.SqlQuery.QueryElements.Interfaces;
    using LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces;

    public class GroupByClause : ClauseBase,
                                 IGroupByClause
    {
        internal GroupByClause(ISelectQuery selectQuery) : base(selectQuery)
        {
        }

        internal GroupByClause(ISelectQuery selectQuery, IGroupByClause clone, Dictionary<ICloneableElement, ICloneableElement> objectTree, Predicate<ICloneableElement> doClone)
            : base(selectQuery)
        {

            clone.Items.ForEach(
                node =>
                {
                    var value = (IQueryExpression)node.Value.Clone(objectTree, doClone);
                    Items.AddLast(value);
                });
        }

        internal GroupByClause(LinkedList<IQueryExpression> items) : base(null)
        {
            items.ForEach(node => Items.AddLast(node.Value));
        }

        public IGroupByClause Expr(IQueryExpression expr)
        {
            Add(expr);
            return this;
        }

        void Add(IQueryExpression expr)
        {
            foreach (var e in Items)
                if (e.Equals(expr))
                    return;

            Items.AddLast(expr);
        }

        public LinkedList<IQueryExpression> Items { get; } = new LinkedList<IQueryExpression>();

        public bool IsEmpty => Items.Count == 0;

        #region ISqlExpressionWalkable Members

        IQueryExpression ISqlExpressionWalkable.Walk(bool skipColumns, Func<IQueryExpression,IQueryExpression> func)
        {
            Items.ForEach(
                node =>
                {
                    node.Value = node.Value.Walk(skipColumns, func);
                });

            return null;
        }

        #endregion

        #region IQueryElement Members

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