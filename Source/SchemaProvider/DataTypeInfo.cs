namespace LinqToDB.SchemaProvider
{
    using System.Diagnostics;

    [DebuggerDisplay("TypeName = {TypeName}, DataType = {DataType}, CreateFormat = {CreateFormat}, CreateParameters = {CreateParameters}")]
    public class DataTypeInfo
    {
        public string TypeName;
        public string DataType;
        public string CreateFormat;
        public string CreateParameters;
        public int    ProviderDbType;
    }
}
