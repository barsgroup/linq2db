using System;

namespace Bars2Db.SchemaProvider
{
    public class GetSchemaOptions
    {
        public string[] ExcludedSchemas;
        public bool GenerateChar1AsString = false;
        public Func<ForeignKeySchema, string> GetAssociationMemberName = null;
        public bool GetProcedures = true;
        public bool GetTables = true;
        public string[] IncludedSchemas;

        public Func<ProcedureSchema, bool> LoadProcedure = _ => true;
        public Action<int, int> ProcedureLoadingProgress = (outOf, current) => { };
    }
}