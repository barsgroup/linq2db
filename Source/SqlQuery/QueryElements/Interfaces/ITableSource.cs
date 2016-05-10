using System.Collections.Generic;
using Bars2Db.SqlQuery.QueryElements.SqlElements.Interfaces;
using Bars2Db.SqlQuery.Search;

namespace Bars2Db.SqlQuery.QueryElements.Interfaces
{
    public interface ITableSource : ISqlTableSource
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