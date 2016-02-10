namespace LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces
{
    using LinqToDB.Mapping;

    public interface ISqlField : IQueryExpression
    {
        string Alias { get; set; }

        string Name { get; set; }

        bool Nullable { get; set; }

        bool IsPrimaryKey { get; set; }

        int PrimaryKeyOrder { get; set; }

        bool IsIdentity { get; set; }

        bool IsInsertable { get; set; }

        bool IsUpdatable { get; set; }

        DataType DataType { get; set; }

        string DbType { get; set; }

        int? Length { get; set; }

        int? Precision { get; set; }

        int? Scale { get; set; }

        string CreateFormat { get; set; }

        ISqlTableSource Table { get; set; }

        ColumnDescriptor ColumnDescriptor { get; set; }

        string PhysicalName { get; set; }
    }
}