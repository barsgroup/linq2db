using System;
using System.Linq.Expressions;
using Bars2Db.Expressions;

namespace Bars2Db.Common
{
    public static class ConvertTo<TTo>
    {
        public static TTo From<TFrom>(TFrom o)
        {
            return Convert<TFrom, TTo>.From(o);
        }
    }

    public static class Convert<TFrom, TTo>
    {
        private static Expression<Func<TFrom, TTo>> _expression;

        static Convert()
        {
            Init();
        }

        public static Expression<Func<TFrom, TTo>> Expression
        {
            get { return _expression; }
            set
            {
                var setDefault = _expression != null;

                if (value == null)
                {
                    Init();
                }
                else
                {
                    _expression = value;
                    From = _expression.Compile();
                }

                if (setDefault)
                    ConvertInfo.Default.Set(
                        typeof(TFrom),
                        typeof(TTo),
                        new ConvertInfo.LambdaInfo(_expression, null, From, false));
            }
        }

        public static Func<TFrom, TTo> Lambda
        {
            get { return From; }
            set
            {
                var setDefault = _expression != null;

                if (value == null)
                {
                    Init();
                }
                else
                {
                    var p = System.Linq.Expressions.Expression.Parameter(typeof(TFrom), "p");

                    From = value;
                    _expression =
                        System.Linq.Expressions.Expression.Lambda<Func<TFrom, TTo>>(
                            System.Linq.Expressions.Expression.Invoke(
                                System.Linq.Expressions.Expression.Constant(value),
                                p),
                            p);
                }

                if (setDefault)
                    ConvertInfo.Default.Set(
                        typeof(TFrom),
                        typeof(TTo),
                        new ConvertInfo.LambdaInfo(_expression, null, From, false));
            }
        }

        public static Func<TFrom, TTo> From { get; private set; }

        private static void Init()
        {
            var expr = ConvertBuilder.GetConverter(null, typeof(TFrom), typeof(TTo));

            _expression = (Expression<Func<TFrom, TTo>>) expr.Item1;

            var rexpr =
                (Expression<Func<TFrom, TTo>>) expr.Item1.Transform(e => e is DefaultValueExpression ? e.Reduce() : e);

            From = rexpr.Compile();
        }
    }
}