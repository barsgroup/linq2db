using Bars2Db.Data;

namespace Bars2Db.SchemaProvider
{
    public interface ISchemaProvider
    {
        DatabaseSchema GetSchema(DataConnection dataConnection, GetSchemaOptions options = null);
    }
}