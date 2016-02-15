namespace LinqToDB.SqlQuery.QueryElements.Interfaces
{
    using System;
    using System.Collections.Generic;

    using LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces;

    public interface ITableSource: ISqlTableSource
    {
        ISqlTableSource Source { get; set; }

        string Alias { get; set; }

        List<IJoinedTable> Joins { get; }

        ITableSource this[ISqlTableSource table] { get; }

        ITableSource this[ISqlTableSource table, string alias] { get; }

        IEnumerable<ISqlTableSource> GetTables();

        int GetJoinNumber();
    }
}