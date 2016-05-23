using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Bars2Db.Expressions;
using Bars2Db.Linq;
using Bars2Db.Linq.Builder;
using Bars2Db.Linq.Interfaces;
using Bars2Db.Properties;

namespace Bars2Db
{
    public static class LinqExtensions
    {
        private static readonly MethodInfo _tableNameMethodInfo =
            MemberHelper.MethodOf(() => TableName<int>(null, null)).GetGenericMethodDefinition();

        private static readonly MethodInfo _databaseNameMethodInfo =
            MemberHelper.MethodOf(() => DatabaseName<int>(null, null)).GetGenericMethodDefinition();

        private static readonly MethodInfo _ownerNameMethodInfo =
            MemberHelper.MethodOf(() => OwnerName<int>(null, null)).GetGenericMethodDefinition();

        private static readonly MethodInfo _schemaNameMethodInfo =
            MemberHelper.MethodOf(() => SchemaName<int>(null, null)).GetGenericMethodDefinition();

        private static readonly MethodInfo _withTableExpressionMethodInfo =
            MemberHelper.MethodOf(() => WithTableExpression<int>(null, null)).GetGenericMethodDefinition();

        private static readonly MethodInfo _with =
            MemberHelper.MethodOf(() => With<int>(null, null)).GetGenericMethodDefinition();

        private static readonly MethodInfo _loadWithMethodInfo =
            MemberHelper.MethodOf(() => LoadWith<int>(null, null)).GetGenericMethodDefinition();

        private static readonly MethodInfo _deleteMethodInfo =
            MemberHelper.MethodOf(() => Delete<int>(null)).GetGenericMethodDefinition();

        private static readonly MethodInfo _deleteMethodInfo2 =
            MemberHelper.MethodOf(() => Delete<int>(null, null)).GetGenericMethodDefinition();

        private static readonly MethodInfo _updateMethodInfo =
            MemberHelper.MethodOf(() => Update<int, int>(null, null, null)).GetGenericMethodDefinition();

        private static readonly MethodInfo _updateMethodInfo2 =
            MemberHelper.MethodOf(() => Update<int>(null, null)).GetGenericMethodDefinition();

        private static readonly MethodInfo _updateMethodInfo3 =
            MemberHelper.MethodOf(() => Update<int>(null, null, null)).GetGenericMethodDefinition();

        private static readonly MethodInfo _updateMethodInfo4 =
            MemberHelper.MethodOf(() => Update<int>(null)).GetGenericMethodDefinition();

        private static readonly MethodInfo _asUpdatableMethodInfo =
            MemberHelper.MethodOf(() => AsUpdatable<int>(null)).GetGenericMethodDefinition();

        private static readonly MethodInfo _setMethodInfo = MemberHelper.MethodOf(() =>
            Set<int, int>((IQueryable<int>) null, null, (Expression<Func<int, int>>) null)).GetGenericMethodDefinition();

        private static readonly MethodInfo _setMethodInfo2 = MemberHelper.MethodOf(() =>
            Set<int, int>((IUpdatable<int>) null, null, (Expression<Func<int, int>>) null)).GetGenericMethodDefinition();

        private static readonly MethodInfo _setMethodInfo3 = MemberHelper.MethodOf(() =>
            Set<int, int>((IQueryable<int>) null, null, (Expression<Func<int>>) null)).GetGenericMethodDefinition();

        private static readonly MethodInfo _setMethodInfo4 = MemberHelper.MethodOf(() =>
            Set<int, int>((IUpdatable<int>) null, null, (Expression<Func<int>>) null)).GetGenericMethodDefinition();

        private static readonly MethodInfo _setMethodInfo5 =
            MemberHelper.MethodOf(() => Set((IQueryable<int>) null, null, 0)).GetGenericMethodDefinition();

        private static readonly MethodInfo _setMethodInfo6 =
            MemberHelper.MethodOf(() => Set((IUpdatable<int>) null, null, 0)).GetGenericMethodDefinition();

        private static readonly MethodInfo _insertMethodInfo =
            MemberHelper.MethodOf(() => Insert<int>(null, null)).GetGenericMethodDefinition();

        private static readonly MethodInfo _insertWithIdentityMethodInfo =
            MemberHelper.MethodOf(() => InsertWithIdentity<int>(null, null)).GetGenericMethodDefinition();

        private static readonly MethodInfo _intoMethodInfo =
            MemberHelper.MethodOf(() => Into<int>(null, null)).GetGenericMethodDefinition();

