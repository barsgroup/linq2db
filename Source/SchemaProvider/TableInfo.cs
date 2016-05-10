using System.Diagnostics;

namespace Bars2Db.SchemaProvider
{
    [DebuggerDisplay(
        "CatalogName = {CatalogName}, SchemaName = {SchemaName}, TableName = {TableName}, IsDefaultSchema = {IsDefaultSchema}, IsView = {IsView}, Description = {Description}"
        )]
    public class TableInfo
    {
        public string CatalogName;
        public string Description;
        public bool IsDefaultSchema;
        public bool IsView;
        public string SchemaName;
        public string TableID;
        public string TableName;
    }
}