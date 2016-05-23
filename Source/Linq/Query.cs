using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using Bars2Db.Common;
using Bars2Db.Data;
using Bars2Db.Expressions;
using Bars2Db.Extensions;
using Bars2Db.Linq.Builder;
using Bars2Db.Linq.Interfaces;
using Bars2Db.Mapping;
using Bars2Db.SqlProvider;
using Bars2Db.SqlQuery.QueryElements;
using Bars2Db.SqlQuery.QueryElements.Enums;
using Bars2Db.SqlQuery.QueryElements.Interfaces;
using Bars2Db.SqlQuery.QueryElements.SqlElements;
using Bars2Db.SqlQuery.QueryElements.SqlElements.Interfaces;

namespace Bars2Db.Linq
{
    public abstract class Query
    {
        protected readonly ICollection<int> _resultMappingIndexes = new List<int>();

        public bool IsSaveResultMappingIndexes { get; set; }

        public ICollection<int> ResultMappingIndexes => _resultMappingIndexes;

        #region Init

        public abstract void Init(IBuildContext parseContext, List<ParameterAccessor> sqlParameters);

        #endregion

        public class QueryInfo : IQueryContext
        {
            public List<ParameterAccessor> Parameters = new List<ParameterAccessor>();

            public QueryInfo()
            {
                SelectQuery = new SelectQuery();
            }

            public ISelectQuery SelectQuery { get; set; }
            public object Context { get; set; }

            public List<string> QueryHints { get; set; }

            public ISqlParameter[] GetParameters()
            {
                var ps = new ISqlParameter[Parameters.Count];

                for (var i = 0; i < ps.Length; i++)
                    ps[i] = Parameters[i].SqlParameter;

                return ps;
            }

            public string GetQueryFieldAliasByFieldName(string fieldName)
            {
                var column = SelectQuery.Select.Columns.FirstOrDefault(
                    c =>
                    {
                        var expression = c.Expression as IColumn;
                        if (expression != null)
                        {
                            return ((ISqlField) expression.Expression).Name == fieldName;
                        }

                        var sqlField = c.Expression as ISqlField;
                        if (sqlField != null)
                        {
                            return sqlField.Alias == null && sqlField.Name == fieldName;
                        }

                        return false;
                    });

                return column != null
                    ? column.Alias
                    : null;
            }
        }

        #region Compare

        public string ContextID;
        public Expression Expression;
        public MappingSchema MappingSchema;
        public SqlProviderFlags SqlProviderFlags;

        public readonly List<QueryInfo> Queries = new List<QueryInfo>(1);

        public bool Compare(string contextId, MappingSchema mappingSchema, Expression expr)
        {
            return
                ContextID.Equals(contextId) &&
                MappingSchema == mappingSchema &&
                Expression.EqualsTo(expr, _queryableAccessorDic);
        }

        private readonly Dictionary<Expression, QueryableAccessor> _queryableAccessorDic =
            new Dictionary<Expression, QueryableAccessor>();

        private readonly List<QueryableAccessor> _queryableAccessorList = new List<QueryableAccessor>();

        internal int AddQueryableAccessors(Expression expr, Expression<Func<Expression, IQueryable>> qe)
        {
            QueryableAccessor e;

            if (_queryableAccessorDic.TryGetValue(expr, out e))
                return _queryableAccessorList.IndexOf(e);

            e = new QueryableAccessor {Accessor = qe.Compile()};
            e.Queryable = e.Accessor(expr);

            _queryableAccessorDic.Add(expr, e);
            _queryableAccessorList.Add(e);

            return _queryableAccessorList.Count - 1;
        }

        public Expression GetIQueryable(int n, Expression expr)
        {
            return _queryableAccessorList[n].Accessor(expr).Expression;
        }

        #endregion
    }

    public class Query<T> : Query
    {
        #region GetInfo

        //static          Query<T> _first;
        //static readonly object   _sync = new object();
        //const int CacheSize = 100;

        public static Query<T> GetQuery(IDataContextInfo dataContextInfo, Expression expr,
            bool isSaveResultMappingIndexes)
        {
            var newQuery = new Query<T>
            {
                IsSaveResultMappingIndexes = isSaveResultMappingIndexes
            };

            return new ExpressionBuilder(newQuery, dataContextInfo, expr, null).Build<T>();
        }

        #endregion

        #region GetSqlText

        public string GetSqlText(IDataContext dataContext, Expression expr, object[] parameters, int idx)
        {
            var query = SetCommand(dataContext, expr, parameters, 0, false);
            return dataContext.GetSqlText(query);
        }

        #endregion

        #region Inner Types

        internal delegate TElement Mapper<out TElement>(
            Query<T> query,
            QueryContext qc,
            IDataContext dc,
            IDataReader rd,
            MappingSchema ms,
            Expression expr,
            object[] ps);

        #endregion

        #region Init

        public Query()
        {
            GetIEnumerable = MakeEnumerable;
        }