        private static readonly MethodInfo _valueMethodInfo =
            MemberHelper.MethodOf(() => Value<int, int>((ITable<int>) null, null, null)).GetGenericMethodDefinition();

        private static readonly MethodInfo _valueMethodInfo2 =
            MemberHelper.MethodOf(() => Value((ITable<int>) null, null, 0)).GetGenericMethodDefinition();

        private static readonly MethodInfo _valueMethodInfo3 =
            MemberHelper.MethodOf(() => Value<int, int>((IValueInsertable<int>) null, null, null))
                .GetGenericMethodDefinition();

        private static readonly MethodInfo _valueMethodInfo4 =
            MemberHelper.MethodOf(() => Value((IValueInsertable<int>) null, null, 0)).GetGenericMethodDefinition();

        private static readonly MethodInfo _insertMethodInfo2 =
            MemberHelper.MethodOf(() => Insert<int>(null)).GetGenericMethodDefinition();

        private static readonly MethodInfo _insertWithIdentityMethodInfo2 =
            MemberHelper.MethodOf(() => InsertWithIdentity<int>(null)).GetGenericMethodDefinition();

        private static readonly MethodInfo _insertMethodInfo3 =
            MemberHelper.MethodOf(() => Insert<int, int>(null, null, null)).GetGenericMethodDefinition();

        private static readonly MethodInfo _insertWithIdentityMethodInfo3 =
            MemberHelper.MethodOf(() => InsertWithIdentity<int, int>(null, null, null)).GetGenericMethodDefinition();

        private static readonly MethodInfo _intoMethodInfo2 =
            MemberHelper.MethodOf(() => Into<int, int>(null, null)).GetGenericMethodDefinition();

        private static readonly MethodInfo _valueMethodInfo5 =
            MemberHelper.MethodOf(() => Value<int, int, int>(null, null, (Expression<Func<int, int>>) null))
                .GetGenericMethodDefinition();

        private static readonly MethodInfo _valueMethodInfo6 =
            MemberHelper.MethodOf(() => Value<int, int, int>(null, null, (Expression<Func<int>>) null))
                .GetGenericMethodDefinition();

        private static readonly MethodInfo _valueMethodInfo7 =
            MemberHelper.MethodOf(() => Value<int, int, int>(null, null, 0)).GetGenericMethodDefinition();

        private static readonly MethodInfo _insertMethodInfo4 =
            MemberHelper.MethodOf(() => Insert<int, int>(null)).GetGenericMethodDefinition();

        private static readonly MethodInfo _insertWithIdentityMethodInfo4 =
            MemberHelper.MethodOf(() => InsertWithIdentity<int, int>(null)).GetGenericMethodDefinition();

        private static readonly MethodInfo _insertOrUpdateMethodInfo =
            MemberHelper.MethodOf(() => InsertOrUpdate<int>(null, null, null)).GetGenericMethodDefinition();

        private static readonly MethodInfo _insertOrUpdateMethodInfo2 =
            MemberHelper.MethodOf(() => InsertOrUpdate<int>(null, null, null, null)).GetGenericMethodDefinition();

        private static readonly MethodInfo _dropMethodInfo2 =
            MemberHelper.MethodOf(() => Drop<int>(null)).GetGenericMethodDefinition();

        private static readonly MethodInfo _takeMethodInfo =
            MemberHelper.MethodOf(() => Take<int>(null, null)).GetGenericMethodDefinition();

        private static readonly MethodInfo _skipMethodInfo =
            MemberHelper.MethodOf(() => Skip<int>(null, null)).GetGenericMethodDefinition();

        private static readonly MethodInfo _elementAtMethodInfo =
            MemberHelper.MethodOf(() => ElementAt<int>(null, null)).GetGenericMethodDefinition();

        private static readonly MethodInfo _elementAtOrDefaultMethodInfo =
            MemberHelper.MethodOf(() => ElementAtOrDefault<int>(null, null)).GetGenericMethodDefinition();

        private static readonly MethodInfo _setMethodInfo7 =
            MemberHelper.MethodOf(() => Having((IQueryable<int>) null, null)).GetGenericMethodDefinition();

        private static readonly MethodInfo _setMethodInfo8 =
            MemberHelper.MethodOf(() => GetContext((IQueryable<int>) null)).GetGenericMethodDefinition();

        #region Scalar Select

        public static T Select<T>(
            [NotNull] this IDataContext dataContext,
            [NotNull, InstantHandle] Expression<Func<T>> selector)
        {
            if (dataContext == null) throw new ArgumentNullException(nameof(dataContext));
            if (selector == null) throw new ArgumentNullException(nameof(selector));

            var q = new Table<T>(dataContext, selector);

            foreach (var item in q)
                return item;

            throw new InvalidOperationException();
        }

