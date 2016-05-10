using System;
using Bars2Db.SqlQuery.QueryElements.SqlElements.Interfaces;
using Bars2Db.SqlQuery.Search;

namespace Bars2Db.SqlQuery.QueryElements.Interfaces
{
    public interface IColumn : IEquatable<IColumn>,
        IQueryExpression
    {
        [SearchContainer]
        IQueryExpression Expression { get; set; }

        ISelectQuery Parent { get; set; }

        string Alias { get; set; }
    }
}