namespace LinqToDB.SqlQuery.QueryElements.Interfaces
{
    using System;
    using System.Collections.Generic;

    using LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces;
    using LinqToDB.SqlQuery.Search;

    public interface ITableSource: ISqlTableSource
    {
        [SearchContainer]
        ISqlTableSource Source { get; set; }

        string Alias { get; set; }

        [SearchContainer]
        LinkedList<IJoinedTable> Joins { get; }

        ITableSource this[ISqlTableSource table, string alias] { get; }

        IEnumerable<ISqlTableSource> GetTables();

        int GetJoinNumber();
    }
}