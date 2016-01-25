namespace LinqToDB.SqlQuery.QueryElements.Clauses
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using LinqToDB.SqlQuery.QueryElements.Interfaces;
    using LinqToDB.SqlQuery.SqlElements;
    using LinqToDB.SqlQuery.SqlElements.Interfaces;

    public class SelectClause : ClauseBase, ISqlExpressionWalkable
    {
        #region Init

        internal SelectClause(SelectQuery selectQuery) : base(selectQuery)
        {
        }

        internal SelectClause(
            SelectQuery  selectQuery,
            SelectClause clone,
            Dictionary<ICloneableElement,ICloneableElement> objectTree,
            Predicate<ICloneableElement> doClone)
            : base(selectQuery)
        {
            _columns.AddRange(clone.Columns.Select(c => (Column)c.Clone(objectTree, doClone)));
            IsDistinct = clone.IsDistinct;
            TakeValue  = (ISqlExpression)clone.TakeValue?.Clone(objectTree, doClone);
            SkipValue  = (ISqlExpression)clone.SkipValue?.Clone(objectTree, doClone);
        }

        internal SelectClause(bool isDistinct, ISqlExpression takeValue, ISqlExpression skipValue, IEnumerable<Column> columns)
            : base(null)
        {
            IsDistinct = isDistinct;
            TakeValue  = takeValue;
            SkipValue  = skipValue;
            _columns.AddRange(columns);
        }

        #endregion

        #region Columns

        //public SelectClause Field(SqlField field)
        //{
        //    AddOrGetColumn(new Column(SelectQuery, field));
        //    return this;
        //}

        //public SelectClause Field(SqlField field, string alias)
        //{
        //    AddOrGetColumn(new Column(SelectQuery, field, alias));
        //    return this;
        //}

        //public SelectClause SubQuery(SelectQuery subQuery)
        //{
        //    if (subQuery.ParentSelect != null && subQuery.ParentSelect != SelectQuery)
        //        throw new ArgumentException("SqlQuery already used as subquery");

        //    subQuery.ParentSelect = SelectQuery;

        //    AddOrGetColumn(new Column(SelectQuery, subQuery));
        //    return this;
        //}

        //public SelectClause SubQuery(SelectQuery selectQuery, string alias)
        //{
        //    if (selectQuery.ParentSelect != null && selectQuery.ParentSelect != SelectQuery)
        //        throw new ArgumentException("SqlQuery already used as subquery");

        //    selectQuery.ParentSelect = SelectQuery;

        //    AddOrGetColumn(new Column(SelectQuery, selectQuery, alias));
        //    return this;
        //}

        public SelectClause Expr(ISqlExpression expr)
        {
            AddOrGetColumn(new Column(SelectQuery, expr));
            return this;
        }

        //public SelectClause Expr(ISqlExpression expr, string alias)
        //{
        //    AddOrGetColumn(new Column(SelectQuery, expr, alias));
        //    return this;
        //}

        //public SelectClause Expr(string expr, params ISqlExpression[] values)
        //{
        //    AddOrGetColumn(new Column(SelectQuery, new SqlExpression(null, expr, values)));
        //    return this;
        //}

        //public SelectClause Expr(Type systemType, string expr, params ISqlExpression[] values)
        //{
        //    AddOrGetColumn(new Column(SelectQuery, new SqlExpression(systemType, expr, values)));
        //    return this;
        //}

        //public SelectClause Expr(string expr, int priority, params ISqlExpression[] values)
        //{
        //    AddOrGetColumn(new Column(SelectQuery, new SqlExpression(null, expr, priority, values)));
        //    return this;
        //}

        //public SelectClause Expr(Type systemType, string expr, int priority, params ISqlExpression[] values)
        //{
        //    AddOrGetColumn(new Column(SelectQuery, new SqlExpression(systemType, expr, priority, values)));
        //    return this;
        //}

        //public SelectClause Expr(string alias, string expr, int priority, params ISqlExpression[] values)
        //{
        //    AddOrGetColumn(new Column(SelectQuery, new SqlExpression(null, expr, priority, values)));
        //    return this;
        //}

        //public SelectClause Expr(Type systemType, string alias, string expr, int priority, params ISqlExpression[] values)
        //{
        //    AddOrGetColumn(new Column(SelectQuery, new SqlExpression(systemType, expr, priority, values)));
        //    return this;
        //}

        //public SelectClause Expr<T>(ISqlExpression expr1, string operation, ISqlExpression expr2)
        //{
        //    AddOrGetColumn(new Column(SelectQuery, new SqlBinaryExpression(typeof(T), expr1, operation, expr2)));
        //    return this;
        //}

        //public SelectClause Expr<T>(ISqlExpression expr1, string operation, ISqlExpression expr2, int priority)
        //{
        //    AddOrGetColumn(new Column(SelectQuery, new SqlBinaryExpression(typeof(T), expr1, operation, expr2, priority)));
        //    return this;
        //}

        //public SelectClause Expr<T>(string alias, ISqlExpression expr1, string operation, ISqlExpression expr2, int priority)
        //{
        //    AddOrGetColumn(new Column(SelectQuery, new SqlBinaryExpression(typeof(T), expr1, operation, expr2, priority), alias));
        //    return this;
        //}

        public int Add(ISqlExpression expr)
        {
            var column = expr as Column;
            if (column != null && column.Parent == SelectQuery)
                throw new InvalidOperationException();

            return _columns.IndexOf(AddOrGetColumn(new Column(SelectQuery, expr)));
        }

        public int Add(ISqlExpression expr, string alias)
        {
            return _columns.IndexOf(AddOrGetColumn(new Column(SelectQuery, expr, alias)));
        }

        Column AddOrGetColumn(Column col)
        {
            if (Columns.All(c => !c.Equals(col)))
            {
                _columns.Add(col);
            }

            return col;
        }

        private readonly List<Column> _columns = new List<Column>();

        public IEnumerable<Column> Columns => _columns;

        #endregion

        #region HasModifier

        public bool HasModifier => IsDistinct || SkipValue != null || TakeValue != null;

        #endregion

        #region Distinct

        public SelectClause Distinct
        {
            get { IsDistinct = true; return this; }
        }

        public bool IsDistinct { get; set; }

        #endregion

        #region Take

        public SelectClause Take(int value)
        {
            TakeValue = new SqlValue(value);
            return this;
        }

        public SelectClause Take(ISqlExpression value)
        {
            TakeValue = value;
            return this;
        }

        public ISqlExpression TakeValue { get; set; }

        #endregion

        #region Skip

        public SelectClause Skip(int value)
        {
            SkipValue = new SqlValue(value);
            return this;
        }

        public SelectClause Skip(ISqlExpression value)
        {
            SkipValue = value;
            return this;
        }

        public ISqlExpression SkipValue { get; set; }

        #endregion

        #region ISqlExpressionWalkable Members

        ISqlExpression ISqlExpressionWalkable.Walk(bool skipColumns, Func<ISqlExpression,ISqlExpression> func)
        {
            for (var i = 0; i < _columns.Count; i++)
            {
                var col  = _columns[i];
                var expr = col.Walk(skipColumns, func);

                var column = expr as Column;
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

        protected override IEnumerable<IQueryElement> GetChildItemsInternal()
        {
            yield return TakeValue;
            yield return SkipValue;

            foreach (var column in Columns)
            {
                yield return column;
            }
        }

        public override QueryElementType ElementType => QueryElementType.SelectClause;

        public override StringBuilder ToString(StringBuilder sb, Dictionary<IQueryElement,IQueryElement> dic)
        {
            if (dic.ContainsKey(this))
                return sb.Append("...");

            dic.Add(this, this);

            sb.Append("SELECT ");

            if (IsDistinct) sb.Append("DISTINCT ");

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
                    ((IQueryElement)c).ToString(sb, dic);
                    sb
                        .Append(" as ")
                        .Append(c.Alias ?? "c" + (_columns.IndexOf(c) + 1))
                        .Append(", \n");
                }

            sb.Length -= 3;

            dic.Remove(this);

            return sb;
        }

        #endregion
    }
}