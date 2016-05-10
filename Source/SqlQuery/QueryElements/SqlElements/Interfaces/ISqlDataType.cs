using System;

namespace Bars2Db.SqlQuery.QueryElements.SqlElements.Interfaces
{
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