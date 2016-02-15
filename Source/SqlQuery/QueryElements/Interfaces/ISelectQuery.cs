namespace LinqToDB.SqlQuery.QueryElements.Interfaces
{
    using System;
    using System.Collections.Generic;

    using LinqToDB.SqlQuery.QueryElements.Clauses;
    using LinqToDB.SqlQuery.QueryElements.Clauses.Interfaces;
    using LinqToDB.SqlQuery.QueryElements.Enums;
    using LinqToDB.SqlQuery.QueryElements.SqlElements;
    using LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces;

    public interface ISelectQuery : ISqlTableSource
    {
        List<ISqlParameter> Parameters { get; }

        List<object> Properties { get; }

        bool IsParameterDependent { get; set; }

        ISelectQuery ParentSelect { get; set; }

        bool IsSimple { get; }

        EQueryType EQueryType { get; set; }

        bool IsCreateTable { get; }

        bool IsSelect { get; }

        bool IsDelete { get; }

        bool IsInsertOrUpdate { get; }

        bool IsInsert { get; }

        bool IsUpdate { get; }

        ISelectClause Select { get; }

        ICreateTableStatement CreateTable { get; }

        IInsertClause Insert { get; }

        IUpdateClause Update { get; }

        DeleteClause Delete { get; }

        IFromClause From { get; }

        IWhereClause Where { get; }

        GroupByClause GroupBy { get; }

        IWhereClause Having { get; }

        IOrderByClause OrderBy { get; }

        LinkedList<IUnion> Unions { get; }

        bool HasUnion { get; }

        void ClearInsert();

        void ClearUpdate();

        void ClearDelete();

        void AddUnion(ISelectQuery union, bool isAll);

        ISelectQuery ProcessParameters();

        ISelectQuery Clone();

        ISelectQuery Clone(Predicate<ICloneableElement> doClone);

        void RemoveAlias(string alias);

        void SetAliases();

        string GetAlias(string desiredAlias, string defaultAlias);

        string[] GetTempAliases(int n, string defaultAlias);

        void ForEachTable(Action<ITableSource> action, HashSet<ISelectQuery> visitedQueries);

        ISqlTableSource GetTableSource(ISqlTableSource table);

        void Init(IInsertClause insert,
                  IUpdateClause update,
                  DeleteClause delete,
                  ISelectClause select,
                  IFromClause from,
                  IWhereClause where,
                  GroupByClause groupBy,
                  IWhereClause having,
                  IOrderByClause orderBy,
                  LinkedList<IUnion> unions,
                  ISelectQuery parentSelect,
                  ICreateTableStatement createTable,
                  bool parameterDependent,
                  List<ISqlParameter> parameters);
    }
}