        #endregion

        internal static ContextParser.Context GetContext<TSource>(this IQueryable<TSource> source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            return source.Provider.Execute<ContextParser.Context>(
                Expression.Call(
                    null,
                    _setMethodInfo8.MakeGenericMethod(typeof(TSource)), source.Expression));
        }

        #region Stub helpers

        public static TOutput Where<TOutput, TSource, TInput>(this TInput source, Func<TSource, bool> predicate)
        {
            throw new InvalidOperationException();
        }

        #endregion

        #region Table Helpers

        public static ITable<T> TableName<T>([NotNull] this ITable<T> table, [NotNull] string name)
        {
            if (table == null) throw new ArgumentNullException(nameof(table));
            if (name == null) throw new ArgumentNullException(nameof(name));

            table.Expression = Expression.Call(
                null,
                _tableNameMethodInfo.MakeGenericMethod(typeof(T)),
                new[] {table.Expression, Expression.Constant(name)});

            var tbl = table as Table<T>;
            if (tbl != null)
                tbl.TableName = name;

            return table;
        }


        public static ITable<T> DatabaseName<T>([NotNull] this ITable<T> table, [NotNull] string name)
        {
            if (table == null) throw new ArgumentNullException(nameof(table));
            if (name == null) throw new ArgumentNullException(nameof(name));

            table.Expression = Expression.Call(
                null,
                _databaseNameMethodInfo.MakeGenericMethod(typeof(T)),
                new[] {table.Expression, Expression.Constant(name)});

            var tbl = table as Table<T>;
            if (tbl != null)
                tbl.DatabaseName = name;

            return table;
        }


        public static ITable<T> OwnerName<T>([NotNull] this ITable<T> table, [NotNull] string name)
        {
            if (table == null) throw new ArgumentNullException(nameof(table));
            if (name == null) throw new ArgumentNullException(nameof(name));

            table.Expression = Expression.Call(
                null,
                _ownerNameMethodInfo.MakeGenericMethod(typeof(T)),
                new[] {table.Expression, Expression.Constant(name)});

            var tbl = table as Table<T>;
            if (tbl != null)
                tbl.SchemaName = name;

            return table;
        }


        public static ITable<T> SchemaName<T>([NotNull] this ITable<T> table, [NotNull] string name)
        {
            if (table == null) throw new ArgumentNullException(nameof(table));
            if (name == null) throw new ArgumentNullException(nameof(name));

            table.Expression = Expression.Call(
                null,
                _schemaNameMethodInfo.MakeGenericMethod(typeof(T)),
                new[] {table.Expression, Expression.Constant(name)});

            var tbl = table as Table<T>;
            if (tbl != null)
                tbl.SchemaName = name;

            return table;
        }


        public static ITable<T> WithTableExpression<T>([NotNull] this ITable<T> table, [NotNull] string expression)
        {
            if (expression == null) throw new ArgumentNullException(nameof(expression));

            table.Expression = Expression.Call(
                null,
                _withTableExpressionMethodInfo.MakeGenericMethod(typeof(T)),
                new[] {table.Expression, Expression.Constant(expression)});

            return table;
        }


        public static ITable<T> With<T>([NotNull] this ITable<T> table, [NotNull] string args)
        {
            if (args == null) throw new ArgumentNullException(nameof(args));

            table.Expression = Expression.Call(
                null,
                _with.MakeGenericMethod(typeof(T)),
                new[] {table.Expression, Expression.Constant(args)});

            return table;
        }

        #endregion

        #region LoadWith

        public static ITable<T> LoadWith<T>(
            [NotNull] this ITable<T> table,
            [NotNull, InstantHandle] Expression<Func<T, object>> selector)
        {
            var expressionQuery = (IExpressionQuery<T>) table;
            return (ITable<T>) expressionQuery.LoadWith(selector);
        }

        public static IQueryable<T> LoadWith<T>(
            [NotNull] this IExpressionQuery<T> table,
            [NotNull, InstantHandle] Expression<Func<T, object>> selector)
        {
            if (table == null) throw new ArgumentNullException(nameof(table));

            table.Expression = GetExpressionLoadWith(table.Expression, selector);

            return table;
        }

        public static MethodCallExpression GetExpressionLoadWith<T>(Expression expression,
            Expression<Func<T, object>> selector)
        {
            return Expression.Call(
                null,
                _loadWithMethodInfo.MakeGenericMethod(typeof(T)),
                new[] {expression, Expression.Quote(selector)});
        }

