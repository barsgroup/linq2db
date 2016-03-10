namespace LinqToDB.SqlQuery.QueryElements.Interfaces
{
    using System;

    using LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces;
    using LinqToDB.SqlQuery.Search;

    public interface IColumn : IEquatable<IColumn>,
                               IQueryExpression
    {
        [SearchContainer]
        IQueryExpression Expression { get; set; }

        ISelectQuery Parent { get; set; }

        string Alias { get; set; }
    }
}