        public override void Init(IBuildContext parseContext, List<ParameterAccessor> sqlParameters)
        {
            Queries.Add(new QueryInfo
            {
                SelectQuery = parseContext.Select,
                Parameters = sqlParameters
            });

            ContextID = parseContext.Builder.DataContextInfo.ContextID;
            MappingSchema = parseContext.Builder.MappingSchema;
            SqlProviderFlags = parseContext.Builder.DataContextInfo.SqlProviderFlags;
            SqlOptimizer = parseContext.Builder.DataContextInfo.GetSqlOptimizer();
            Expression = parseContext.Builder.OriginalExpression;
        }

        private void ClearParameters()
        {
            foreach (var query in Queries)
                foreach (var sqlParameter in query.Parameters)
                    sqlParameter.Expression = null;
        }

        #endregion

        #region Properties & Fields

        public ISqlOptimizer SqlOptimizer;

        public Func<QueryContext, IDataContextInfo, Expression, object[], object> GetElement;
        public Func<QueryContext, IDataContextInfo, Expression, object[], IEnumerable<T>> GetIEnumerable;

        private IEnumerable<T> MakeEnumerable(QueryContext qc, IDataContextInfo dci, Expression expr, object[] ps)
        {
            yield return ConvertTo<T>.From(GetElement(qc, dci, expr, ps));
        }

        #endregion

        #region NonQueryQuery

        private void FinalizeQuery()
        {
            for (var index = 0; index < Queries.Count; index++)
            {
                var sql = Queries[index];

                sql.SelectQuery = SqlOptimizer.Finalize(sql.SelectQuery);
                sql.Parameters =
                    sql.Parameters.OrderBy(p => sql.SelectQuery.Parameters.IndexOf(p.SqlParameter)).ToList();
            }
        }

        public void SetNonQueryQuery()
        {
            FinalizeQuery();

            if (Queries.Count != 1)
                throw new InvalidOperationException();

            ClearParameters();

            GetElement = (ctx, db, expr, ps) => NonQueryQuery(db, expr, ps);
        }

        private int NonQueryQuery(IDataContextInfo dataContextInfo, Expression expr, object[] parameters)
        {
            var dataContext = dataContextInfo.DataContext;

            object query = null;

            try
            {
                query = SetCommand(dataContext, expr, parameters, 0, true);

                var res = dataContext.ExecuteNonQuery(query);

                return res;
            }
            finally
            {
                if (query != null)
                    dataContext.ReleaseQuery(query);

                if (dataContextInfo.DisposeContext)
                    dataContext.Dispose();
            }
        }

        public void SetNonQueryQuery2()
        {
            FinalizeQuery();

            if (Queries.Count != 2)
                throw new InvalidOperationException();

            ClearParameters();

            GetElement = (ctx, db, expr, ps) => NonQueryQuery2(db, expr, ps);
        }

        private int NonQueryQuery2(IDataContextInfo dataContextInfo, Expression expr, object[] parameters)
        {
            var dataContext = dataContextInfo.DataContext;

            object query = null;

            try
            {
                query = SetCommand(dataContext, expr, parameters, 0, true);

                var n = dataContext.ExecuteNonQuery(query);

                if (n != 0)
                    return n;

                query = SetCommand(dataContext, expr, parameters, 1, true);
                return dataContext.ExecuteNonQuery(query);
            }
            finally
            {
                if (query != null)
                    dataContext.ReleaseQuery(query);

                if (dataContextInfo.DisposeContext)
                    dataContext.Dispose();
            }
        }

        #endregion

        #region ScalarQuery

        public void SetScalarQuery<TS>()
        {
            FinalizeQuery();

            if (Queries.Count != 1)
                throw new InvalidOperationException();

            ClearParameters();

            GetElement = (ctx, db, expr, ps) => ScalarQuery<TS>(db, expr, ps);
        }

        private TS ScalarQuery<TS>(IDataContextInfo dataContextInfo, Expression expr, object[] parameters)
        {
            var dataContext = dataContextInfo.DataContext;

            object query = null;

            try
            {
                query = SetCommand(dataContext, expr, parameters, 0, true);
                return (TS) dataContext.ExecuteScalar(query);
            }
            finally
            {
                if (query != null)
                    dataContext.ReleaseQuery(query);

                if (dataContextInfo.DisposeContext)
                    dataContext.Dispose();
            }
        }

        #endregion

        #region RunQuery

        private int GetParameterIndex(IQueryExpression parameter)
        {
            for (var i = 0; i < Queries[0].Parameters.Count; i++)
            {
                var p = Queries[0].Parameters[i].SqlParameter;

                if (p == parameter)
                    return i;
            }

            throw new InvalidOperationException();
        }

        private IEnumerable<IDataReader> RunQuery(IDataContextInfo dataContextInfo, Expression expr, object[] parameters,
            int queryNumber)
        {
            var dataContext = dataContextInfo.DataContext;

            object query = null;

            try
            {
                query = SetCommand(dataContext, expr, parameters, queryNumber, true);

                using (var dr = dataContext.ExecuteReader(query))
                    while (dr.Read())
                        yield return dr;
            }
            finally
            {
                if (query != null)
                    dataContext.ReleaseQuery(query);

                if (dataContextInfo.DisposeContext)
                    dataContext.Dispose();
            }
        }

