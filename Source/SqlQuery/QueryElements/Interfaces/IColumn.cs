namespace LinqToDB.SqlQuery.QueryElements.Interfaces
{
    using System;

    using LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces;

    public interface IColumn : IEquatable<IColumn>,
                               IQueryExpression
    {
        IQueryExpression Expression { get; set; }

        ISelectQuery Parent { get; set; }

        string Alias { get; set; }
    }
}