using System.Diagnostics;

namespace Bars2Db.SchemaProvider
{
    [DebuggerDisplay(
        "TableID = {TableID}, Name = {Name}, DataType = {DataType}, Length = {Length}, Precision = {Precision}, Scale = {Scale}"
        )]
    public class ColumnInfo
    {
        public string ColumnType;
        public string DataType;
        public string Description;
        public bool IsIdentity;
        public bool IsNullable;
        public long? Length;
        public string Name;
        public int Ordinal;
        public int? Precision;
        public int? Scale;
        public bool SkipOnInsert;
        public bool SkipOnUpdate;
        public string TableID;
    }
}