        private object SetCommand(IDataContext dataContext, Expression expr, object[] parameters, int idx,
            bool clearQueryHints)
        {
            lock (this)
            {
                SetParameters(expr, parameters, idx);

                var query = Queries[idx];

                if (idx == 0 && (dataContext.QueryHints.Count > 0 || dataContext.NextQueryHints.Count > 0))
                {
                    query.QueryHints = new List<string>(dataContext.QueryHints);
                    query.QueryHints.AddRange(dataContext.NextQueryHints);

                    if (clearQueryHints)
                        dataContext.NextQueryHints.Clear();
                }

                return dataContext.SetQuery(query);
            }
        }

        private ConcurrentDictionary<Type, Func<object, object>> _enumConverters;

        internal void SetParameters(Expression expr, object[] parameters, int idx)
        {
            foreach (var p in Queries[idx].Parameters)
            {
                var value = p.Accessor(expr, parameters);

                var enumerable = value as IEnumerable;
                if (enumerable != null)
                {
                    var type = enumerable.GetType();
                    var etype = type.GetItemType();

                    if (etype == null || etype == typeof(object) || etype.IsEnumEx() ||
                        (type.IsGenericTypeEx() && type.GetGenericTypeDefinition() == typeof(Nullable<>) &&
                         etype.GetGenericArgumentsEx()[0].IsEnumEx()))
                    {
                        var values = new List<object>();

                        foreach (var v in enumerable)
                        {
                            value = v;

                            if (v != null)
                            {
                                var valueType = v.GetType();

                                if (valueType.ToNullableUnderlying().IsEnumEx())
                                {
                                    if (_enumConverters == null)
                                        _enumConverters = new ConcurrentDictionary<Type, Func<object, object>>();

                                    Func<object, object> converter;

                                    if (!_enumConverters.TryGetValue(valueType, out converter))
                                    {
                                        var toType = Converter.GetDefaultMappingFromEnumType(MappingSchema, valueType);
                                        var convExpr = MappingSchema.GetConvertExpression(valueType, toType);
                                        var convParam = Expression.Parameter(typeof(object));

                                        var lex = Expression.Lambda<Func<object, object>>(
                                            Expression.Convert(
                                                convExpr.GetBody(Expression.Convert(convParam, valueType)),
                                                typeof(object)),
                                            convParam);

                                        converter = lex.Compile();
                                    }

                                    value = converter(v);
                                }
                            }

                            values.Add(value);
                        }

                        value = values;
                    }
                }

                p.SqlParameter.Value = value;
            }
        }

        #endregion

        #region Object Operations

        private static class ObjectOperation<T1>
        {
            public static readonly Dictionary<object, Query<int>> Insert = new Dictionary<object, Query<int>>();

            public static readonly Dictionary<object, Query<object>> InsertWithIdentity =
                new Dictionary<object, Query<object>>();

            public static readonly Dictionary<object, Query<int>> InsertOrUpdate = new Dictionary<object, Query<int>>();
            public static readonly Dictionary<object, Query<int>> Update = new Dictionary<object, Query<int>>();
            public static readonly Dictionary<object, Query<int>> Delete = new Dictionary<object, Query<int>>();
        }

        private static ParameterAccessor GetParameter(IDataContext dataContext, ISqlField field)
        {
            var exprParam = Expression.Parameter(typeof(Expression), "expr");

            Expression getter = Expression.Convert(
                Expression.Property(
                    Expression.Convert(exprParam, typeof(ConstantExpression)),
                    ReflectionHelper.Constant.Value),
                typeof(T));

            var members = field.Name.Split('.');
            var defValue = Expression.Constant(dataContext.MappingSchema.GetDefaultValue(field.SystemType),
                field.SystemType);

            for (var i = 0; i < members.Length; i++)
            {
                var member = members[i];
                Expression pof = Expression.PropertyOrField(getter, member);

                getter = i == 0
                    ? pof
                    : Expression.Condition(Expression.Equal(getter, Expression.Constant(null)), defValue, pof);
            }

            var expr = dataContext.MappingSchema.GetConvertExpression(field.SystemType, typeof(DataParameter),
                createDefault: false);

            if (expr != null)
                getter = Expression.PropertyOrField(expr.GetBody(getter), "Value");

            var param = ExpressionBuilder.CreateParameterAccessor(
                dataContext, getter, getter, exprParam, Expression.Parameter(typeof(object[]), "ps"),
                field.Name.Replace('.', '_'));

            return param;
        }

        #region Insert

