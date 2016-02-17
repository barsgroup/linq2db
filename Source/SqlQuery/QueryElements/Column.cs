namespace LinqToDB.SqlQuery.QueryElements
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    using LinqToDB.SqlQuery.QueryElements.Enums;
    using LinqToDB.SqlQuery.QueryElements.Interfaces;
    using LinqToDB.SqlQuery.QueryElements.SqlElements;
    using LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces;

    using Seterlund.CodeGuard;

    public class Column : BaseQueryElement,
                          IColumn
    {
        public Column(ISelectQuery parent, IQueryExpression expression, string alias)
        {
            if (expression == null) throw new ArgumentNullException(nameof(expression));

            Parent     = parent;
            Expression = expression;
            _alias     = alias;

#if DEBUG
            _columnNumber = ++_columnCounter;
#endif
        }

        public Column(ISelectQuery builder, IQueryExpression expression)
            : this(builder, expression, null)
        {
        }

#if DEBUG
        readonly int _columnNumber;
        static   int _columnCounter;
#endif

        public IQueryExpression Expression { get; set; }
        public ISelectQuery Parent     { get; set; }

        internal string _alias;
        public   string  Alias
        {
            get
            {
                if (_alias == null)
                {
                    var sqlField = Expression as ISqlField;
                    if (sqlField != null)
                    {
                        return sqlField.Alias ?? sqlField.PhysicalName;
                    }

                    var column = Expression as IColumn;
                    if (column != null)
                    {
                        return column.Alias;
                    }
                }

                return _alias;
            }
            set { _alias = value; }
        }

        public bool Equals(IColumn other)
        {
            return Expression.Equals(other.Expression) && Equals(Parent, other.Parent);
        }

#if OVERRIDETOSTRING

			public override string ToString()
			{
				return ((IQueryElement)this).ToString(new StringBuilder(), new Dictionary<IQueryElement,IQueryElement>()).ToString();
			}

#endif

        #region ISqlExpression Members

        public bool CanBeNull()
        {
            return Expression.CanBeNull();
        }

        public bool Equals(IQueryExpression other, Func<IQueryExpression,IQueryExpression,bool> comparer)
        {
            if (this == other)
                return true;

            var column = other as IColumn;
            return
                column != null &&
                Expression.Equals(column.Expression, comparer) &&
                comparer(this, column);
        }

        public int Precedence => SqlQuery.Precedence.Primary;

        public Type SystemType => Expression.SystemType;

        public ICloneableElement Clone(Dictionary<ICloneableElement, ICloneableElement> objectTree, Predicate<ICloneableElement> doClone)
        {
            if (!doClone(this))
                return this;

            ICloneableElement clone;

            var parent = (ISelectQuery)Parent.Clone(objectTree, doClone);

            if (!objectTree.TryGetValue(this, out clone))
                objectTree.Add(this, clone = new Column(
                                                 parent,
                                                 (IQueryExpression)Expression.Clone(objectTree, doClone),
                                                 _alias));

            return clone;
        }

        #endregion

        #region IEquatable<ISqlExpression> Members

        bool IEquatable<IQueryExpression>.Equals(IQueryExpression other)
        {
            if (this == other)
                return true;

            var column = other as IColumn;
            return column != null && Equals(column);
        }

        #endregion

        #region ISqlExpressionWalkable Members

        public IQueryExpression Walk(bool skipColumns, Func<IQueryExpression,IQueryExpression> func)
        {
            if (!(skipColumns && Expression is IColumn))
                Expression = Expression.Walk(skipColumns, func);

            return func(this);
        }

        #endregion

        #region IQueryElement Members

        public override void GetChildren(LinkedList<IQueryElement> list)
        {
            list.AddLast(Expression);
        }

        public sealed override EQueryElementType ElementType => EQueryElementType.Column;

        public sealed override StringBuilder ToString(StringBuilder sb, Dictionary<IQueryElement,IQueryElement> dic)
        {
            if (dic.ContainsKey(this))
                return sb.Append("...");

            dic.Add(this, this);

            sb
                .Append('t')
                .Append(Parent.SourceID)
                .Append(".");

#if DEBUG
            sb.Append('[').Append(_columnNumber).Append(']');
#endif

            if (Expression is ISelectQuery)
            {
                sb
                    .Append("(\n\t\t");
                var len = sb.Length;
                Expression.ToString(sb, dic).Replace("\n", "\n\t\t", len, sb.Length - len);
                sb.Append("\n\t)");
            }
            else
            {
                Expression.ToString(sb, dic);
            }

            dic.Remove(this);

            return sb;
        }

        #endregion
    }
}