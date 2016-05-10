using System.Collections.Generic;

namespace Bars2Db.SchemaProvider
{
    public class DatabaseSchema
    {
        public string DataSource { get; set; }

        public string Database { get; set; }

        public string ServerVersion { get; set; }

        public List<TableSchema> Tables { get; set; }

        public List<ProcedureSchema> Procedures { get; set; }
    }
}