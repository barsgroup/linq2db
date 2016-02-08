namespace LinqToDB.SqlQuery.QueryElements.Interfaces
{
    using System.Collections.Generic;

    using LinqToDB.SqlQuery.QueryElements.Conditions;
    using LinqToDB.SqlQuery.QueryElements.SqlElements;
    using LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces;

    public interface IFromClause: IClauseBase, ISqlExpressionWalkable
    {
        IFromClause Table(ISqlTableSource table, params Join[] joins);

        IFromClause Table(ISqlTableSource table, string alias, params Join[] joins);

        ITableSource this[ISqlTableSource table] { get; }

        ITableSource this[ISqlTableSource table, string alias] { get; }

        bool IsChild(ISqlTableSource table);

        List<ITableSource> Tables { get; }

        ISqlTableSource FindTableSource(SqlTable table);
    }
}