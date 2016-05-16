using System;

namespace Bars2Db.SqlQuery.Search.TypeGraph
{
    [Flags]
    public enum ConnectionType
    {
        None = 0,

        Property = 1 << 0,

        Cast = 1 << 1
    }
}