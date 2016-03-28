namespace LinqToDB.SqlQuery.Search.TypeGraph
{
    using System;

    [Flags]
    public enum ConnectionType
    {
        None = 0,

        Property = 1 << 0,
        
        Cast = 1 << 1
    }
}