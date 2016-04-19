namespace LinqToDB.SqlQuery.QueryElements
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using LinqToDB.Extensions;
    using LinqToDB.SqlQuery.QueryElements.Enums;
    using LinqToDB.SqlQuery.QueryElements.Interfaces;
    using LinqToDB.SqlQuery.QueryElements.SqlElements;
    using LinqToDB.SqlQuery.QueryElements.SqlElements.Enums;
    using LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces;
    using LinqToDB.SqlQuery.Search;

    public class TableSource : BaseQueryElement, ITableSource
    {
        public TableSource(ISqlTableSource source, string alias)
            : this(source, alias, null)
        {
        }

        public TableSource(ISqlTableSource source, string alias, params IJoinedTable[] joins)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            Source = source;
            _alias = alias;

            if (joins != null)
            {
                Joins = new LinkedList<IJoinedTable>(joins);
            }
        }

        public TableSource(ISqlTableSource source, string alias, IEnumerable<IJoinedTable> joins)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            Source = source;
            _alias = alias;

            if (joins != null)
            {
                Joins = new LinkedList<IJoinedTable>(joins);
            }
        }

        public ISqlTableSource Source       { get; set; }

        public ESqlTableType    SqlTableType
        {
            get { return Source.SqlTableType; }
            set { throw new NotSupportedException();}
        }

        // TODO: remove internal.
        internal string _alias;
        public   string  Alias
        {
            get
            {
                if (string.IsNullOrEmpty(_alias))
                {
                    var tableSource = Source as ITableSource;
                    if (tableSource != null)
                        return tableSource.Alias;

                    var sqlTable = Source as ISqlTable;
                    if (sqlTable != null)
                        return sqlTable.Alias;
                }

                return _alias;
            }
            set { _alias = value; }
        }

        public ITableSource this[ISqlTableSource table] => this[table, null];

        public ITableSource this[ISqlTableSource table, string alias] => Joins.Select(tj => SelectQuery.CheckTableSource(tj.Table, table, alias)).FirstOrDefault(t => t != null);

        public LinkedList<IJoinedTable>  Joins { get; } = new LinkedList<IJoinedTable>();

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

            Joins.ForEach(node => n += node.Value.Table.GetJoinNumber());

            return n;
        }

#if OVERRIDETOSTRING

            public override string ToString()
            {
                return ((IQueryElement)this).ToString(new StringBuilder(), new Dictionary<IQueryElement,IQueryElement>()).ToString();
            }

#endif

        #region IEquatable<ISqlExpression> Members

        bool IEquatable<IQueryExpression>.Equals(IQueryExpression other)
        {
            return this == other;
        }

        #endregion

        #region ISqlExpressionWalkable Members

        public IQueryExpression Walk(bool skipColumns, Func<IQueryExpression,IQueryExpression> func)
        {
            Source = (ISqlTableSource)Source.Walk(skipColumns, func);

            Joins.ForEach(node => node.Value.Walk(skipColumns, func));

            return this;
        }

        #endregion

        #region ISqlTableSource Members

        public int       SourceID => Source.SourceID;

        public ISqlField All
        {
            get { return Source.All; }
            set {  }
        }

        IList<IQueryExpression> ISqlTableSource.GetKeys(bool allIfEmpty)
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

                foreach (var joinedTable in Joins.Select(jt => (IJoinedTable)jt.Clone(objectTree, doClone)))
                {
                    ts.Joins.AddLast(joinedTable);
                }
            }

            return clone;
        }

        #endregion

        #region IQueryElement Members

        public override EQueryElementType ElementType => EQueryElementType.TableSource;

        public override StringBuilder ToString(StringBuilder sb, Dictionary<IQueryElement, IQueryElement> dic)
        {
            if (dic.ContainsKey(this))
                return sb.Append("...");

            dic.Add(this, this);

            if (Source is ISelectQuery)
            {
                sb.Append("(\n\t");
                var len = sb.Length;
                Source.ToString(sb, dic).Replace("\n", "\n\t", len, sb.Length - len);
                sb.Append("\n)");
            }
            else
                Source.ToString(sb, dic);

            sb.Append(" as t").Append(SourceID);

            Joins.ForEach(
                node =>

                {
                    sb.AppendLine().Append('\t');
                    var len = sb.Length;
                    node.Value.ToString(sb, dic).Replace("\n", "\n\t", len, sb.Length - len);

                });

            dic.Remove(this);

            return sb;
        }

        #endregion

        #region ISqlExpression Members

        public bool CanBeNull()
        {
            return Source.CanBeNull();
        }

        public bool Equals(IQueryExpression other, Func<IQueryExpression,IQueryExpression,bool> comparer)
        {
            return this == other;
        }

        public int  Precedence => Source.Precedence;

        public Type SystemType => Source.SystemType;

        #endregion
    }
}