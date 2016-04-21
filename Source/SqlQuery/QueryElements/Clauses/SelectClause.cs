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

    public class SelectClause : ClauseBase,
                                ISelectClause
    {
        #region Init

        internal SelectClause(ISelectQuery selectQuery) : base(selectQuery)
        {
        }

        internal SelectClause(ISelectQuery selectQuery, ISelectClause clone, Dictionary<ICloneableElement, ICloneableElement> objectTree, Predicate<ICloneableElement> doClone)
            : base(selectQuery)
        {
            _columns.AddRange(clone.Columns.Select(c => (IColumn)c.Clone(objectTree, doClone)));
            IsDistinct = clone.IsDistinct;
            TakeValue = (IQueryExpression)clone.TakeValue?.Clone(objectTree, doClone);
            SkipValue = (IQueryExpression)clone.SkipValue?.Clone(objectTree, doClone);
        }

        internal SelectClause(bool isDistinct, IQueryExpression takeValue, IQueryExpression skipValue, IEnumerable<IColumn> columns) : base(null)
        {
            IsDistinct = isDistinct;
            TakeValue = takeValue;
            SkipValue = skipValue;
            _columns.AddRange(columns);
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

            return _columns.IndexOf(AddOrGetColumn(new Column(SelectQuery, expr)));
        }

        public int Add(IQueryExpression expr, string alias)
        {
            return _columns.IndexOf(AddOrGetColumn(new Column(SelectQuery, expr, alias)));
        }

        IColumn AddOrGetColumn(IColumn col)
        {
            if (_columns.All(c => !c.Equals(col)))
            {
                _columns.Add(col);
            }

            return col;
        }

        private readonly List<IColumn> _columns = new List<IColumn>();

        public List<IColumn> Columns => _columns;

        #endregion

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
            for (var i = 0; i < _columns.Count; i++)
            {
                var col = _columns[i];
                var expr = col.Walk(skipColumns, func);

                var column = expr as IColumn;
                if (column != null)
                    _columns[i] = column;
                else
                    _columns[i] = new Column(col.Parent, expr, col.Alias);
            }

            TakeValue = TakeValue?.Walk(skipColumns, func);
            SkipValue = SkipValue?.Walk(skipColumns, func);

            return null;
        }

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

            if (_columns.Count == 0)
                sb.Append("\t*, \n");
            else
                foreach (var c in Columns)
                {
                    sb.Append("\t");
                    c.ToString(sb, dic);
                    sb.Append(" as ").Append(c.Alias ?? "c" + (_columns.IndexOf(c) + 1)).Append(", \n");
                }

            sb.Length -= 3;

            dic.Remove(this);

            return sb;
        }

        #endregion
    }
}