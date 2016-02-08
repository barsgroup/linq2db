namespace LinqToDB.SqlQuery.QueryElements.Interfaces
{
    using System;

    using LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces;

    public interface IColumn : IEquatable<IColumn>,
                               ISqlExpression
    {
        ISqlExpression Expression { get; set; }

        ISelectQuery Parent { get; set; }

        string Alias { get; set; }
    }
}