        public static int Insert(
            IDataContextInfo dataContextInfo, T obj,
            string tableName = null, string databaseName = null, string schemaName = null)
        {
            if (Equals(default(T), obj))
                return 0;

            Query<int> ei;

            var key = new {dataContextInfo.MappingSchema, dataContextInfo.ContextID};

            if (!ObjectOperation<T>.Insert.TryGetValue(key, out ei))
                if (!ObjectOperation<T>.Insert.TryGetValue(key, out ei))
                {
                    var sqlTable = new SqlTable<T>(dataContextInfo.MappingSchema);
                    var sqlQuery = new SelectQuery {EQueryType = EQueryType.Insert};

                    if (tableName != null) sqlTable.PhysicalName = tableName;
                    if (databaseName != null) sqlTable.Database = databaseName;
                    if (schemaName != null) sqlTable.Owner = schemaName;

                    sqlQuery.Insert.Into = sqlTable;

                    ei = new Query<int>
                    {
                        MappingSchema = dataContextInfo.MappingSchema,
                        ContextID = dataContextInfo.ContextID,
                        SqlOptimizer = dataContextInfo.GetSqlOptimizer(),
                        Queries = {new QueryInfo {SelectQuery = sqlQuery}}
                    };

                    foreach (var field in sqlTable.Fields)
                    {
                        if (field.Value.IsInsertable)
                        {
                            var param = GetParameter(dataContextInfo.DataContext, field.Value);

                            ei.Queries[0].Parameters.Add(param);

                            sqlQuery.Insert.Items.AddLast(new SetExpression(field.Value, param.SqlParameter));
                        }
                        else if (field.Value.IsIdentity)
                        {
                            var sqlb = dataContextInfo.CreateSqlBuilder();
                            var expr = sqlb.GetIdentityExpression(sqlTable);

                            if (expr != null)
                                sqlQuery.Insert.Items.AddLast(new SetExpression(field.Value, expr));
                        }
                    }

                    ei.SetNonQueryQuery();

                    ObjectOperation<T>.Insert.Add(key, ei);
                }

            return (int) ei.GetElement(null, dataContextInfo, Expression.Constant(obj), null);
        }

        #endregion

        #region InsertWithIdentity

        public static object InsertWithIdentity(IDataContextInfo dataContextInfo, T obj)
        {
            if (Equals(default(T), obj))
                return 0;

            Query<object> ei;

            var key = new {dataContextInfo.MappingSchema, dataContextInfo.ContextID};

            if (!ObjectOperation<T>.InsertWithIdentity.TryGetValue(key, out ei))
                if (!ObjectOperation<T>.InsertWithIdentity.TryGetValue(key, out ei))
                {
                    var sqlTable = new SqlTable<T>(dataContextInfo.MappingSchema);
                    var sqlQuery = new SelectQuery {EQueryType = EQueryType.Insert};

                    sqlQuery.Insert.Into = sqlTable;
                    sqlQuery.Insert.WithIdentity = true;

                    ei = new Query<object>
                    {
                        MappingSchema = dataContextInfo.MappingSchema,
                        ContextID = dataContextInfo.ContextID,
                        SqlOptimizer = dataContextInfo.GetSqlOptimizer(),
                        Queries = {new QueryInfo {SelectQuery = sqlQuery}}
                    };

                    foreach (var field in sqlTable.Fields)
                    {
                        if (field.Value.IsInsertable)
                        {
                            var param = GetParameter(dataContextInfo.DataContext, field.Value);

                            ei.Queries[0].Parameters.Add(param);

                            sqlQuery.Insert.Items.AddLast(new SetExpression(field.Value, param.SqlParameter));
                        }
                        else if (field.Value.IsIdentity)
                        {
                            var sqlb = dataContextInfo.CreateSqlBuilder();
                            var expr = sqlb.GetIdentityExpression(sqlTable);

                            if (expr != null)
                                sqlQuery.Insert.Items.AddLast(new SetExpression(field.Value, expr));
                        }
                    }

                    ei.SetScalarQuery<object>();

                    ObjectOperation<T>.InsertWithIdentity.Add(key, ei);
                }

            return ei.GetElement(null, dataContextInfo, Expression.Constant(obj), null);
        }

        #endregion

        #region InsertOrReplace

