using System.Collections.Generic;
using Bars2Db.Data;

namespace Bars2Db.DataProvider.Oracle
{
    internal class OracleMerge : BasicMerge
    {
        protected override bool BuildUsing<T>(DataConnection dataConnection, IEnumerable<T> source)
        {
            return BuildUsing2(dataConnection, source, null, "FROM SYS.DUAL");
        }
    }
}