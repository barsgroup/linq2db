namespace LinqToDB.SqlQuery.QueryElements.Interfaces
{
    using System;
    using System.Collections.Generic;

    using LinqToDB.SqlQuery.QueryElements.Clauses;
    using LinqToDB.SqlQuery.QueryElements.SqlElements;
    using LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces;

    public interface ISelectQuery : ISqlTableSource
    {
        List<SqlParameter> Parameters { get; }

        List<object> Properties { get; }

        bool IsParameterDependent { get; set; }

        ISelectQuery ParentSelect { get; set; }

        bool IsSimple { get; }

        QueryType QueryType { get; set; }

        bool IsCreateTable { get; }

        bool IsSelect { get; }

        bool IsDelete { get; }

        bool IsInsertOrUpdate { get; }

        bool IsInsert { get; }

        bool IsUpdate { get; }

        SelectClause Select { get; }

        CreateTableStatement CreateTable { get; }

        InsertClause Insert { get; }

        UpdateClause Update { get; }

        DeleteClause Delete { get; }

        FromClause From { get; }

        WhereClause Where { get; }

        GroupByClause GroupBy { get; }

        WhereClause Having { get; }

        OrderByClause OrderBy { get; }

        List<Union> Unions { get; }

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

        void Init(InsertClause insert,
                  UpdateClause update,
                  DeleteClause delete,
                  SelectClause select,
                  FromClause from,
                  WhereClause where,
                  GroupByClause groupBy,
                  WhereClause having,
                  OrderByClause orderBy,
                  List<Union> unions,
                  ISelectQuery parentSelect,
                  CreateTableStatement createTable,
                  bool parameterDependent,
                  List<SqlParameter> parameters);
    }
}