        public static int InsertOrReplace(IDataContextInfo dataContextInfo, T obj)
        {
            if (Equals(default(T), obj))
                return 0;

            Query<int> ei;

            var key = new {dataContextInfo.MappingSchema, dataContextInfo.ContextID};

            if (!ObjectOperation<T>.InsertOrUpdate.TryGetValue(key, out ei))
            {
                if (!ObjectOperation<T>.InsertOrUpdate.TryGetValue(key, out ei))
                {
                    var fieldDic = new Dictionary<ISqlField, ParameterAccessor>();
                    var sqlTable = new SqlTable<T>(dataContextInfo.MappingSchema);
                    var sqlQuery = new SelectQuery {EQueryType = EQueryType.InsertOrUpdate};

                    ParameterAccessor param;

                    sqlQuery.Insert.Into = sqlTable;
                    sqlQuery.Update.Table = sqlTable;

                    sqlQuery.From.Table(sqlTable);

                    ei = new Query<int>
                    {
                        MappingSchema = dataContextInfo.MappingSchema,
                        ContextID = dataContextInfo.ContextID,
                        SqlOptimizer = dataContextInfo.GetSqlOptimizer(),
                        Queries = {new QueryInfo {SelectQuery = sqlQuery}},
                        SqlProviderFlags = dataContextInfo.SqlProviderFlags
                    };

                    var supported = ei.SqlProviderFlags.IsInsertOrUpdateSupported &&
                                    ei.SqlProviderFlags.CanCombineParameters;

                    // Insert.
                    //
                    foreach (var field in sqlTable.Fields.Select(f => f.Value))
                    {
                        if (field.IsInsertable)
                        {
                            if (!supported || !fieldDic.TryGetValue(field, out param))
                            {
                                param = GetParameter(dataContextInfo.DataContext, field);
                                ei.Queries[0].Parameters.Add(param);

                                if (supported)
                                    fieldDic.Add(field, param);
                            }

                            sqlQuery.Insert.Items.AddLast(new SetExpression(field, param.SqlParameter));
                        }
                        else if (field.IsIdentity)
                        {
                            throw new LinqException("InsertOrUpdate method does not support identity field '{0}.{1}'.",
                                sqlTable.Name, field.Name);
                        }
                    }

                    // Update.
                    //
                    var keys = sqlTable.GetKeys(true).Cast<ISqlField>().ToList();
                    var fields = sqlTable.Fields.Values.Where(f => f.IsUpdatable).Except(keys).ToList();

                    if (keys.Count == 0)
                        throw new LinqException(
                            "InsertOrUpdate method requires the '{0}' table to have a primary key.", sqlTable.Name);

                    var q =
                        (
                            from k in keys
                            join i in sqlQuery.Insert.Items on k equals i.Column
                            select new {k, i}
                            ).ToList();

                    var missedKey = keys.Except(q.Select(i => i.k)).FirstOrDefault();

                    if (missedKey != null)
                        throw new LinqException(
                            "InsertOrUpdate method requires the '{0}.{1}' field to be included in the insert setter.",
                            sqlTable.Name,
                            missedKey.Name);

                    if (fields.Count == 0)
                        throw new LinqException("There are no fields to update in the type '{0}'.", sqlTable.Name);

                    foreach (var field in fields)
                    {
                        if (!supported || !fieldDic.TryGetValue(field, out param))
                        {
                            param = GetParameter(dataContextInfo.DataContext, field);
                            ei.Queries[0].Parameters.Add(param);

                            if (supported)
                                fieldDic.Add(field, param = GetParameter(dataContextInfo.DataContext, field));
                        }

                        sqlQuery.Update.Items.AddLast(new SetExpression(field, param.SqlParameter));
                    }

                    for (var index = 0; index < q.Count; index++)
                    {
                        sqlQuery.Update.Keys.AddLast(q[index].i);
                    }

                    // Set the query.
                    //
                    if (ei.SqlProviderFlags.IsInsertOrUpdateSupported)
                        ei.SetNonQueryQuery();
                    else
                        ei.MakeAlternativeInsertOrUpdate(sqlQuery);

                    ObjectOperation<T>.InsertOrUpdate.Add(key, ei);
                }
            }

            return (int) ei.GetElement(null, dataContextInfo, Expression.Constant(obj), null);
        }

        internal void MakeAlternativeInsertOrUpdate(ISelectQuery selectQuery)
        {
            var dic = new Dictionary<ICloneableElement, ICloneableElement>();

            var insertQuery = (ISelectQuery) selectQuery.Clone(dic, _ => true);

            insertQuery.EQueryType = EQueryType.Insert;
            insertQuery.ClearUpdate();
            insertQuery.From.Tables.Clear();

            Queries.Add(new QueryInfo
            {
                SelectQuery = insertQuery,
                Parameters = Queries[0].Parameters
                    .Select(p => new ParameterAccessor
                    {
                        Expression = p.Expression,
                        Accessor = p.Accessor,
                        SqlParameter = dic.ContainsKey(p.SqlParameter) ? (ISqlParameter) dic[p.SqlParameter] : null
                    })
                    .Where(p => p.SqlParameter != null)
                    .ToList()
            });

            var keys = selectQuery.Update.Keys;

            foreach (var key in keys)
                selectQuery.Where.Expr(key.Column).Equal.Expr(key.Expression);

            selectQuery.EQueryType = EQueryType.Update;
            selectQuery.ClearInsert();

            SetNonQueryQuery2();

            Queries.Add(new QueryInfo
            {
                SelectQuery = insertQuery,
                Parameters = Queries[0].Parameters.ToList()
            });
        }

        #endregion

        #region Update

        public static int Update(IDataContextInfo dataContextInfo, T obj)
        {
            if (Equals(default(T), obj))
                return 0;

            Query<int> ei;

            var key = new {dataContextInfo.MappingSchema, dataContextInfo.ContextID};

            if (!ObjectOperation<T>.Update.TryGetValue(key, out ei))
                if (!ObjectOperation<T>.Update.TryGetValue(key, out ei))
                {
                    var sqlTable = new SqlTable<T>(dataContextInfo.MappingSchema);
                    var sqlQuery = new SelectQuery {EQueryType = EQueryType.Update};

                    sqlQuery.From.Table(sqlTable);

                    ei = new Query<int>
                    {
                        MappingSchema = dataContextInfo.MappingSchema,
                        ContextID = dataContextInfo.ContextID,
                        SqlOptimizer = dataContextInfo.GetSqlOptimizer(),
                        Queries = {new QueryInfo {SelectQuery = sqlQuery}}
                    };

                    var keys = sqlTable.GetKeys(true).Cast<ISqlField>().ToList();
                    var fields = sqlTable.Fields.Values.Where(f => f.IsUpdatable).Except(keys).ToList();

                    if (fields.Count == 0)
                    {
                        if (Common.Configuration.Linq.IgnoreEmptyUpdate)
                            return 0;

                        throw new LinqException(
                            (keys.Count == sqlTable.Fields.Count
                                ? "There are no fields to update in the type '{0}'. No PK is defined or all fields are keys."
                                : "There are no fields to update in the type '{0}'.")
                                .Args(sqlTable.Name));
                    }

                    foreach (var field in fields)
                    {
                        var param = GetParameter(dataContextInfo.DataContext, field);

                        ei.Queries[0].Parameters.Add(param);

                        sqlQuery.Update.Items.AddLast(new SetExpression(field, param.SqlParameter));
                    }

                    foreach (var field in keys)
                    {
                        var param = GetParameter(dataContextInfo.DataContext, field);

                        ei.Queries[0].Parameters.Add(param);

                        sqlQuery.Where.Field(field).Equal.Expr(param.SqlParameter);

                        if (field.Nullable)
                            sqlQuery.IsParameterDependent = true;
                    }

                    ei.SetNonQueryQuery();

                    ObjectOperation<T>.Update.Add(key, ei);
                }

            return (int) ei.GetElement(null, dataContextInfo, Expression.Constant(obj), null);
        }

