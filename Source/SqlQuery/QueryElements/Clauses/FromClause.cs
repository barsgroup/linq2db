namespace LinqToDB.SqlQuery.QueryElements.Clauses
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using LinqToDB.Extensions;
    using LinqToDB.SqlQuery.QueryElements.Clauses.Interfaces;
    using LinqToDB.SqlQuery.QueryElements.Conditions;
    using LinqToDB.SqlQuery.QueryElements.Conditions.Interfaces;
    using LinqToDB.SqlQuery.QueryElements.Enums;
    using LinqToDB.SqlQuery.QueryElements.Interfaces;
    using LinqToDB.SqlQuery.QueryElements.SqlElements;
    using LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces;
    using LinqToDB.SqlQuery.Search;

    public class FromClause : ClauseBase,
                              IFromClause
    {
        internal FromClause(ISelectQuery selectQuery) : base(selectQuery)
        {
        }

        internal FromClause(ISelectQuery selectQuery, IFromClause clone, Dictionary<ICloneableElement, ICloneableElement> objectTree, Predicate<ICloneableElement> doClone)
            : base(selectQuery)
        {

            clone.Tables.ForEach(
                node =>
                {
                    var value = (ITableSource)node.Value.Clone(objectTree, doClone);
                    _tables.AddLast(value);
                });
        }

        internal FromClause(LinkedList<ITableSource> tables) : base(null)
        {
            tables.ForEach(node => _tables.AddLast(node.Value));
        }

        public IFromClause Table(ISqlTableSource table, params IJoin[] joins)
        {
            return Table(table, null, joins);
        }

        public IFromClause Table(ISqlTableSource table, string alias, params IJoin[] joins)
        {
            var ts = AddOrGetTable(table, alias);

            if (joins != null && joins.Length > 0)
            {
                for (int index = 0; index < joins.Length; index++)
                {
                    ts.Joins.AddLast(joins[index].JoinedTable);
                }
            }

            return this;
        }

        ITableSource GetTable(ISqlTableSource table, string alias)
        {
            foreach (var ts in Tables)
                if (ts.Source == table)
                    if (alias == null || ts.Alias == alias)
                        return ts;
                    else
                        throw new ArgumentException("alias");

            return null;
        }

        ITableSource AddOrGetTable(ISqlTableSource table, string alias)
        {
            var ts = GetTable(table, alias);

            if (ts != null)
                return ts;

            var t = new TableSource(table, alias);

            Tables.AddLast(t);

            return t;
        }

        public ITableSource this[ISqlTableSource table] => this[table, null];

        public ITableSource this[ISqlTableSource table, string alias]
        {
            get
            {
                foreach (var ts in Tables)
                {
                    var t = QueryElements.SelectQuery.CheckTableSource(ts, table, alias);

                    if (t != null)
                        return t;
                }

                return null;
            }
        }

        public bool IsChild(ISqlTableSource table)
        {
            return Tables.Any(ts => ts.Source == table || CheckChild(ts.Joins, table));
        }

        static bool CheckChild(IEnumerable<IJoinedTable> joins, ISqlTableSource table)
        {
            foreach (var j in joins)
                if (j.Table.Source == table || CheckChild(j.Table.Joins, table))
                    return true;
            return false;
        }

        readonly LinkedList<ITableSource> _tables = new LinkedList<ITableSource>();

        public LinkedList<ITableSource>  Tables => _tables;

        static IEnumerable<ISqlTableSource> GetJoinTables(ITableSource source, EQueryElementType elementType)
        {
            if (source.Source.ElementType == elementType)
                yield return source.Source;

            foreach (var join in source.Joins)
                foreach (var table in GetJoinTables(@join.Table, elementType))
                    yield return table;
        }

        internal IEnumerable<ISqlTableSource> GetFromTables()
        {
            return Tables.SelectMany(_ => GetJoinTables(_, EQueryElementType.SqlTable));
        }

        internal IEnumerable<ISqlTableSource> GetFromQueries()
        {
            return Tables.SelectMany(_ => GetJoinTables(_, EQueryElementType.SqlQuery));
        }

        static ITableSource FindTableSource(ITableSource source, ISqlTable table)
        {
            if (source.Source == table)
                return source;

            source.Joins.ApplyUntilNonDefaultResult(
                node =>
                    {
                        var join = node.Value;
                        var ts = FindTableSource(@join.Table, table);
                        return ts;
                    });

            return null;
        }

        public ISqlTableSource FindTableSource(ISqlTable table)
        {
            foreach (var source in Tables)
            {
                var ts = FindTableSource(source, table);
                if (ts != null)
                    return ts;
            }

            return null;
        }

        #region ISqlExpressionWalkable Members

        IQueryExpression ISqlExpressionWalkable.Walk(bool skipColumns, Func<IQueryExpression, IQueryExpression> func)
        {
            Tables.ForEach(source => source.Value.Walk(skipColumns, func));

            return null;
        }

        #endregion

        #region IQueryElement Members

        public override EQueryElementType ElementType => EQueryElementType.FromClause;

        public override StringBuilder ToString(StringBuilder sb, Dictionary<IQueryElement,IQueryElement> dic)
        {
            sb.Append(" \nFROM \n");

            if (Tables.Count > 0)
            {
                foreach (IQueryElement ts in Tables)
                {
                    sb.Append('\t');
                    var len = sb.Length;
                    ts.ToString(sb, dic).Replace("\n", "\n\t", len, sb.Length - len);
                    sb.Append(", ");
                }

                sb.Length -= 2;
            }

            return sb;
        }

        #endregion
    }
}