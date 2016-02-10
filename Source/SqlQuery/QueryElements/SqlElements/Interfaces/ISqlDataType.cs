namespace LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces
{
    using System;

    public interface ISqlDataType : IQueryExpression
    {
        DataType DataType { get; }

        Type Type { get; }

        int? Length { get; }

        int? Precision { get; }

        int? Scale { get; }

        bool IsCharDataType { get; }
    }
}