        #endregion

        #region Delete

        public static int Delete(IDataContextInfo dataContextInfo, T obj)
        {
            if (Equals(default(T), obj))
                return 0;

            Query<int> ei;

            var key = new {dataContextInfo.MappingSchema, dataContextInfo.ContextID};

            if (!ObjectOperation<T>.Delete.TryGetValue(key, out ei))
                if (!ObjectOperation<T>.Delete.TryGetValue(key, out ei))
                {
                    var sqlTable = new SqlTable<T>(dataContextInfo.MappingSchema);
                    var sqlQuery = new SelectQuery {EQueryType = EQueryType.Delete};

                    sqlQuery.From.Table(sqlTable);

                    ei = new Query<int>
                    {
                        MappingSchema = dataContextInfo.MappingSchema,
                        ContextID = dataContextInfo.ContextID,
                        SqlOptimizer = dataContextInfo.GetSqlOptimizer(),
                        Queries = {new QueryInfo {SelectQuery = sqlQuery}}
                    };

                    var keys = sqlTable.GetKeys(true).Cast<ISqlField>().ToList();

                    if (keys.Count == 0)
                        throw new LinqException("Table '{0}' does not have primary key.".Args(sqlTable.Name));

                    foreach (var field in keys)
                    {
                        var param = GetParameter(dataContextInfo.DataContext, field);

                        ei.Queries[0].Parameters.Add(param);

                        sqlQuery.Where.Field(field).Equal.Expr(param.SqlParameter);

                        if (field.Nullable)
                            sqlQuery.IsParameterDependent = true;
                    }

                    ei.SetNonQueryQuery();

                    ObjectOperation<T>.Delete.Add(key, ei);
                }

            return (int) ei.GetElement(null, dataContextInfo, Expression.Constant(obj), null);
        }

        #endregion

        #endregion

        #region DDL Operations

        public static ITable<T> CreateTable(IDataContextInfo dataContextInfo,
            string tableName = null,
            string databaseName = null,
            string schemaName = null,
            string statementHeader = null,
            string statementFooter = null,
            EDefaulNullable eDefaulNullable = EDefaulNullable.None)
        {
            var sqlTable = new SqlTable<T>(dataContextInfo.MappingSchema);
            var sqlQuery = new SelectQuery {EQueryType = EQueryType.CreateTable};

            if (tableName != null) sqlTable.PhysicalName = tableName;
            if (databaseName != null) sqlTable.Database = databaseName;
            if (schemaName != null) sqlTable.Owner = schemaName;

            sqlQuery.CreateTable.Table = sqlTable;
            sqlQuery.CreateTable.StatementHeader = statementHeader;
            sqlQuery.CreateTable.StatementFooter = statementFooter;
            sqlQuery.CreateTable.EDefaulNullable = eDefaulNullable;

            var query = new Query<int>
            {
                MappingSchema = dataContextInfo.MappingSchema,
                ContextID = dataContextInfo.ContextID,
                SqlOptimizer = dataContextInfo.GetSqlOptimizer(),
                Queries = {new QueryInfo {SelectQuery = sqlQuery}}
            };

            query.SetNonQueryQuery();

            query.GetElement(null, dataContextInfo, Expression.Constant(null), null);

            ITable<T> table = new Table<T>(dataContextInfo);

            if (tableName != null) table = table.TableName(tableName);
            if (databaseName != null) table = table.DatabaseName(databaseName);
            if (schemaName != null) table = table.SchemaName(schemaName);

            return table;
        }

