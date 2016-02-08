namespace LinqToDB.SqlQuery.QueryElements.Interfaces
{
    using System;
    using System.Collections.Generic;

    using LinqToDB.SqlQuery.QueryElements.Clauses;
    using LinqToDB.SqlQuery.QueryElements.Enums;
    using LinqToDB.SqlQuery.QueryElements.SqlElements;
    using LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces;

    public interface ISelectQuery : ISqlTableSource
    {
        List<SqlParameter> Parameters { get; }

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

        SelectClause Select { get; }

        ICreateTableStatement CreateTable { get; }

        IInsertClause Insert { get; }

        UpdateClause Update { get; }

        DeleteClause Delete { get; }

        IFromClause From { get; }

        WhereClause Where { get; }

        GroupByClause GroupBy { get; }

        WhereClause Having { get; }

        OrderByClause OrderBy { get; }

        List<IUnion> Unions { get; }

        bool HasUnion { get; }

        string SqlText { get; }

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
                  UpdateClause update,
                  DeleteClause delete,
                  SelectClause select,
                  IFromClause from,
                  WhereClause where,
                  GroupByClause groupBy,
                  WhereClause having,
                  OrderByClause orderBy,
                  List<IUnion> unions,
                  ISelectQuery parentSelect,
                  ICreateTableStatement createTable,
                  bool parameterDependent,
                  List<SqlParameter> parameters);
    }
}