namespace LinqToDB.SchemaProvider
{
    using System.Diagnostics;

    [DebuggerDisplay("TableID = {TableID}, PrimaryKeyName = {PrimaryKeyName}, ColumnName = {ColumnName}, Ordinal = {Ordinal}")]
    public class PrimaryKeyInfo
    {
        public string TableID;
        public string PrimaryKeyName;
        public string ColumnName;
        public int    Ordinal;
    }
}
