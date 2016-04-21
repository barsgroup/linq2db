namespace LinqToDB.SqlQuery.QueryElements.Interfaces
{
    using System.Collections.Generic;

    using LinqToDB.SqlQuery.QueryElements.Clauses;
    using LinqToDB.SqlQuery.QueryElements.Clauses.Interfaces;
    using LinqToDB.SqlQuery.QueryElements.Enums;
    using LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces;
    using LinqToDB.SqlQuery.Search;

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

        bool IsInsert { get; }

        bool IsUpdate { get; }

        [SearchContainer]
        ISelectClause Select { get; }

        ICreateTableStatement CreateTable { get; }

        [SearchContainer]
        IInsertClause Insert { get; }

        IUpdateClause Update { get; }

        [SearchContainer]
        IDeleteClause Delete { get; }

        [SearchContainer]
        IFromClause From { get; }

        [SearchContainer]
        IWhereClause Where { get; }

        [SearchContainer]
        IGroupByClause GroupBy { get; }

        [SearchContainer]
        IWhereClause Having { get; }

        [SearchContainer]
        IOrderByClause OrderBy { get; }

        [SearchContainer]
        LinkedList<IUnion> Unions { get; }

        bool HasUnion { get; }

        void ClearInsert();

        void ClearUpdate();

        ISelectQuery ProcessParameters();

        ISelectQuery Clone();

        void SetAliases();

        string[] GetTempAliases(int n, string defaultAlias);

        ISqlTableSource GetTableSource(ISqlTableSource table);

        void Init(IInsertClause insert,
                  IUpdateClause update,
                  IDeleteClause delete,
                  ISelectClause select,
                  IFromClause from,
                  IWhereClause where,
                  IGroupByClause groupBy,
                  IWhereClause having,
                  IOrderByClause orderBy,
                  LinkedList<IUnion> unions,
                  ISelectQuery parentSelect,
                  ICreateTableStatement createTable,
                  bool parameterDependent,
                  List<ISqlParameter> parameters);
    }
}