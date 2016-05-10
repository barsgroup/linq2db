using Bars2Db.Mapping;

namespace Bars2Db.SqlQuery.QueryElements.SqlElements.Interfaces
{
    public interface ISqlField : IQueryExpression
    {
        string Alias { get; set; }

        string Name { get; }

        bool Nullable { get; }

        bool IsPrimaryKey { get; }

        int PrimaryKeyOrder { get; }

        bool IsIdentity { get; }

        bool IsInsertable { get; }

        bool IsUpdatable { get; }

        DataType DataType { get; }

        string DbType { get; }

        int? Length { get; }

        int? Precision { get; }

        int? Scale { get; }

        string CreateFormat { get; }

        ISqlTableSource Table { get; set; }

        ColumnDescriptor ColumnDescriptor { get; }

        string PhysicalName { get; }
    }
}