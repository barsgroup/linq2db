using System;
using System.Collections.Generic;
using Bars2Db.Mapping;
using Bars2Db.SqlQuery.Search;

namespace Bars2Db.SqlQuery.QueryElements.SqlElements.Interfaces
{
    public interface ISqlTable : ISqlTableSource
    {
        string Name { get; set; }

        string Alias { get; set; }

        string Database { get; set; }

        string Owner { get; set; }

        Type ObjectType { get; }

        string PhysicalName { get; set; }

        [SearchContainer]
        LinkedList<IQueryExpression> TableArguments { get; }

        [SearchContainer]
        Dictionary<string, ISqlField> Fields { get; }

        SequenceNameAttribute[] SequenceAttributes { get; }

        ISqlField GetIdentityField();
    }
}