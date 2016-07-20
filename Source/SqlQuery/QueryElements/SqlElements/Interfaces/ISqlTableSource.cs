using System.Collections.Generic;
using Bars2Db.SqlQuery.QueryElements.SqlElements.Enums;

namespace Bars2Db.SqlQuery.QueryElements.SqlElements.Interfaces
{
    public interface ISqlTableSource : IQueryExpression
    {
        int SourceID { get; }

        ESqlTableType SqlTableType { get; set; }

        IList<IQueryExpression> GetKeys(bool allIfEmpty);
    }
}