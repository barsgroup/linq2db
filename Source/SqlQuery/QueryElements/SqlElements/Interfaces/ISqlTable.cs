namespace LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces
{
    using System;
    using System.Collections.Generic;

    using LinqToDB.Mapping;
    using LinqToDB.SqlQuery.Search;

    public interface ISqlTable : ISqlTableSource
    {
        ISqlField this[string fieldName] { get; }

        string Name { get; set; }

        string Alias { get; set; }

        string Database { get; set; }

        string Owner { get; set; }

        Type ObjectType { get; set; }

        string PhysicalName { get; set; }

        [SearchContainer]
        LinkedList<IQueryExpression> TableArguments { get;}

        [SearchContainer]
        Dictionary<string, ISqlField> Fields { get; }

        SequenceNameAttribute[] SequenceAttributes { get; }

        ISqlField GetIdentityField();

        void Add(ISqlField field);

        void AddRange(IEnumerable<ISqlField> collection);
    }
}