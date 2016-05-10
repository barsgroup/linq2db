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
    public class SelectClause : ClauseBase,
        ISelectClause
    {
        #region HasModifier

        public bool HasModifier => IsDistinct || SkipValue != null || TakeValue != null;

        #endregion

        public bool IsDistinct { get; set; }

        #region Take

        public IQueryExpression TakeValue { get; set; }

        #endregion

        #region Skip

        public IQueryExpression SkipValue { get; set; }

        #endregion

        #region ISqlExpressionWalkable Members

        IQueryExpression ISqlExpressionWalkable.Walk(bool skipColumns, Func<IQueryExpression, IQueryExpression> func)
        {
            for (var i = 0; i < Columns.Count; i++)
            {
                var col = Columns[i];
                var expr = col.Walk(skipColumns, func);

                var column = expr as IColumn;
                if (column != null)
                    Columns[i] = column;
                else
                    Columns[i] = new Column(col.Parent, expr, col.Alias);
            }

            TakeValue = TakeValue?.Walk(skipColumns, func);
            SkipValue = SkipValue?.Walk(skipColumns, func);

            return null;
        }

        #endregion

        #region Init

        internal SelectClause(ISelectQuery selectQuery) : base(selectQuery)
        {
        }

        internal SelectClause(ISelectQuery selectQuery, ISelectClause clone,
            Dictionary<ICloneableElement, ICloneableElement> objectTree, Predicate<ICloneableElement> doClone)
            : base(selectQuery)
        {
            Columns.AddRange(clone.Columns.Select(c => (IColumn) c.Clone(objectTree, doClone)));
            IsDistinct = clone.IsDistinct;
            TakeValue = (IQueryExpression) clone.TakeValue?.Clone(objectTree, doClone);
            SkipValue = (IQueryExpression) clone.SkipValue?.Clone(objectTree, doClone);
        }

        internal SelectClause(bool isDistinct, IQueryExpression takeValue, IQueryExpression skipValue,
            IEnumerable<IColumn> columns) : base(null)
        {
            IsDistinct = isDistinct;
            TakeValue = takeValue;
            SkipValue = skipValue;
            Columns.AddRange(columns);
        }

        #endregion

        #region Columns

        public void Expr(IQueryExpression expr)
        {
            AddOrGetColumn(new Column(SelectQuery, expr));
        }

        public int Add(IQueryExpression expr)
        {
            var column = expr as Column;
            if (column != null && column.Parent == SelectQuery)
                throw new InvalidOperationException();

            return Columns.IndexOf(AddOrGetColumn(new Column(SelectQuery, expr)));
        }

        public int Add(IQueryExpression expr, string alias)
        {
            return Columns.IndexOf(AddOrGetColumn(new Column(SelectQuery, expr, alias)));
        }

        private IColumn AddOrGetColumn(IColumn col)
        {
            if (Columns.All(c => !c.Equals(col)))
            {
                Columns.Add(col);
            }

            return col;
        }

        public List<IColumn> Columns { get; } = new List<IColumn>();

        #endregion

        #region IQueryElement Members

        public override EQueryElementType ElementType => EQueryElementType.SelectClause;

        public override StringBuilder ToString(StringBuilder sb, Dictionary<IQueryElement, IQueryElement> dic)
        {
            if (dic.ContainsKey(this))
                return sb.Append("...");

            dic.Add(this, this);

            sb.Append("SELECT ");

            if (IsDistinct)
                sb.Append("DISTINCT ");

            if (SkipValue != null)
            {
                sb.Append("SKIP ");
                SkipValue.ToString(sb, dic);
                sb.Append(" ");
            }

            if (TakeValue != null)
            {
                sb.Append("TAKE ");
                TakeValue.ToString(sb, dic);
                sb.Append(" ");
            }

            sb.AppendLine();

            if (Columns.Count == 0)
                sb.Append("\t*, \n");
            else
                foreach (var c in Columns)
                {
                    sb.Append("\t");
                    c.ToString(sb, dic);
                    sb.Append(" as ").Append(c.Alias ?? "c" + (Columns.IndexOf(c) + 1)).Append(", \n");
                }

            sb.Length -= 3;

            dic.Remove(this);

            return sb;
        }

        #endregion
    }
}