        public static void DropTable(IDataContextInfo dataContextInfo,
            string tableName = null,
            string databaseName = null,
            string ownerName = null)
        {
            var sqlTable = new SqlTable<T>(dataContextInfo.MappingSchema);
            var sqlQuery = new SelectQuery {EQueryType = EQueryType.CreateTable};

            if (tableName != null) sqlTable.PhysicalName = tableName;
            if (databaseName != null) sqlTable.Database = databaseName;
            if (ownerName != null) sqlTable.Owner = ownerName;

            sqlQuery.CreateTable.Table = sqlTable;
            sqlQuery.CreateTable.IsDrop = true;

            var query = new Query<int>
            {
                MappingSchema = dataContextInfo.MappingSchema,
                ContextID = dataContextInfo.ContextID,
                SqlOptimizer = dataContextInfo.GetSqlOptimizer(),
                Queries = {new QueryInfo {SelectQuery = sqlQuery}}
            };

            query.SetNonQueryQuery();

            query.GetElement(null, dataContextInfo, Expression.Constant(null), null);
        }

        #endregion

        #region New Builder Support

        public void SetElementQuery(Func<QueryContext, IDataContext, IDataReader, Expression, object[], object> mapper)
        {
            FinalizeQuery();

            if (Queries.Count != 1)
                throw new InvalidOperationException();

            ClearParameters();

            GetElement = (ctx, db, expr, ps) => RunQuery(ctx, db, expr, ps, mapper);
        }

        private TE RunQuery<TE>(
            QueryContext ctx,
            IDataContextInfo dataContextInfo,
            Expression expr,
            object[] parameters,
            Func<QueryContext, IDataContext, IDataReader, Expression, object[], TE> mapper)
        {
            var dataContext = dataContextInfo.DataContext;

            object query = null;

            try
            {
                query = SetCommand(dataContext, expr, parameters, 0, true);

                using (var dr = dataContext.ExecuteReader(query))
                    while (dr.Read())
                        return mapper(ctx, dataContext, dr, expr, parameters);

                return Array<TE>.Empty.First();
            }
            finally
            {
                if (query != null)
                    dataContext.ReleaseQuery(query);

                if (dataContextInfo.DisposeContext)
                    dataContext.Dispose();
            }
        }

        private Func<IDataContextInfo, Expression, object[], int, IEnumerable<IDataReader>> GetQuery()
        {
            FinalizeQuery();

            if (Queries.Count != 1)
                throw new InvalidOperationException();

            Func<IDataContextInfo, Expression, object[], int, IEnumerable<IDataReader>> query = RunQuery;

            var select = Queries[0].SelectQuery.Select;

            if (select.SkipValue != null && !SqlProviderFlags.GetIsSkipSupportedFlag(Queries[0].SelectQuery))
            {
                var q = query;

                var sqlValue = select.SkipValue as ISqlValue;
                if (sqlValue != null)
                {
                    var n = (int) sqlValue.Value;

                    if (n > 0)
                        query = (db, expr, ps, qn) => q(db, expr, ps, qn).Skip(n);
                }
                else if (select.SkipValue is ISqlParameter)
                {
                    var i = GetParameterIndex(select.SkipValue);
                    query =
                        (db, expr, ps, qn) =>
                            q(db, expr, ps, qn).Skip((int) Queries[0].Parameters[i].Accessor(expr, ps));
                }
            }

            if (select.TakeValue != null && !SqlProviderFlags.IsTakeSupported)
            {
                var q = query;

                var sqlValue = select.TakeValue as ISqlValue;
                if (sqlValue != null)
                {
                    var n = (int) sqlValue.Value;

                    if (n > 0)
                        query = (db, expr, ps, qn) => q(db, expr, ps, qn).Take(n);
                }
                else if (select.TakeValue is ISqlParameter)
                {
                    var i = GetParameterIndex(select.TakeValue);
                    query =
                        (db, expr, ps, qn) =>
                            q(db, expr, ps, qn).Take((int) Queries[0].Parameters[i].Accessor(expr, ps));
                }
            }

            return query;
        }

        internal void SetQuery(
            Expression<Func<QueryContext, IDataContext, IDataReader, Expression, object[], T>> expression)
        {
            var query = GetQuery();
            var mapInfo = new MapInfo {Expression = expression};

            if (IsSaveResultMappingIndexes)
            {
                SaveResultMappingIndexes(expression);
            }

            ClearParameters();

            GetIEnumerable = (ctx, db, expr, ps) => Map(query(db, expr, ps, 0), ctx, db, expr, ps, mapInfo);
        }

        public void SaveResultMappingIndexes(Expression expression)
        {
            var findedExpression =
                (NewExpression)
                    expression.Find(
                        baseExpression =>
                            baseExpression.NodeType == ExpressionType.New && baseExpression.Type == typeof(T));

            foreach (var argument in findedExpression.Arguments)
            {
                _resultMappingIndexes.Add(FindIndex(expression, argument));
            }
        }

        private int FindIndex(Expression expression, Expression exp)
        {
            int? idx = null;
            var drExpr = (ConvertFromDataReaderExpression) exp.Find(e => e is ConvertFromDataReaderExpression);
            if (drExpr != null)
            {
                return drExpr.Idx;
            }

            var memberAccess = exp as MemberExpression;
            if (memberAccess != null)
            {
                var memberName = memberAccess.Member.Name;
                memberAccess = memberAccess.Expression as MemberExpression ?? memberAccess;

                var expr =
                    (MemberInitExpression)
                        expression.Find(
                            baseExpression =>
                                baseExpression.NodeType == ExpressionType.MemberInit &&
                                baseExpression.Type == memberAccess.Type);

                var memberAssignments = expr != null
                    ? expr.Bindings.Cast<MemberAssignment>()
                        .Where(memberBinding => memberBinding.Member.Name == memberName)
                    : Enumerable.Empty<MemberAssignment>();

                foreach (var assignment in memberAssignments)
                {
                    idx = FindIndex(expression, assignment.Expression);
                    break;
                }
            }

            return idx ?? 0;
        }

