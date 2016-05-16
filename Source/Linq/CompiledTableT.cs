using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Bars2Db.Linq.Builder;
using Bars2Db.Mapping;

namespace Bars2Db.Linq
{
    internal class CompiledTable<T>
    {
        private readonly Expression _expression;

        private readonly Dictionary<object, Query<T>> _infos = new Dictionary<object, Query<T>>();

        private readonly LambdaExpression _lambda;
        private readonly object _sync = new object();

        private string _lastContextID;
        private MappingSchema _lastMappingSchema;
        private Query<T> _lastQuery;

        public CompiledTable(LambdaExpression lambda, Expression expression)
        {
            _lambda = lambda;
            _expression = expression;
        }

        private Query<T> GetInfo(IDataContext dataContext)
        {
            var dataContextInfo = DataContextInfo.Create(dataContext);

            string lastContextID;
            MappingSchema lastMappingSchema;
            Query<T> query;

            lock (_sync)
            {
                lastContextID = _lastContextID;
                lastMappingSchema = _lastMappingSchema;
                query = _lastQuery;
            }

            var contextID = dataContextInfo.ContextID;
            var mappingSchema = dataContextInfo.MappingSchema;

            if (lastContextID != contextID || lastMappingSchema != mappingSchema)
                query = null;

            if (query == null)
            {
                var key = new {contextID, mappingSchema};

                lock (_sync)
                    _infos.TryGetValue(key, out query);

                if (query == null)
                {
                    lock (_sync)
                    {
                        _infos.TryGetValue(key, out query);

                        if (query == null)
                        {
                            query =
                                new ExpressionBuilder(new Query<T>(), dataContextInfo, _expression,
                                    _lambda.Parameters.ToArray())
                                    .Build<T>();

                            _infos.Add(key, query);

                            _lastContextID = contextID;
                            _lastMappingSchema = mappingSchema;
                            _lastQuery = query;
                        }
                    }
                }
            }

            return query;
        }

        public IQueryable<T> Create(object[] parameters)
        {
            var db = (IDataContext) parameters[0];
            return new Table<T>(db, _expression) {Info = GetInfo(db), Parameters = parameters};
        }

        public T Execute(object[] parameters)
        {
            var db = (IDataContext) parameters[0];
            var ctx = DataContextInfo.Create(db);
            var query = GetInfo(db);

            return (T) query.GetElement(null, ctx, _expression, parameters);
        }
    }
}