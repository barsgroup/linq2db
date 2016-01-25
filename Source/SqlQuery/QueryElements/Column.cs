namespace LinqToDB.SqlQuery.QueryElements
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    using LinqToDB.SqlQuery.QueryElements.Interfaces;
    using LinqToDB.SqlQuery.SqlElements;
    using LinqToDB.SqlQuery.SqlElements.Interfaces;

    public class Column : BaseQueryElement, IEquatable<Column>, ISqlExpression
    {
        public Column(SelectQuery parent, ISqlExpression expression, string alias)
        {
            if (expression == null) throw new ArgumentNullException("expression");

            Parent     = parent;
            Expression = expression;
            _alias     = alias;

#if DEBUG
            _columnNumber = ++_columnCounter;
#endif
        }

        public Column(SelectQuery builder, ISqlExpression expression)
            : this(builder, expression, null)
        {
        }

#if DEBUG
        readonly int _columnNumber;
        static   int _columnCounter;
#endif

        public ISqlExpression Expression { get; set; }
        public SelectQuery    Parent     { get; set; }

        internal string _alias;
        public   string  Alias
        {
            get
            {
                if (_alias == null)
                {
                    if (Expression is SqlField)
                    {
                        var field = (SqlField)Expression;
                        return field.Alias ?? field.PhysicalName;
                    }

                    if (Expression is Column)
                    {
                        var col = (Column)Expression;
                        return col.Alias;
                    }
                }

                return _alias;
            }
            set { _alias = value; }
        }

        public bool Equals(Column other)
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

        public bool Equals(ISqlExpression other, Func<ISqlExpression,ISqlExpression,bool> comparer)
        {
            if (this == other)
                return true;

            return
                other is Column &&
                Expression.Equals(((Column)other).Expression, comparer) &&
                comparer(this, other);
        }

        public int Precedence => SqlQuery.Precedence.Primary;

        public Type SystemType => Expression.SystemType;

        public ICloneableElement Clone(Dictionary<ICloneableElement, ICloneableElement> objectTree, Predicate<ICloneableElement> doClone)
        {
            if (!doClone(this))
                return this;

            ICloneableElement clone;

            var parent = (SelectQuery)Parent.Clone(objectTree, doClone);

            if (!objectTree.TryGetValue(this, out clone))
                objectTree.Add(this, clone = new Column(
                                                 parent,
                                                 (ISqlExpression)Expression.Clone(objectTree, doClone),
                                                 _alias));

            return clone;
        }

        #endregion

        #region IEquatable<ISqlExpression> Members

        bool IEquatable<ISqlExpression>.Equals(ISqlExpression other)
        {
            if (this == other)
                return true;

            return other is Column && Equals((Column)other);
        }

        #endregion

        #region ISqlExpressionWalkable Members

        public ISqlExpression Walk(bool skipColumns, Func<ISqlExpression,ISqlExpression> func)
        {
            if (!(skipColumns && Expression is Column))
                Expression = Expression.Walk(skipColumns, func);

            return func(this);
        }

        #endregion

        #region IQueryElement Members

        protected override IEnumerable<IQueryElement> GetChildItemsInternal()
        {
            yield return Expression;
        }

        public sealed override QueryElementType ElementType => QueryElementType.Column;

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

            if (Expression is SelectQuery)
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