        private class MapInfo
        {
            public Expression<Func<QueryContext, IDataContext, IDataReader, Expression, object[], T>> Expression;
            public Func<QueryContext, IDataContext, IDataReader, Expression, object[], T> Mapper;
            public Expression<Func<QueryContext, IDataContext, IDataReader, Expression, object[], T>> MapperExpression;
        }

        private static IEnumerable<T> Map(
            IEnumerable<IDataReader> data,
            QueryContext queryContext,
            IDataContextInfo dataContextInfo,
            Expression expr,
            object[] ps,
            MapInfo mapInfo)
        {
            var closeQueryContext = false;

            if (queryContext == null)
            {
                closeQueryContext = true;
                queryContext = new QueryContext(dataContextInfo, expr, ps);
            }

            var isFaulted = false;

            foreach (var dr in data)
            {
                var mapper = mapInfo.Mapper;

                if (mapper == null)
                {
                    mapInfo.MapperExpression = mapInfo.Expression.Transform(e =>
                    {
                        var ex = e as ConvertFromDataReaderExpression;
                        return ex != null ? ex.Reduce(dr) : e;
                    }) as Expression<Func<QueryContext, IDataContext, IDataReader, Expression, object[], T>>;

                    // IT : # MapperExpression.Compile()
                    //
                    mapInfo.Mapper = mapper = mapInfo.MapperExpression.Compile();
                }

                T result;

                try
                {
                    result = mapper(queryContext, dataContextInfo.DataContext, dr, expr, ps);
                }
                catch (FormatException)
                {
                    if (isFaulted)
                        throw;

                    isFaulted = true;

                    mapInfo.Mapper = mapInfo.Expression.Compile();
                    result = mapInfo.Mapper(queryContext, dataContextInfo.DataContext, dr, expr, ps);
                }
                catch (InvalidCastException)
                {
                    if (isFaulted)
                        throw;

                    isFaulted = true;

                    mapInfo.Mapper = mapInfo.Expression.Compile();
                    result = mapInfo.Mapper(queryContext, dataContextInfo.DataContext, dr, expr, ps);
                }
                finally
                {
                    if (closeQueryContext)
                        queryContext.Close();
                }

                yield return result;
            }
        }

        internal void SetQuery(
            Expression<Func<QueryContext, IDataContext, IDataReader, Expression, object[], int, T>> expression)
        {
            var query = GetQuery();
            var mapInfo = new MapInfo2 {Expression = expression};

            ClearParameters();

            GetIEnumerable = (ctx, db, expr, ps) => Map(query(db, expr, ps, 0), ctx, db, expr, ps, mapInfo);
        }

        private class MapInfo2
        {
            public Expression<Func<QueryContext, IDataContext, IDataReader, Expression, object[], int, T>> Expression;
            public Func<QueryContext, IDataContext, IDataReader, Expression, object[], int, T> Mapper;
        }

        private static IEnumerable<T> Map(
            IEnumerable<IDataReader> data,
            QueryContext queryContext,
            IDataContextInfo dataContextInfo,
            Expression expr,
            object[] ps,
            MapInfo2 mapInfo)
        {
            if (queryContext == null)
                queryContext = new QueryContext(dataContextInfo, expr, ps);

            var counter = 0;
            var isFaulted = false;

            foreach (var dr in data)
            {
                var mapper = mapInfo.Mapper;

                if (mapper == null)
                {
                    var mapperExpression = mapInfo.Expression.Transform(e =>
                    {
                        var ex = e as ConvertFromDataReaderExpression;
                        return ex != null ? ex.Reduce(dr) : e;
                    }) as Expression<Func<QueryContext, IDataContext, IDataReader, Expression, object[], int, T>>;

                    mapInfo.Mapper = mapper = mapperExpression.Compile();
                }

                T result;

                try
                {
                    result = mapper(queryContext, dataContextInfo.DataContext, dr, expr, ps, counter);
                }
                catch (FormatException)
                {
                    if (isFaulted)
                        throw;

                    isFaulted = true;

                    mapInfo.Mapper = mapInfo.Expression.Compile();
                    result = mapInfo.Mapper(queryContext, dataContextInfo.DataContext, dr, expr, ps, counter);
                }
                catch (InvalidCastException)
                {
                    if (isFaulted)
                        throw;

                    isFaulted = true;

                    mapInfo.Mapper = mapInfo.Expression.Compile();
                    result = mapInfo.Mapper(queryContext, dataContextInfo.DataContext, dr, expr, ps, counter);
                }

                counter++;

                yield return result;
            }
        }

        #endregion
    }

    public class ParameterAccessor
    {
        public Func<Expression, object[], object> Accessor;
        public Expression Expression;
        public ISqlParameter SqlParameter;
    }
}