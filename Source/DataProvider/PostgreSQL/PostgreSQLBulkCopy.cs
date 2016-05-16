using System.Collections.Generic;
using Bars2Db.Data;

namespace Bars2Db.DataProvider.PostgreSQL
{
    internal class PostgreSQLBulkCopy : BasicBulkCopy
    {
        protected override BulkCopyRowsCopied MultipleRowsCopy<T>(
            DataConnection dataConnection, BulkCopyOptions options, IEnumerable<T> source)
        {
            return MultipleRowsCopy1(dataConnection, options, false, source);
        }
    }
}