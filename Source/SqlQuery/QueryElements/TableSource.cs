namespace LinqToDB.SqlQuery.QueryElements
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using LinqToDB.SqlQuery.QueryElements.Interfaces;
    using LinqToDB.SqlQuery.SqlElements;
    using LinqToDB.SqlQuery.SqlElements.Interfaces;

    public class TableSource : BaseQueryElement, ISqlTableSource
    {
        public TableSource(ISqlTableSource source, string alias)
            : this(source, alias, null)
        {
        }

        public TableSource(ISqlTableSource source, string alias, params JoinedTable[] joins)
        {
            if (source == null) throw new ArgumentNullException("source");

            Source = source;
            _alias = alias;

            if (joins != null)
                _joins.AddRange(joins);
        }

        public TableSource(ISqlTableSource source, string alias, IEnumerable<JoinedTable> joins)
        {
            if (source == null) throw new ArgumentNullException("source");

            Source = source;
            _alias = alias;

            if (joins != null)
                _joins.AddRange(joins);
        }

        public ISqlTableSource Source       { get; set; }
        public SqlTableType    SqlTableType => Source.SqlTableType;

        // TODO: remove internal.
        internal string _alias;
        public   string  Alias
        {
            get
            {
                if (String.IsNullOrEmpty(_alias))
                {
                    if (Source is TableSource)
                        return (Source as TableSource).Alias;

                    if (Source is SqlTable)
                        return ((SqlTable)Source).Alias;
                }

                return _alias;
            }
            set { _alias = value; }
        }

        public TableSource this[ISqlTableSource table] => this[table, null];

        public TableSource this[ISqlTableSource table, string alias]
        {
            get
            {
                foreach (var tj in Joins)
                {
                    var t = SelectQuery.CheckTableSource(tj.Table, table, alias);

                    if (t != null)
                        return t;
                }

                return null;
            }
        }

        readonly List<JoinedTable> _joins = new List<JoinedTable>();
        public   List<JoinedTable>  Joins => _joins;

        public void ForEach(Action<TableSource> action, HashSet<SelectQuery> visitedQueries)
        {
            action(this);
            foreach (var join in Joins)
                @join.Table.ForEach(action, visitedQueries);

            if (Source is SelectQuery && visitedQueries.Contains((SelectQuery)Source))
                ((SelectQuery)Source).ForEachTable(action, visitedQueries);
        }

        public IEnumerable<ISqlTableSource> GetTables()
        {
            yield return Source;

            foreach (var join in Joins)
                foreach (var table in @join.Table.GetTables())
                    yield return table;
        }

        public int GetJoinNumber()
        {
            var n = Joins.Count;

            foreach (var join in Joins)
                n += @join.Table.GetJoinNumber();

            return n;
        }

#if OVERRIDETOSTRING

			public override string ToString()
			{
				return ((IQueryElement)this).ToString(new StringBuilder(), new Dictionary<IQueryElement,IQueryElement>()).ToString();
			}

#endif

        #region IEquatable<ISqlExpression> Members

        bool IEquatable<ISqlExpression>.Equals(ISqlExpression other)
        {
            return this == other;
        }

        #endregion

        #region ISqlExpressionWalkable Members

        public ISqlExpression Walk(bool skipColumns, Func<ISqlExpression,ISqlExpression> func)
        {
            Source = (ISqlTableSource)Source.Walk(skipColumns, func);

            foreach (var t in Joins)
                ((ISqlExpressionWalkable)t).Walk(skipColumns, func);

            return this;
        }

        #endregion

        #region ISqlTableSource Members

        public int       SourceID => Source.SourceID;

        public SqlField  All => Source.All;

        IList<ISqlExpression> ISqlTableSource.GetKeys(bool allIfEmpty)
        {
            return Source.GetKeys(allIfEmpty);
        }

        #endregion

        #region ICloneableElement Members

        public ICloneableElement Clone(Dictionary<ICloneableElement, ICloneableElement> objectTree, Predicate<ICloneableElement> doClone)
        {
            if (!doClone(this))
                return this;

            ICloneableElement clone;

            if (!objectTree.TryGetValue(this, out clone))
            {
                var ts = new TableSource((ISqlTableSource)Source.Clone(objectTree, doClone), _alias);

                objectTree.Add(this, clone = ts);

                ts._joins.AddRange(_joins.Select(jt => (JoinedTable)jt.Clone(objectTree, doClone)));
            }

            return clone;
        }

        #endregion

        #region IQueryElement Members

        protected override IEnumerable<IQueryElement> GetChildItemsInternal()
        {
            return base.GetChildItemsInternal().UnionChilds(Source).UnionChilds(Joins);
        }

        public override QueryElementType ElementType => QueryElementType.TableSource;

        public override StringBuilder ToString(StringBuilder sb, Dictionary<IQueryElement,IQueryElement> dic)
        {
            if (dic.ContainsKey(this))
                return sb.Append("...");

            dic.Add(this, this);

            if (Source is SelectQuery)
            {
                sb.Append("(\n\t");
                var len = sb.Length;
                Source.ToString(sb, dic).Replace("\n", "\n\t", len, sb.Length - len);
                sb.Append("\n)");
            }
            else
                Source.ToString(sb, dic);

            sb
                .Append(" as t")
                .Append(SourceID);

            foreach (IQueryElement join in Joins)
            {
                sb.AppendLine().Append('\t');
                var len = sb.Length;
                @join.ToString(sb, dic).Replace("\n", "\n\t", len, sb.Length - len);
            }

            dic.Remove(this);

            return sb;
        }

        #endregion

        #region ISqlExpression Members

        public bool CanBeNull()
        {
            return Source.CanBeNull();
        }

        public bool Equals(ISqlExpression other, Func<ISqlExpression,ISqlExpression,bool> comparer)
        {
            return this == other;
        }

        public int  Precedence => Source.Precedence;

        public Type SystemType => Source.SystemType;

        #endregion
    }
}