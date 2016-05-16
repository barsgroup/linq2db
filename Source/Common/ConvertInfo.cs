using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using Bars2Db.Mapping;

namespace Bars2Db.Common
{
    internal class ConvertInfo
    {
        public static ConvertInfo Default = new ConvertInfo();

        private readonly ConcurrentDictionary<Type, ConcurrentDictionary<Type, LambdaInfo>> _expressions =
            new ConcurrentDictionary<Type, ConcurrentDictionary<Type, LambdaInfo>>();

        public void Set(Type from, Type to, LambdaInfo expr)
        {
            Set(_expressions, from, to, expr);
        }

        private static void Set(ConcurrentDictionary<Type, ConcurrentDictionary<Type, LambdaInfo>> expressions,
            Type from, Type to, LambdaInfo expr)
        {
            ConcurrentDictionary<Type, LambdaInfo> dic;

            if (!expressions.TryGetValue(from, out dic))
                expressions[from] = dic = new ConcurrentDictionary<Type, LambdaInfo>();

            dic[to] = expr;
        }

        public LambdaInfo Get(Type from, Type to)
        {
            ConcurrentDictionary<Type, LambdaInfo> dic;
            LambdaInfo li;

            return _expressions.TryGetValue(from, out dic) && dic.TryGetValue(to, out li) ? li : null;
        }

        public LambdaInfo Create(MappingSchema mappingSchema, Type from, Type to)
        {
            var ex = ConvertBuilder.GetConverter(mappingSchema, from, to);
            var lm = ex.Item1.Compile();
            var ret = new LambdaInfo(ex.Item1, ex.Item2, lm, ex.Item3);

            Set(_expressions, from, to, ret);

            return ret;
        }

        public class LambdaInfo
        {
            public LambdaExpression CheckNullLambda;
            public Delegate Delegate;
            public bool IsSchemaSpecific;

            public LambdaExpression Lambda;

            public LambdaInfo(
                LambdaExpression checkNullLambda,
                LambdaExpression lambda,
                Delegate @delegate,
                bool isSchemaSpecific)
            {
                CheckNullLambda = checkNullLambda;
                Lambda = lambda ?? checkNullLambda;
                Delegate = @delegate;
                IsSchemaSpecific = isSchemaSpecific;
            }
        }
    }
}