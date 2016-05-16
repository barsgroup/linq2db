using System.Diagnostics;

namespace Bars2Db.SchemaProvider
{
    [DebuggerDisplay(
        "TypeName = {TypeName}, DataType = {DataType}, CreateFormat = {CreateFormat}, CreateParameters = {CreateParameters}"
        )]
    public class DataTypeInfo
    {
        public string CreateFormat;
        public string CreateParameters;
        public string DataType;
        public int ProviderDbType;
        public string TypeName;
    }
}