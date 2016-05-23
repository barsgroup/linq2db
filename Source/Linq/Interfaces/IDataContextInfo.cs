using Bars2Db.Mapping;
using Bars2Db.SqlProvider;

namespace Bars2Db.Linq.Interfaces
{
    public interface IDataContextInfo
    {
        IDataContext DataContext { get; }
        string ContextID { get; }
        MappingSchema MappingSchema { get; }
        bool DisposeContext { get; }
        SqlProviderFlags SqlProviderFlags { get; }

        ISqlBuilder CreateSqlBuilder();
        ISqlOptimizer GetSqlOptimizer();
        IDataContextInfo Clone(bool forNestedQuery);
    }
}