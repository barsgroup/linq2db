using System.Diagnostics;

namespace Bars2Db.SchemaProvider
{
    [DebuggerDisplay(
        "TableID = {TableID}, PrimaryKeyName = {PrimaryKeyName}, ColumnName = {ColumnName}, Ordinal = {Ordinal}")]
    public class PrimaryKeyInfo
    {
        public string ColumnName;
        public int Ordinal;
        public string PrimaryKeyName;
        public string TableID;
    }
}