        #endregion

        #region Delete

        public static int Delete<T>([NotNull] this IQueryable<T> source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            return source.Provider.Execute<int>(
                Expression.Call(
                    null,
                    _deleteMethodInfo.MakeGenericMethod(typeof(T)), source.Expression));
        }


        public static int Delete<T>(
            [NotNull] this IQueryable<T> source,
            [NotNull, InstantHandle] Expression<Func<T, bool>> predicate)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));

            return source.Provider.Execute<int>(
                Expression.Call(
                    null,
                    _deleteMethodInfo2.MakeGenericMethod(typeof(T)),
                    new[] {source.Expression, Expression.Quote(predicate)}));
        }

        #endregion

        #region Update

        public static int Update<TSource, TTarget>(
            [NotNull] this IQueryable<TSource> source,
            [NotNull] ITable<TTarget> target,
            [NotNull, InstantHandle] Expression<Func<TSource, TTarget>> setter)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (target == null) throw new ArgumentNullException(nameof(target));
            if (setter == null) throw new ArgumentNullException(nameof(setter));

            return source.Provider.Execute<int>(
                Expression.Call(
                    null,
                    _updateMethodInfo.MakeGenericMethod(typeof(TSource), typeof(TTarget)),
                    new[] {source.Expression, ((IQueryable<TTarget>) target).Expression, Expression.Quote(setter)}));
        }


        public static int Update<T>(
            [NotNull] this IQueryable<T> source,
            [NotNull, InstantHandle] Expression<Func<T, T>> setter)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (setter == null) throw new ArgumentNullException(nameof(setter));

            return source.Provider.Execute<int>(
                Expression.Call(
                    null,
                    _updateMethodInfo2.MakeGenericMethod(typeof(T)),
                    new[] {source.Expression, Expression.Quote(setter)}));
        }


        public static int Update<T>(
            [NotNull] this IQueryable<T> source,
            [NotNull, InstantHandle] Expression<Func<T, bool>> predicate,
            [NotNull, InstantHandle] Expression<Func<T, T>> setter)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));
            if (setter == null) throw new ArgumentNullException(nameof(setter));

            return source.Provider.Execute<int>(
                Expression.Call(
                    null,
                    _updateMethodInfo3.MakeGenericMethod(typeof(T)),
                    new[] {source.Expression, Expression.Quote(predicate), Expression.Quote(setter)}));
        }


        public static int Update<T>([NotNull] this IUpdatable<T> source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            var query = ((Updatable<T>) source).Query;

            return query.Provider.Execute<int>(
                Expression.Call(
                    null,
                    _updateMethodInfo4.MakeGenericMethod(typeof(T)), query.Expression));
        }

        private class Updatable<T> : IUpdatable<T>
        {
            public IQueryable<T> Query;
        }


        public static IUpdatable<T> AsUpdatable<T>([NotNull] this IQueryable<T> source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            var query = source.Provider.CreateQuery<T>(
                Expression.Call(
                    null,
                    _asUpdatableMethodInfo.MakeGenericMethod(typeof(T)), source.Expression));

            return new Updatable<T> {Query = query};
        }


        public static IUpdatable<T> Set<T, TV>(
            [NotNull] this IQueryable<T> source,
            [NotNull, InstantHandle] Expression<Func<T, TV>> extract,
            [NotNull, InstantHandle] Expression<Func<T, TV>> update)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (extract == null) throw new ArgumentNullException(nameof(extract));
            if (update == null) throw new ArgumentNullException(nameof(update));

            var query = source.Provider.CreateQuery<T>(
                Expression.Call(
                    null,
                    _setMethodInfo.MakeGenericMethod(typeof(T), typeof(TV)),
                    new[] {source.Expression, Expression.Quote(extract), Expression.Quote(update)}));

            return new Updatable<T> {Query = query};
        }


        public static IUpdatable<T> Set<T, TV>(
            [NotNull] this IUpdatable<T> source,
            [NotNull, InstantHandle] Expression<Func<T, TV>> extract,
            [NotNull, InstantHandle] Expression<Func<T, TV>> update)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (extract == null) throw new ArgumentNullException(nameof(extract));
            if (update == null) throw new ArgumentNullException(nameof(update));

            var query = ((Updatable<T>) source).Query;

            query = query.Provider.CreateQuery<T>(
                Expression.Call(
                    null,
                    _setMethodInfo2.MakeGenericMethod(typeof(T), typeof(TV)),
                    new[] {query.Expression, Expression.Quote(extract), Expression.Quote(update)}));

            return new Updatable<T> {Query = query};
        }


        public static IUpdatable<T> Set<T, TV>(
            [NotNull] this IQueryable<T> source,
            [NotNull, InstantHandle] Expression<Func<T, TV>> extract,
            [NotNull, InstantHandle] Expression<Func<TV>> update)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (extract == null) throw new ArgumentNullException(nameof(extract));
            if (update == null) throw new ArgumentNullException(nameof(update));

            var query = source.Provider.CreateQuery<T>(
                Expression.Call(
                    null,
                    _setMethodInfo3.MakeGenericMethod(typeof(T), typeof(TV)),
                    new[] {source.Expression, Expression.Quote(extract), Expression.Quote(update)}));

            return new Updatable<T> {Query = query};
        }


        public static IUpdatable<T> Set<T, TV>(
            [NotNull] this IUpdatable<T> source,
            [NotNull, InstantHandle] Expression<Func<T, TV>> extract,
            [NotNull, InstantHandle] Expression<Func<TV>> update)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (extract == null) throw new ArgumentNullException(nameof(extract));
            if (update == null) throw new ArgumentNullException(nameof(update));

            var query = ((Updatable<T>) source).Query;

            query = query.Provider.CreateQuery<T>(
                Expression.Call(
                    null,
                    _setMethodInfo4.MakeGenericMethod(typeof(T), typeof(TV)),
                    new[] {query.Expression, Expression.Quote(extract), Expression.Quote(update)}));

            return new Updatable<T> {Query = query};
        }


        public static IUpdatable<T> Set<T, TV>(
            [NotNull] this IQueryable<T> source,
            [NotNull, InstantHandle] Expression<Func<T, TV>> extract,
            TV value)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (extract == null) throw new ArgumentNullException(nameof(extract));

            var query = source.Provider.CreateQuery<T>(
                Expression.Call(
                    null,
                    _setMethodInfo5.MakeGenericMethod(typeof(T), typeof(TV)),
                    new[] {source.Expression, Expression.Quote(extract), Expression.Constant(value, typeof(TV))}));

            return new Updatable<T> {Query = query};
        }


        public static IUpdatable<T> Set<T, TV>(
            [NotNull] this IUpdatable<T> source,
            [NotNull, InstantHandle] Expression<Func<T, TV>> extract,
            TV value)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (extract == null) throw new ArgumentNullException(nameof(extract));

            var query = ((Updatable<T>) source).Query;

            query = query.Provider.CreateQuery<T>(
                Expression.Call(
                    null,
                    _setMethodInfo6.MakeGenericMethod(typeof(T), typeof(TV)),
                    new[] {query.Expression, Expression.Quote(extract), Expression.Constant(value, typeof(TV))}));

            return new Updatable<T> {Query = query};
        }

        #endregion

        #region Insert

        public static int Insert<T>(
            [NotNull] this ITable<T> target,
            [NotNull, InstantHandle] Expression<Func<T>> setter)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));
            if (setter == null) throw new ArgumentNullException(nameof(setter));

            IQueryable<T> query = target;

            return query.Provider.Execute<int>(
                Expression.Call(
                    null,
                    _insertMethodInfo.MakeGenericMethod(typeof(T)),
                    new[] {query.Expression, Expression.Quote(setter)}));
        }


        public static object InsertWithIdentity<T>(
            [NotNull] this ITable<T> target,
            [NotNull, InstantHandle] Expression<Func<T>> setter)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));
            if (setter == null) throw new ArgumentNullException(nameof(setter));

            IQueryable<T> query = target;

            return query.Provider.Execute<object>(
                Expression.Call(
                    null,
                    _insertWithIdentityMethodInfo.MakeGenericMethod(typeof(T)),
                    new[] {query.Expression, Expression.Quote(setter)}));
        }

        #region ValueInsertable

        private class ValueInsertable<T> : IValueInsertable<T>
        {
            public IQueryable<T> Query;
        }


        public static IValueInsertable<T> Into<T>(this IDataContext dataContext, [NotNull] ITable<T> target)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));

            IQueryable<T> query = target;

            var q = query.Provider.CreateQuery<T>(
                Expression.Call(
                    null,
                    _intoMethodInfo.MakeGenericMethod(typeof(T)),
                    new[] {Expression.Constant(null, typeof(IDataContext)), query.Expression}));

            return new ValueInsertable<T> {Query = q};
        }


        public static IValueInsertable<T> Value<T, TV>(
            [NotNull] this ITable<T> source,
            [NotNull, InstantHandle] Expression<Func<T, TV>> field,
            [NotNull, InstantHandle] Expression<Func<TV>> value)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (field == null) throw new ArgumentNullException(nameof(field));
            if (value == null) throw new ArgumentNullException(nameof(value));

            var query = (IQueryable<T>) source;

            var q = query.Provider.CreateQuery<T>(
                Expression.Call(
                    null,
                    _valueMethodInfo.MakeGenericMethod(typeof(T), typeof(TV)),
                    new[] {query.Expression, Expression.Quote(field), Expression.Quote(value)}));

            return new ValueInsertable<T> {Query = q};
        }


        public static IValueInsertable<T> Value<T, TV>(
            [NotNull] this ITable<T> source,
            [NotNull, InstantHandle] Expression<Func<T, TV>> field,
            TV value)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (field == null) throw new ArgumentNullException(nameof(field));

            var query = (IQueryable<T>) source;

            var q = query.Provider.CreateQuery<T>(
                Expression.Call(
                    null,
                    _valueMethodInfo2.MakeGenericMethod(typeof(T), typeof(TV)),
                    new[] {query.Expression, Expression.Quote(field), Expression.Constant(value, typeof(TV))}));

            return new ValueInsertable<T> {Query = q};
        }


        public static IValueInsertable<T> Value<T, TV>(
            [NotNull] this IValueInsertable<T> source,
            [NotNull, InstantHandle] Expression<Func<T, TV>> field,
            [NotNull, InstantHandle] Expression<Func<TV>> value)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (field == null) throw new ArgumentNullException(nameof(field));
            if (value == null) throw new ArgumentNullException(nameof(value));

            var query = ((ValueInsertable<T>) source).Query;

            var q = query.Provider.CreateQuery<T>(
                Expression.Call(
                    null,
                    _valueMethodInfo3.MakeGenericMethod(typeof(T), typeof(TV)),
                    new[] {query.Expression, Expression.Quote(field), Expression.Quote(value)}));

            return new ValueInsertable<T> {Query = q};
        }


        public static IValueInsertable<T> Value<T, TV>(
            [NotNull] this IValueInsertable<T> source,
            [NotNull, InstantHandle] Expression<Func<T, TV>> field,
            TV value)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (field == null) throw new ArgumentNullException(nameof(field));

            var query = ((ValueInsertable<T>) source).Query;

            var q = query.Provider.CreateQuery<T>(
                Expression.Call(
                    null,
                    _valueMethodInfo4.MakeGenericMethod(typeof(T), typeof(TV)),
                    new[] {query.Expression, Expression.Quote(field), Expression.Constant(value, typeof(TV))}));

            return new ValueInsertable<T> {Query = q};
        }


        public static int Insert<T>([NotNull] this IValueInsertable<T> source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            var query = ((ValueInsertable<T>) source).Query;

            return query.Provider.Execute<int>(
                Expression.Call(
                    null,
                    _insertMethodInfo2.MakeGenericMethod(typeof(T)), query.Expression));
        }


        public static object InsertWithIdentity<T>([NotNull] this IValueInsertable<T> source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            var query = ((ValueInsertable<T>) source).Query;

            return query.Provider.Execute<object>(
                Expression.Call(
                    null,
                    _insertWithIdentityMethodInfo2.MakeGenericMethod(typeof(T)), query.Expression));
        }

        #endregion

        #region SelectInsertable

        public static int Insert<TSource, TTarget>(
            [NotNull] this IQueryable<TSource> source,
            [NotNull] ITable<TTarget> target,
            [NotNull, InstantHandle] Expression<Func<TSource, TTarget>> setter)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (target == null) throw new ArgumentNullException(nameof(target));
            if (setter == null) throw new ArgumentNullException(nameof(setter));

            return source.Provider.Execute<int>(
                Expression.Call(
                    null,
                    _insertMethodInfo3.MakeGenericMethod(typeof(TSource), typeof(TTarget)),
                    new[] {source.Expression, ((IQueryable<TTarget>) target).Expression, Expression.Quote(setter)}));
        }


        public static object InsertWithIdentity<TSource, TTarget>(
            [NotNull] this IQueryable<TSource> source,
            [NotNull] ITable<TTarget> target,
            [NotNull, InstantHandle] Expression<Func<TSource, TTarget>> setter)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (target == null) throw new ArgumentNullException(nameof(target));
            if (setter == null) throw new ArgumentNullException(nameof(setter));

            return source.Provider.Execute<object>(
                Expression.Call(
                    null,
                    _insertWithIdentityMethodInfo3.MakeGenericMethod(typeof(TSource), typeof(TTarget)),
                    new[] {source.Expression, ((IQueryable<TTarget>) target).Expression, Expression.Quote(setter)}));
        }

        private class SelectInsertable<T, TT> : ISelectInsertable<T, TT>
        {
            public IQueryable<T> Query;
        }


        public static ISelectInsertable<TSource, TTarget> Into<TSource, TTarget>(
            [NotNull] this IQueryable<TSource> source,
            [NotNull] ITable<TTarget> target)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));

            var q = source.Provider.CreateQuery<TSource>(
                Expression.Call(
                    null,
                    _intoMethodInfo2.MakeGenericMethod(typeof(TSource), typeof(TTarget)),
                    new[] {source.Expression, ((IQueryable<TTarget>) target).Expression}));

            return new SelectInsertable<TSource, TTarget> {Query = q};
        }


        public static ISelectInsertable<TSource, TTarget> Value<TSource, TTarget, TValue>(
            [NotNull] this ISelectInsertable<TSource, TTarget> source,
            [NotNull, InstantHandle] Expression<Func<TTarget, TValue>> field,
            [NotNull, InstantHandle] Expression<Func<TSource, TValue>> value)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (field == null) throw new ArgumentNullException(nameof(field));
            if (value == null) throw new ArgumentNullException(nameof(value));

            var query = ((SelectInsertable<TSource, TTarget>) source).Query;

            var q = query.Provider.CreateQuery<TSource>(
                Expression.Call(
                    null,
                    _valueMethodInfo5.MakeGenericMethod(typeof(TSource), typeof(TTarget), typeof(TValue)),
                    new[] {query.Expression, Expression.Quote(field), Expression.Quote(value)}));

            return new SelectInsertable<TSource, TTarget> {Query = q};
        }


        public static ISelectInsertable<TSource, TTarget> Value<TSource, TTarget, TValue>(
            [NotNull] this ISelectInsertable<TSource, TTarget> source,
            [NotNull, InstantHandle] Expression<Func<TTarget, TValue>> field,
            [NotNull, InstantHandle] Expression<Func<TValue>> value)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (field == null) throw new ArgumentNullException(nameof(field));
            if (value == null) throw new ArgumentNullException(nameof(value));

            var query = ((SelectInsertable<TSource, TTarget>) source).Query;

            var q = query.Provider.CreateQuery<TSource>(
                Expression.Call(
                    null,
                    _valueMethodInfo6.MakeGenericMethod(typeof(TSource), typeof(TTarget), typeof(TValue)),
                    new[] {query.Expression, Expression.Quote(field), Expression.Quote(value)}));

            return new SelectInsertable<TSource, TTarget> {Query = q};
        }


        public static ISelectInsertable<TSource, TTarget> Value<TSource, TTarget, TValue>(
            [NotNull] this ISelectInsertable<TSource, TTarget> source,
            [NotNull, InstantHandle] Expression<Func<TTarget, TValue>> field,
            TValue value)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (field == null) throw new ArgumentNullException(nameof(field));

            var query = ((SelectInsertable<TSource, TTarget>) source).Query;

            var q = query.Provider.CreateQuery<TSource>(
                Expression.Call(
                    null,
                    _valueMethodInfo7.MakeGenericMethod(typeof(TSource), typeof(TTarget), typeof(TValue)),
                    new[] {query.Expression, Expression.Quote(field), Expression.Constant(value, typeof(TValue))}));

            return new SelectInsertable<TSource, TTarget> {Query = q};
        }


        public static int Insert<TSource, TTarget>([NotNull] this ISelectInsertable<TSource, TTarget> source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            var query = ((SelectInsertable<TSource, TTarget>) source).Query;

            return query.Provider.Execute<int>(
                Expression.Call(
                    null,
                    _insertMethodInfo4.MakeGenericMethod(typeof(TSource), typeof(TTarget)), query.Expression));
        }


        public static object InsertWithIdentity<TSource, TTarget>(
            [NotNull] this ISelectInsertable<TSource, TTarget> source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            var query = ((SelectInsertable<TSource, TTarget>) source).Query;

            return query.Provider.Execute<object>(
                Expression.Call(
                    null,
                    _insertWithIdentityMethodInfo4.MakeGenericMethod(typeof(TSource), typeof(TTarget)), query.Expression));
        }

        #endregion

        #endregion

        #region InsertOrUpdate

        public static int InsertOrUpdate<T>(
            [NotNull] this ITable<T> target,
            [NotNull, InstantHandle] Expression<Func<T>> insertSetter,
            [NotNull, InstantHandle] Expression<Func<T, T>> onDuplicateKeyUpdateSetter)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));
            if (insertSetter == null) throw new ArgumentNullException(nameof(insertSetter));
            if (onDuplicateKeyUpdateSetter == null) throw new ArgumentNullException(nameof(onDuplicateKeyUpdateSetter));

            IQueryable<T> query = target;

            return query.Provider.Execute<int>(
                Expression.Call(
                    null,
                    _insertOrUpdateMethodInfo.MakeGenericMethod(typeof(T)),
                    new[]
                    {query.Expression, Expression.Quote(insertSetter), Expression.Quote(onDuplicateKeyUpdateSetter)}));
        }


        public static int InsertOrUpdate<T>(
            [NotNull] this ITable<T> target,
            [NotNull, InstantHandle] Expression<Func<T>> insertSetter,
            [NotNull, InstantHandle] Expression<Func<T, T>> onDuplicateKeyUpdateSetter,
            [NotNull, InstantHandle] Expression<Func<T>> keySelector)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));
            if (insertSetter == null) throw new ArgumentNullException(nameof(insertSetter));
            if (onDuplicateKeyUpdateSetter == null) throw new ArgumentNullException(nameof(onDuplicateKeyUpdateSetter));
            if (keySelector == null) throw new ArgumentNullException(nameof(keySelector));

            IQueryable<T> query = target;

            return query.Provider.Execute<int>(
                Expression.Call(
                    null,
                    _insertOrUpdateMethodInfo2.MakeGenericMethod(typeof(T)), query.Expression,
                    Expression.Quote(insertSetter), Expression.Quote(onDuplicateKeyUpdateSetter),
                    Expression.Quote(keySelector)));
        }

        #endregion

        #region DDL Operations

        public static int Drop<T>([NotNull] this ITable<T> target)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));

            IQueryable<T> query = target;

            return query.Provider.Execute<int>(
                Expression.Call(
                    null,
                    _dropMethodInfo2.MakeGenericMethod(typeof(T)), query.Expression));
        }

        #endregion

        #region Take / Skip / ElementAt

        public static IQueryable<TSource> Take<TSource>(
            [NotNull] this IQueryable<TSource> source,
            [NotNull, InstantHandle] Expression<Func<int>> count)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (count == null) throw new ArgumentNullException(nameof(count));

            return source.Provider.CreateQuery<TSource>(
                Expression.Call(
                    null,
                    _takeMethodInfo.MakeGenericMethod(typeof(TSource)),
                    new[] {source.Expression, Expression.Quote(count)}));
        }


        public static IQueryable<TSource> Skip<TSource>(
            [NotNull] this IQueryable<TSource> source,
            [NotNull, InstantHandle] Expression<Func<int>> count)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (count == null) throw new ArgumentNullException(nameof(count));

            return source.Provider.CreateQuery<TSource>(
                Expression.Call(
                    null,
                    _skipMethodInfo.MakeGenericMethod(typeof(TSource)),
                    new[] {source.Expression, Expression.Quote(count)}));
        }


        public static TSource ElementAt<TSource>(
            [NotNull] this IQueryable<TSource> source,
            [NotNull, InstantHandle] Expression<Func<int>> index)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (index == null) throw new ArgumentNullException(nameof(index));

            return source.Provider.Execute<TSource>(
                Expression.Call(
                    null,
                    _elementAtMethodInfo.MakeGenericMethod(typeof(TSource)),
                    new[] {source.Expression, Expression.Quote(index)}));
        }


        public static TSource ElementAtOrDefault<TSource>(
            [NotNull] this IQueryable<TSource> source,
            [NotNull, InstantHandle] Expression<Func<int>> index)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (index == null) throw new ArgumentNullException(nameof(index));

            return source.Provider.Execute<TSource>(
                Expression.Call(
                    null,
                    _elementAtOrDefaultMethodInfo.MakeGenericMethod(typeof(TSource)),
                    new[] {source.Expression, Expression.Quote(index)}));
        }

        #endregion

        #region Having

        public static IQueryable<TSource> Having<TSource>(
            [NotNull] this IQueryable<TSource> source,
            [NotNull, InstantHandle] Expression<Func<TSource, bool>> predicate)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));

            return source.Provider.CreateQuery<TSource>(
                Expression.Call(
                    null,
                    _setMethodInfo7.MakeGenericMethod(typeof(TSource)),
                    new[] {source.Expression, Expression.Quote(predicate)}));
        }

        #endregion
    }
}