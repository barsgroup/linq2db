namespace LinqToDB.SqlQuery.QueryElements.Clauses.Interfaces
{
    using System.Collections.Generic;

    using LinqToDB.SqlQuery.QueryElements.Conditions;
    using LinqToDB.SqlQuery.QueryElements.Conditions.Interfaces;
    using LinqToDB.SqlQuery.QueryElements.Interfaces;
    using LinqToDB.SqlQuery.QueryElements.SqlElements;
    using LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces;
    using LinqToDB.SqlQuery.Search;

    public interface IFromClause: IClauseWithConditionBase, ISqlExpressionWalkable
    {
        IFromClause Table(ISqlTableSource table, params IJoin[] joins);

        ITableSource this[ISqlTableSource table] { get; }

        ITableSource this[ISqlTableSource table, string alias] { get; }

        bool IsChild(ISqlTableSource table);

        [SearchContainer]
        LinkedList<ITableSource> Tables { get; }

        ISqlTableSource FindTableSource(ISqlTable table);
    }
}