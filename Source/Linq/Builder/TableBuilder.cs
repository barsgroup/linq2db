﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Bars2Db.Common;
using Bars2Db.Expressions;
using Bars2Db.Extensions;
using Bars2Db.Mapping;
using Bars2Db.Reflection;
using Bars2Db.SqlQuery;
using Bars2Db.SqlQuery.QueryElements.Conditions;
using Bars2Db.SqlQuery.QueryElements.Conditions.Interfaces;
using Bars2Db.SqlQuery.QueryElements.Enums;
using Bars2Db.SqlQuery.QueryElements.Interfaces;
using Bars2Db.SqlQuery.QueryElements.Predicates;
using Bars2Db.SqlQuery.QueryElements.Predicates.Interfaces;
using Bars2Db.SqlQuery.QueryElements.SqlElements;
using Bars2Db.SqlQuery.QueryElements.SqlElements.Interfaces;

namespace Bars2Db.Linq.Builder
{
    internal class TableBuilder : ISequenceBuilder
    {
        #region TableContext

        public class TableContext : IBuildContext
        {
            #region ConvertToSql

            public SqlInfo[] ConvertToSql(Expression expression, int level, ConvertFlags flags)
            {
                switch (flags)
                {
                    case ConvertFlags.All:
                    {
                        var table = FindTable(expression, level, false);

                        if (table.Field == null)
                        {
                            return table.Table.SqlTable.Fields.Values.Select(
                                f => new SqlInfo(f.ColumnDescriptor.MemberInfo)
                                         {
                                             Sql = f
                                         }).ToArray();
                        }

                        if (expression == null)
                        {
                            var fields = table.Table.SqlTable.Fields.Values;
                            var sqlInfo = fields.Select(
                                f => new SqlInfo
                                {
                                    Sql = f
                                });
                            return sqlInfo.ToArray();
                        }

                            break;
                    }

                    case ConvertFlags.Key:
                    {
                        var table = FindTable(expression, level, false);

                        if (table.Field == null)
                        {
                            var q =
                                table.Table.SqlTable.Fields.Values.Where(f => f.IsPrimaryKey)
                                    .OrderBy(f => f.PrimaryKeyOrder)
                                    .Select(f => new SqlInfo(f.ColumnDescriptor.MemberInfo) { Sql = f });

                            var key = q.ToArray();

                            return key.Length != 0 ? key : ConvertToSql(expression, level, ConvertFlags.All);
                        }

                        break;
                    }

                    case ConvertFlags.Field:
                    {
                        var table = FindTable(expression, level, true);

                        if (table.Field != null)
                            return new[]
                            {
                                new SqlInfo(table.Field.ColumnDescriptor.MemberInfo) {Sql = table.Field}
                            };

                        if (expression == null)
                        {
                            var fields = table.Table.SqlTable.Fields.Values;
                            var sqlInfo = fields.Select(
                                f => new SqlInfo
                                         {
                                             Sql = f
                                         });
                            return sqlInfo.ToArray();
                        }
                        break;
                    }
                }

                throw new NotImplementedException();
            }

            #endregion

            #region IsExpression

            public IsExpressionResult IsExpression(Expression expression, int level, RequestFor requestFor)
            {
                switch (requestFor)
                {
                    case RequestFor.Field:
                    {
                        var table = FindTable(expression, level, false);
                        return new IsExpressionResult(table != null && table.Field != null);
                    }

                    case RequestFor.Table:
                    case RequestFor.Object:
                    {
                        var table = FindTable(expression, level, false);
                        var isTable =
                            table != null &&
                            table.Field == null &&
                            (expression == null || expression.GetLevelExpression(table.Level) == expression);

                        return new IsExpressionResult(isTable, isTable ? table.Table : null);
                    }

                    case RequestFor.Expression:
                    {
                        if (expression == null)
                            return IsExpressionResult.False;

                        var levelExpression = expression.GetLevelExpression(level);

                        switch (levelExpression.NodeType)
                        {
                            case ExpressionType.MemberAccess:
                            case ExpressionType.Parameter:
                            case ExpressionType.Call:

                                var table = FindTable(expression, level, false);
                                return new IsExpressionResult(table == null);
                        }

                        return IsExpressionResult.True;
                    }

                    case RequestFor.Association:
                    {
                        if (EntityDescriptor.Associations.Count > 0)
                        {
                            var table = FindTable(expression, level, false);
                            var isat =
                                table != null &&
                                table.Table is AssociatedTableContext &&
                                table.Field == null &&
                                (expression == null || expression.GetLevelExpression(table.Level) == expression);

                            return new IsExpressionResult(isat, isat ? table.Table : null);
                        }

                        return IsExpressionResult.False;
                    }
                }

                return IsExpressionResult.False;
            }

            #endregion

            #region ConvertToParentIndex

            public int ConvertToParentIndex(int index, IBuildContext context)
            {
                return Parent == null ? index : Parent.ConvertToParentIndex(index, this);
            }

            #endregion

            #region SetAlias

            public void SetAlias(string alias)
            {
                if (alias == null)
                    return;

#if NETFX_CORE
                if (alias.Contains("<"))
#else
                if (alias.Contains('<'))
#endif

                    if (SqlTable.Alias == null)
                        SqlTable.Alias = alias;
            }

            #endregion

            #region GetSubQuery

            public IQueryExpression GetSubQuery(IBuildContext context)
            {
                return null;
            }

            #endregion

            #region Properties

#if DEBUG
            public string _sqlQueryText => Select == null ? "" : Select.SqlText;
#endif

            public ExpressionBuilder Builder { get; }
            public Expression Expression { get; }
            public ISelectQuery Select { get; set; }
            public List<MemberInfo[]> LoadWith { get; set; }

            public virtual IBuildContext Parent { get; set; }

            public Type OriginalType;
            public Type ObjectType;
            public EntityDescriptor EntityDescriptor;
            public ISqlTable SqlTable;

            #endregion

            #region Init

            public TableContext(ExpressionBuilder builder, BuildInfo buildInfo, Type originalType)
            {
                Builder = builder;
                Parent = buildInfo.Parent;
                Expression = buildInfo.Expression;
                Select = buildInfo.SelectQuery;

                OriginalType = originalType;
                ObjectType = GetObjectType();
                SqlTable = new SqlTable(builder.MappingSchema, ObjectType);
                EntityDescriptor = Builder.MappingSchema.GetEntityDescriptor(ObjectType);

                Select.From.Table(SqlTable);

                Init();
            }

            protected TableContext(ExpressionBuilder builder, ISelectQuery selectQuery)
            {
                Builder = builder;
                Select = selectQuery;
            }

            public TableContext(ExpressionBuilder builder, BuildInfo buildInfo)
            {
                Builder = builder;
                Parent = buildInfo.Parent;
                Expression = buildInfo.Expression;
                Select = buildInfo.SelectQuery;

                var mc = (MethodCallExpression) Expression;
                var attr = builder.GetTableFunctionAttribute(mc.Method);

                if (!typeof(ITable<>).IsSameOrParentOf(mc.Method.ReturnType))
                    throw new LinqException("Table function has to return Table<T>.");

                OriginalType = mc.Method.ReturnType.GetGenericArgumentsEx()[0];
                ObjectType = GetObjectType();
                SqlTable = new SqlTable(builder.MappingSchema, ObjectType);
                EntityDescriptor = Builder.MappingSchema.GetEntityDescriptor(ObjectType);

                Select.From.Table(SqlTable);

                var args = mc.Arguments.Select(a => builder.ConvertToSql(this, a));

                attr.SetTable(Builder.MappingSchema, SqlTable, mc.Method, mc.Arguments, args);

                Init();
            }

            protected Type GetObjectType()
            {
                for (var type = OriginalType.BaseTypeEx();
                    type != null && type != typeof(object);
                    type = type.BaseTypeEx())
                {
                    var mapping = Builder.MappingSchema.GetEntityDescriptor(type).InheritanceMapping;

                    if (mapping.Count > 0)
                        return type;
                }

                return OriginalType;
            }

            public List<InheritanceMapping> InheritanceMapping;

            protected void Init()
            {
                Builder.Contexts.Add(this);

                InheritanceMapping = EntityDescriptor.InheritanceMapping;

                // Original table is a parent.
                //
                if (ObjectType == OriginalType)
                {
                    return;
                }

                var predicate = Builder.MakeIsPredicate(this, OriginalType);

                if (predicate is IExpr)
                {
                    GetDescriminatorConditionsStorage().AddLast(new Condition(false, predicate));
                }
            }

            protected virtual LinkedList<ICondition> GetDescriminatorConditionsStorage()
            {
                return Select.Where.Search.Conditions;
            }

            #endregion

            #region BuildQuery

            private static object DefaultInheritanceMappingException(object value, Type type)
            {
                throw new LinqException(
                    "Inheritance mapping is not defined for discriminator value '{0}' in the '{1}' hierarchy.", value,
                    type);
            }

            private void SetLoadWithBindings(Type objectType, ParameterExpression parentObject, List<Expression> exprs)
            {
                var loadWith = GetLoadWith();

                if (loadWith == null)
                    return;

                var members = GetLoadWith(loadWith);

                foreach (var member in members)
                {
                    var ma = Expression.MakeMemberAccess(Expression.Constant(null, objectType), member.MemberInfo);

                    if (member.NextLoadWith.Count > 0)
                    {
                        var table = FindTable(ma, 1, false);
                        table.Table.LoadWith = member.NextLoadWith;
                    }

                    var attr = Builder.MappingSchema.GetAttribute<AssociationAttribute>(member.MemberInfo);

                    var ex = BuildExpression(ma, 1, parentObject);
                    var ax = Expression.Assign(
                        attr != null && attr.Storage != null
                            ? Expression.PropertyOrField(parentObject, attr.Storage)
                            : Expression.MakeMemberAccess(parentObject, member.MemberInfo),
                        ex);

                    exprs.Add(ax);
                }
            }

            private static bool IsRecordAttribute(Attribute attr)
            {
                return attr.GetType().FullName == "Microsoft.FSharp.Core.CompilationMappingAttribute";
            }

            private ParameterExpression _variable;

            private Expression BuildTableExpression(bool buildBlock, Type objectType, int[] index)
            {
                if (buildBlock && _variable != null)
                    return _variable;

                var entityDescriptor = Builder.MappingSchema.GetEntityDescriptor(objectType);

                var attr = Builder.MappingSchema.GetAttributes<Attribute>(objectType).FirstOrDefault(IsRecordAttribute);

                var expr = attr == null
                    ? BuildDefaultConstructor(entityDescriptor, objectType, index)
                    : BuildRecordConstructor(entityDescriptor, objectType, index);

                expr = ProcessExpression(expr);

                if (!buildBlock)
                    return expr;

                return _variable = Builder.BuildVariable(expr);
            }

            private Expression BuildDefaultConstructor(EntityDescriptor entityDescriptor, Type objectType, int[] index)
            {
                var members =
                    (
                        from idx in index.Select((n, i) => new {n, i})
                        where idx.n >= 0
                        let cd = entityDescriptor.Columns[idx.i]
                        where
                            cd.Storage != null ||
                            !(cd.MemberAccessor.MemberInfo is PropertyInfo) ||
                            ((PropertyInfo) cd.MemberAccessor.MemberInfo).GetSetMethodEx(true) != null
                        select new
                        {
                            Column = cd,
                            Expr =
                                new ConvertFromDataReaderExpression(cd.MemberType, idx.n, Builder.DataReaderLocal,
                                    Builder.DataContextInfo.DataContext)
                        }
                        ).ToList();

                Expression expr = Expression.MemberInit(
                    Expression.New(objectType),
                    members
                        .Where(m => !m.Column.MemberAccessor.IsComplex)
                        .Where(m => !m.Column.Transparent)
                        .Select(m => (MemberBinding) Expression.Bind(
                            m.Column.Storage == null
                                ? m.Column.MemberAccessor.MemberInfo
                                : Expression.PropertyOrField(Expression.Constant(null, objectType), m.Column.Storage)
                                    .Member,
                            m.Expr)));

                var hasComplex = members.Any(m => m.Column.MemberAccessor.IsComplex);
                var loadWith = GetLoadWith();

                if (hasComplex || loadWith != null)
                {
                    var obj = Expression.Variable(expr.Type);
                    var exprs = new List<Expression> {Expression.Assign(obj, expr)};

                    if (hasComplex)
                    {
                        exprs.AddRange(
                            members.Where(m => m.Column.MemberAccessor.IsComplex).Select(m =>
                                m.Column.MemberAccessor.SetterExpression.GetBody(obj, m.Expr)));
                    }

                    if (loadWith != null)
                    {
                        SetLoadWithBindings(objectType, obj, exprs);
                    }

                    exprs.Add(obj);

                    expr = Expression.Block(new[] {obj}, exprs);
                }

                return expr;
            }

            private class ColumnInfo
            {
                public Expression Expression;
                public bool IsComplex;
                public string Name;
            }

            private IEnumerable<Expression> GetExpressions(TypeAccessor typeAccessor, bool isRecordType,
                List<ColumnInfo> columns)
            {
                var members = isRecordType
                    ? typeAccessor.Members.Where(m =>
                        Builder.MappingSchema.GetAttributes<Attribute>(m.MemberInfo).Any(IsRecordAttribute))
                    : typeAccessor.Members;

                foreach (var member in members)
                {
                    var column = columns.FirstOrDefault(c => !c.IsComplex && c.Name == member.Name);

                    if (column != null)
                    {
                        yield return column.Expression;
                    }
                    else
                    {
                        var name = member.Name + '.';
                        var cols = columns.Where(c => c.IsComplex && c.Name.StartsWith(name)).ToList();

                        if (cols.Count == 0)
                        {
                            yield return null;
                        }
                        else
                        {
                            foreach (var col in cols)
                            {
                                col.Name = col.Name.Substring(name.Length);
                                col.IsComplex = col.Name.Contains(".");
                            }

                            var typeAcc = TypeAccessor.GetAccessor(member.Type);
                            var isRec =
                                Builder.MappingSchema.GetAttributes<Attribute>(member.Type).Any(IsRecordAttribute);

                            var exprs = GetExpressions(typeAcc, isRec, cols).ToList();

                            if (isRec)
                            {
                                var ctor = member.Type.GetConstructorsEx().Single();
                                var ctorParms = ctor.GetParameters();

                                var parms =
                                    (
                                        from p in ctorParms.Select((p, i) => new {p, i})
                                        join e in exprs.Select((e, i) => new {e, i}) on p.i equals e.i into j
                                        from e in j.DefaultIfEmpty()
                                        select
                                            e.e ??
                                            Expression.Constant(
                                                p.p.DefaultValue ??
                                                Builder.MappingSchema.GetDefaultValue(p.p.ParameterType),
                                                p.p.ParameterType)
                                        ).ToList();

                                yield return Expression.New(ctor, parms);
                            }
                            else
                            {
                                var expr = Expression.MemberInit(
                                    Expression.New(member.Type),
                                    from m in typeAcc.Members.Zip(exprs, (m, e) => new {m, e})
                                    where m.e != null
                                    select (MemberBinding) Expression.Bind(m.m.MemberInfo, m.e));

                                yield return expr;
                            }
                        }
                    }
                }
            }

            private Expression BuildRecordConstructor(EntityDescriptor entityDescriptor, Type objectType, int[] index)
            {
                var ctor = objectType.GetConstructorsEx().Single();

                var exprs = GetExpressions(entityDescriptor.TypeAccessor, true,
                    (
                        from idx in index.Select((n, i) => new {n, i})
                        where idx.n >= 0
                        let cd = entityDescriptor.Columns[idx.i]
                        select new ColumnInfo
                        {
                            IsComplex = cd.MemberAccessor.IsComplex,
                            Name = cd.MemberName,
                            Expression =
                                new ConvertFromDataReaderExpression(cd.MemberType, idx.n, Builder.DataReaderLocal,
                                    Builder.DataContextInfo.DataContext)
                        }
                        ).ToList()).ToList();

                var parms =
                    (
                        from p in ctor.GetParameters().Select((p, i) => new {p, i})
                        join e in exprs.Select((e, i) => new {e, i}) on p.i equals e.i into j
                        from e in j.DefaultIfEmpty()
                        select
                            e.e ??
                            Expression.Constant(
                                p.p.DefaultValue ?? Builder.MappingSchema.GetDefaultValue(p.p.ParameterType),
                                p.p.ParameterType)
                        ).ToList();

                var expr = Expression.New(ctor, parms);

                return expr;
            }

            protected virtual Expression ProcessExpression(Expression expression)
            {
                return expression;
            }

            private int[] BuildIndex(int[] index, Type objectType)
            {
                var n = 0;
                var ed = Builder.MappingSchema.GetEntityDescriptor(objectType);

                var names = ed.Columns
                    .Where(cd => cd.MemberAccessor.TypeAccessor.Type == ed.TypeAccessor.Type)
                    .ToDictionary(cd => cd.MemberName, cd => n++);

                var q =
                    from r in SqlTable.Fields.Values.Select((f, i) => new {f, i})
                    where names.ContainsKey(r.f.Name)
                    orderby names[r.f.Name]
                    select index[r.i];

                return q.ToArray();
            }

            protected virtual Expression BuildQuery(Type tableType, TableContext tableContext,
                ParameterExpression parentObject)
            {
                SqlInfo[] info;

                if (ObjectType == tableType || tableType == typeof(object))
                {
                    info = ConvertToIndex(null, 0, ConvertFlags.All);
                }
                else
                {
                    info = ConvertToSql(null, 0, ConvertFlags.All);

                    var table = new SqlTable(Builder.MappingSchema, tableType);
                    var q =
                        from fld1 in table.Fields.Values.Select((f, i) => new {f, i})
                        join fld2 in info on fld1.f.Name equals ((ISqlField) fld2.Sql).Name
                        orderby fld1.i
                        select GetIndex(fld2);

                    info = q.ToArray();
                }

                var index = info
                    .Select(idx => ConvertToParentIndex(idx.Index, null))
                    .ToArray();

                if (ObjectType != tableType || InheritanceMapping.Count == 0)
                    return BuildTableExpression(!Builder.IsBlockDisable, tableType, index);

                Expression expr;

                var defaultMapping = InheritanceMapping.SingleOrDefault(m => m.IsDefault);

                if (defaultMapping != null)
                {
                    expr = Expression.Convert(
                        BuildTableExpression(false, defaultMapping.Type, BuildIndex(index, defaultMapping.Type)),
                        ObjectType);
                }
                else
                {
                    var exceptionMethod = MemberHelper.MethodOf(() => DefaultInheritanceMappingException(null, null));
                    var dindex =
                        SqlTable.Fields.Values
                        .Where(f => f.Name == InheritanceMapping[0].DiscriminatorName)
                        .Select(f => ConvertToParentIndex(_indexes[f].Index, null)).First();

                    expr = Expression.Convert(
                        Expression.Call(null, exceptionMethod,
                            Expression.Call(
                                ExpressionBuilder.DataReaderParam,
                                ReflectionHelper.DataReader.GetValue,
                                Expression.Constant(dindex)),
                            Expression.Constant(ObjectType)),
                        ObjectType);
                }

                foreach (
                    var mapping in InheritanceMapping.Select((m, i) => new {m, i}).Where(m => m.m != defaultMapping))
                {
                    var dindex =
                        (
                            from f in SqlTable.Fields.Values
                            where f.Name == InheritanceMapping[mapping.i].DiscriminatorName
                            select ConvertToParentIndex(_indexes[f].Index, null)
                            ).First();

                    Expression testExpr;

                    if (mapping.m.Code == null)
                    {
                        testExpr = Expression.Call(
                            ExpressionBuilder.DataReaderParam,
                            ReflectionHelper.DataReader.IsDBNull,
                            Expression.Constant(dindex));
                    }
                    else
                    {
                        var codeType = mapping.m.Code.GetType();

                        testExpr = Expression.Equal(
                            Expression.Constant(mapping.m.Code),
                            Builder.BuildSql(codeType, dindex));
                    }

                    expr = Expression.Condition(
                        testExpr,
                        Expression.Convert(
                            BuildTableExpression(false, mapping.m.Type, BuildIndex(index, mapping.m.Type)), ObjectType),
                        expr);
                }

                return expr;
            }

            public void BuildQuery<T>(Query<T> query, ParameterExpression queryParameter)
            {
                var expr = BuildQuery(typeof(T), this, null);
                var mapper = Builder.BuildMapper<T>(expr);

                query.SetQuery(mapper);
            }

            #endregion

            #region BuildExpression

            public Expression BuildExpression(Expression expression, int level)
            {
                return BuildExpression(expression, level, null);
            }

            private Expression BuildExpression(Expression expression, int level, ParameterExpression parentObject)
            {
                // Build table.
                //
                var table = FindTable(expression, level, false);

                if (table == null)
                {
                    var memberExpression = expression as MemberExpression;
                    if (memberExpression != null)
                    {
                        if (EntityDescriptor != null &&
                            EntityDescriptor.TypeAccessor.Type == memberExpression.Member.DeclaringType)
                        {
                            throw new LinqException("Member '{0}.{1}' is not a table column.",
                                memberExpression.Member.DeclaringType.Name, memberExpression.Member.Name);
                        }
                    }

                    throw new InvalidOperationException();
                }

                if (table.Field == null)
                    return table.Table.BuildQuery(table.Table.OriginalType, table.Table, parentObject);

                // Build field.
                //
                var info = ConvertToIndex(expression, level, ConvertFlags.Field).Single();
                var idx = ConvertToParentIndex(info.Index, null);

                return Builder.BuildSql(expression, idx);
            }

            #endregion

            #region ConvertToIndex

            private readonly Dictionary<IQueryExpression, SqlInfo> _indexes =
                new Dictionary<IQueryExpression, SqlInfo>();

            protected SqlInfo GetIndex(SqlInfo expr)
            {
                SqlInfo n;

                if (_indexes.TryGetValue(expr.Sql, out n))
                    return n;

                var sqlField = expr.Sql as ISqlField;
                expr.Index = sqlField != null
                    ? Select.Select.Add(sqlField, sqlField.Alias)
                    : Select.Select.Add(expr.Sql);

                expr.Query = Select;

                _indexes.Add(expr.Sql, expr);

                return expr;
            }

            public SqlInfo[] ConvertToIndex(Expression expression, int level, ConvertFlags flags)
            {
                switch (flags)
                {
                    case ConvertFlags.Field:
                    case ConvertFlags.Key:
                    case ConvertFlags.All:

                        var info = ConvertToSql(expression, level, flags);

                        for (var i = 0; i < info.Length; i++)
                            info[i] = GetIndex(info[i]);

                        return info;
                }

                throw new NotImplementedException();
            }

            #endregion

            #region GetContext

            private interface IAssociationHelper
            {
                Expression GetExpression(Expression parent, AssociatedTableContext association);
            }

            private class AssociationHelper<T> : IAssociationHelper
                where T : class
            {
                public Expression GetExpression(Expression parent, AssociatedTableContext association)
                {
                    var expression = association.Builder.DataContextInfo.DataContext.GetTable<T>();

                    var loadWith = association.GetLoadWith();

                    if (loadWith != null)
                    {
                        foreach (var members in loadWith)
                        {
                            var pLoadWith = Expression.Parameter(typeof(T), "t");
                            var isPrevList = false;

                            Expression obj = pLoadWith;

                            foreach (var member in members)
                            {
                                if (isPrevList)
                                    obj = new GetItemExpression(obj);

                                obj = Expression.MakeMemberAccess(obj, member);

                                isPrevList = typeof(IEnumerable).IsSameOrParentOf(obj.Type);
                            }

                            expression = expression.LoadWith(Expression.Lambda<Func<T, object>>(obj, pLoadWith));
                        }
                    }

                    Expression expr = null;
                    var param = Expression.Parameter(typeof(T), "c");

                    foreach (var cond in association.ParentAssociationJoin.Condition.Conditions)
                    {
                        IExprExpr p;

                        var searchCondition = cond.Predicate as ISearchCondition;
                        if (searchCondition != null)
                        {
                            p = searchCondition.Conditions
                                .Select(c => c.Predicate)
                                .OfType<IExprExpr>()
                                .First();
                        }
                        else
                        {
                            p = (IExprExpr) cond.Predicate;
                        }

                        var e1 =
                            Expression.MakeMemberAccess(parent, ((ISqlField) p.Expr1).ColumnDescriptor.MemberInfo) as
                                Expression;

                        Expression e2 = Expression.MakeMemberAccess(param,
                            ((ISqlField) p.Expr2).ColumnDescriptor.MemberInfo);

                        if (e1.Type != e2.Type)
                        {
                            if (e1.Type.CanConvertTo(e2.Type))
                                e1 = Expression.Convert(e1, e2.Type);
                            else if (e2.Type.CanConvertTo(e1.Type))
                                e2 = Expression.Convert(e2, e1.Type);
                        }

//						while (e1.Type != e2.Type)
//						{
//							if (e1.Type.IsNullable())
//							{
//								e1 = Expression.PropertyOrField(e1, "Value");
//								continue;
//							}
//
//							if (e2.Type.IsNullable())
//							{
//								e2 = Expression.PropertyOrField(e2, "Value");
//								continue;
//							}
//
//							e2 = Expression.Convert(e2, e1.Type);
//						}

                        var ex = Expression.Equal(e1, e2);

                        expr = expr == null ? ex : Expression.AndAlso(expr, ex);
                    }

                    var predicate = Expression.Lambda<Func<T, bool>>(expr, param);

                    return expression.Where(predicate).Expression;
                }
            }

            public IBuildContext GetContext(Expression expression, int level, BuildInfo buildInfo)
            {
                if (expression == null)
                {
                    if (buildInfo != null && buildInfo.IsSubQuery)
                    {
                        var table = new TableContext(
                            Builder,
                            new BuildInfo(Parent is SelectManyBuilder.SelectManyContext ? this : Parent, Expression,
                                buildInfo.SelectQuery),
                            SqlTable.ObjectType);

                        return table;
                    }

                    return this;
                }

                if (EntityDescriptor.Associations.Count > 0)
                {
                    var levelExpression = expression.GetLevelExpression(level);

                    if (buildInfo != null && buildInfo.IsSubQuery)
                    {
                        if (levelExpression == expression && expression.NodeType == ExpressionType.MemberAccess)
                        {
                            var tableLevel = GetAssociation(expression, level);
                            var association = (AssociatedTableContext) tableLevel.Table;

                            if (association.IsList)
                            {
                                var ma = (MemberExpression) buildInfo.Expression;
                                var atype = typeof(AssociationHelper<>).MakeGenericType(association.ObjectType);
                                var helper = (IAssociationHelper) Activator.CreateInstance(atype);
                                var expr = helper.GetExpression(ma.Expression, association);

                                buildInfo.IsAssociationBuilt = true;

                                if (tableLevel.IsNew || buildInfo.CopyTable)
                                    association.ParentAssociationJoin.IsWeak = true;

                                return Builder.BuildSequence(new BuildInfo(buildInfo, expr));
                            }
                        }
                        else
                        {
                            var association = GetAssociation(levelExpression, level);
                            ((AssociatedTableContext) association.Table).ParentAssociationJoin.IsWeak = false;

//							var paj         = ((AssociatedTableContext)association.Table).ParentAssociationJoin;
//
//							paj.IsWeak = paj.IsWeak && buildInfo.CopyTable;

                            return association.Table.GetContext(expression, level + 1, buildInfo);
                        }
                    }
                }

                throw new InvalidOperationException();
            }

            #endregion

            #region Helpers

            protected class LoadWithItem
            {
                public MemberInfo MemberInfo;
                public List<MemberInfo[]> NextLoadWith;
            }

            protected List<LoadWithItem> GetLoadWith(List<MemberInfo[]> infos)
            {
                return
                    (
                        from lw in infos
                        select new
                        {
                            head = lw.First(),
                            tail = lw.Skip(1).ToArray()
                        }
                        into info
                        group info by info.head
                        into gr
                        select new LoadWithItem
                        {
                            MemberInfo = gr.Key,
                            NextLoadWith = (from i in gr where i.tail.Length > 0 select i.tail).ToList()
                        }
                        ).ToList();
            }

            protected internal virtual List<MemberInfo[]> GetLoadWith()
            {
                return LoadWith;
            }

            private ISqlField GetField(Expression expression, int level, bool throwException)
            {
                if (expression.NodeType == ExpressionType.MemberAccess)
                {
                    var memberExpression = (MemberExpression) expression;

                    if (EntityDescriptor.Aliases != null)
                    {
                        if (EntityDescriptor.Aliases.ContainsKey(memberExpression.Member.Name))
                        {
                            var alias = EntityDescriptor[memberExpression.Member.Name];

                            if (alias == null)
                            {
                                foreach (var column in EntityDescriptor.Columns)
                                {
                                    if (column.MemberInfo.EqualsTo(memberExpression.Member, SqlTable.ObjectType))
                                    {
                                        expression = memberExpression = Expression.PropertyOrField(
                                            Expression.Convert(memberExpression.Expression,
                                                column.MemberInfo.DeclaringType), column.MemberName);
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                var expr = memberExpression.Expression;

                                if (alias.MemberInfo.DeclaringType != memberExpression.Member.DeclaringType)
                                    expr = Expression.Convert(memberExpression.Expression,
                                        alias.MemberInfo.DeclaringType);

                                expression = memberExpression = Expression.PropertyOrField(expr, alias.MemberName);
                            }
                        }
                    }

                    var levelExpression = expression.GetLevelExpression(level);

                    if (levelExpression.NodeType == ExpressionType.MemberAccess)
                    {
                        if (levelExpression != expression)
                        {
                            var levelMember = (MemberExpression) levelExpression;

                            if (memberExpression.Member.IsNullableValueMember() &&
                                memberExpression.Expression == levelExpression)
                                memberExpression = levelMember;
                            else
                            {
                                var sameType =
                                    levelMember.Member.ReflectedTypeEx() == SqlTable.ObjectType ||
                                    levelMember.Member.DeclaringType == SqlTable.ObjectType;

                                if (!sameType)
                                {
                                    var mi = SqlTable.ObjectType.GetInstanceMemberEx(levelMember.Member.Name);
                                    sameType = mi.Any(_ => _.DeclaringType == levelMember.Member.DeclaringType);
                                }

                                if (sameType || InheritanceMapping.Count > 0)
                                {
                                    foreach (var field in SqlTable.Fields.Values)
                                    {
                                        if (field.Name.IndexOf('.') >= 0)
                                        {
                                            var name = levelMember.Member.Name;

                                            for (var ex = (MemberExpression) expression;
                                                ex != levelMember;
                                                ex = (MemberExpression) ex.Expression)
                                                name += "." + ex.Member.Name;

                                            if (field.Name == name)
                                                return field;
                                        }
                                    }
                                }
                            }
                        }

                        if (levelExpression == memberExpression)
                        {
                            foreach (var field in SqlTable.Fields.Values)
                            {
                                if (
                                    field.ColumnDescriptor.MemberInfo.EqualsTo(memberExpression.Member,
                                        SqlTable.ObjectType) && !field.ColumnDescriptor.Transparent)
                                {
                                    if (field.ColumnDescriptor.MemberAccessor.IsComplex)
                                    {
                                        var name = memberExpression.Member.Name;
                                        var childExpression = memberExpression;

                                        if (childExpression.Expression is MemberExpression)
                                        {
                                            while ((childExpression = childExpression.Expression as MemberExpression) !=
                                                   null)
                                            {
                                                name = childExpression.Member.Name + '.' + name;
                                            }

                                            var fld = SqlTable.Fields.Values.FirstOrDefault(f => f.Name == name);

                                            if (fld != null)
                                                return fld;
                                        }
                                    }
                                    else
                                    {
                                        return field;
                                    }
                                }

                                if (InheritanceMapping.Count > 0 && field.Name == memberExpression.Member.Name)
                                    foreach (var mapping in InheritanceMapping)
                                        foreach (
                                            var mm in Builder.MappingSchema.GetEntityDescriptor(mapping.Type).Columns)
                                            if (mm.MemberAccessor.MemberInfo.EqualsTo(memberExpression.Member) &&
                                                !field.ColumnDescriptor.Transparent)
                                                return field;
                            }

                            if (throwException &&
                                EntityDescriptor != null &&
                                EntityDescriptor.TypeAccessor.Type == memberExpression.Member.DeclaringType)
                            {
                                throw new LinqException("Member '{0}.{1}' is not a table column.",
                                    memberExpression.Member.DeclaringType.Name, memberExpression.Member.Name);
                            }
                        }
                    }
                }

                return null;
            }

            [Properties.NotNull] private readonly Dictionary<MemberInfo, AssociatedTableContext> _associations =
                new Dictionary<MemberInfo, AssociatedTableContext>(new MemberInfoComparer());

            private class TableLevel
            {
                public ISqlField Field;
                public bool IsNew;
                public int Level;
                public TableContext Table;
            }

            private TableLevel FindTable(Expression expression, int level, bool throwException)
            {
                if (expression == null)
                    return new TableLevel {Table = this};

                var levelExpression = expression.GetLevelExpression(level);

                switch (levelExpression.NodeType)
                {
                    case ExpressionType.MemberAccess:
                    case ExpressionType.Parameter:
                    {
                        var field = GetField(expression, level, throwException);

                        if (field != null || (level == 0 && levelExpression == expression))
                            return new TableLevel {Table = this, Field = field, Level = level};

                        return GetAssociation(expression, level);
                    }
                }

                return null;
            }

            private TableLevel GetAssociation(Expression expression, int level)
            {
                var objectMapper = EntityDescriptor;
                var levelExpression = expression.GetLevelExpression(level);
                var inheritance =
                    (
                        from m in InheritanceMapping
                        let om = Builder.MappingSchema.GetEntityDescriptor(m.Type)
                        where om.Associations.Count > 0
                        select om
                        ).ToList();

                if (objectMapper.Associations.Count > 0 || inheritance.Count > 0)
                {
                    if (levelExpression.NodeType == ExpressionType.MemberAccess)
                    {
                        var memberExpression = (MemberExpression) levelExpression;
                        var isNew = false;

                        AssociatedTableContext tableAssociation;

                        if (!_associations.TryGetValue(memberExpression.Member, out tableAssociation))
                        {
                            var q =
                                from a in
                                    objectMapper.Associations.Concat(inheritance.SelectMany(om => om.Associations))
                                where a.MemberInfo.EqualsTo(memberExpression.Member)
                                select new AssociatedTableContext(Builder, this, a) {Parent = Parent};

                            tableAssociation = q.FirstOrDefault();

                            isNew = true;

                            _associations.Add(memberExpression.Member, tableAssociation);
                        }

                        if (tableAssociation != null)
                        {
                            if (levelExpression == expression)
                                return new TableLevel {Table = tableAssociation, Level = level, IsNew = isNew};

                            var al = tableAssociation.GetAssociation(expression, level + 1);

                            if (al != null)
                                return al;

                            var field = tableAssociation.GetField(expression, level + 1, false);

                            return new TableLevel
                            {
                                Table = tableAssociation,
                                Field = field,
                                Level = field == null ? level : level + 1,
                                IsNew = isNew
                            };
                        }
                    }
                }

                return null;
            }

            #endregion
        }

        #endregion

        #region AssociatedTableContext

        public class AssociatedTableContext : TableContext
        {
            public readonly AssociationDescriptor Association;
            public readonly bool IsList;
            public readonly TableContext ParentAssociation;
            public readonly IJoinedTable ParentAssociationJoin;

            public AssociatedTableContext(ExpressionBuilder builder, TableContext parent,
                AssociationDescriptor association)
                : base(builder, parent.Select)
            {
                var type = association.MemberInfo.GetMemberType();
                var left = association.CanBeNull;

                if (typeof(IEnumerable).IsSameOrParentOf(type))
                {
                    var etypes = type.GetGenericArguments(typeof(IEnumerable<>));
                    type = etypes != null && etypes.Length > 0 ? etypes[0] : type.GetListItemType();
                    IsList = true;
                }

                OriginalType = type;
                ObjectType = GetObjectType();
                EntityDescriptor = Builder.MappingSchema.GetEntityDescriptor(ObjectType);
                SqlTable = new SqlTable(builder.MappingSchema, ObjectType);

                var psrc = parent.Select.From[parent.SqlTable];
                var join = left ? SqlTable.WeakLeftJoin() : SqlTable.WeakInnerJoin();

                Association = association;
                ParentAssociation = parent;
                ParentAssociationJoin = join.JoinedTable;

                psrc.Joins.AddLast(join.JoinedTable);

                for (var i = 0; i < association.ThisKey.Length; i++)
                {
                    ISqlField field1;
                    ISqlField field2;

                    if (!parent.SqlTable.Fields.TryGetValue(association.ThisKey[i], out field1))
                        throw new LinqException("Association key '{0}' not found for type '{1}.", association.ThisKey[i],
                            parent.ObjectType);

                    if (!SqlTable.Fields.TryGetValue(association.OtherKey[i], out field2))
                        throw new LinqException("Association key '{0}' not found for type '{1}.",
                            association.OtherKey[i], ObjectType);

//					join.Field(field1).Equal.Field(field2);

                    ISqlPredicate predicate = new ExprExpr(
                        field1, EOperator.Equal, field2);

                    predicate = builder.Convert(parent, predicate);

                    join.JoinedTable.Condition.Conditions.AddLast(new Condition(false, predicate));
                }

                Init();
            }

            public override IBuildContext Parent
            {
                get { return ParentAssociation.Parent; }
                set { }
            }

            protected override LinkedList<ICondition> GetDescriminatorConditionsStorage()
            {
                return ParentAssociationJoin.Condition.Conditions;
            }

            protected override Expression ProcessExpression(Expression expression)
            {
                var isLeft = false;

                for (
                    var association = this;
                    isLeft == false && association != null;
                    association = association.ParentAssociation as AssociatedTableContext)
                {
                    isLeft =
                        association.ParentAssociationJoin.JoinType == EJoinType.Left ||
                        association.ParentAssociationJoin.JoinType == EJoinType.OuterApply;
                }

                if (isLeft)
                {
                    Expression cond = null;

                    var keys = ConvertToIndex(null, 0, ConvertFlags.Key);

                    foreach (var key in keys)
                    {
                        var index2 = ConvertToParentIndex(key.Index, null);

                        Expression e = Expression.Call(
                            ExpressionBuilder.DataReaderParam,
                            ReflectionHelper.DataReader.IsDBNull,
                            Expression.Constant(index2));

                        cond = cond == null ? e : Expression.AndAlso(cond, e);
                    }

                    expression = Expression.Condition(cond, Expression.Constant(null, expression.Type), expression);
                }

                return expression;
            }

            protected internal override List<MemberInfo[]> GetLoadWith()
            {
                if (LoadWith == null)
                {
                    var loadWith = ParentAssociation.GetLoadWith();

                    if (loadWith != null)
                    {
                        foreach (var item in GetLoadWith(loadWith))
                        {
                            if (Association.MemberInfo.EqualsTo(item.MemberInfo))
                            {
                                LoadWith = item.NextLoadWith;
                                break;
                            }
                        }
                    }
                }

                return LoadWith;
            }

            protected override Expression BuildQuery(Type tableType, TableContext tableContext,
                ParameterExpression parentObject)
            {
                if (IsList == false)
                    return base.BuildQuery(tableType, tableContext, parentObject);

                if (Common.Configuration.Linq.AllowMultipleQuery == false)
                    throw new LinqException(
                        "Multiple queries are not allowed. Set the 'LinqToDB.Common.Configuration.Linq.AllowMultipleQuery' flag to 'true' to allow multiple queries.");

                var sqtype = typeof(SubQueryHelper<>).MakeGenericType(tableType);
                var helper = (ISubQueryHelper) Activator.CreateInstance(sqtype);

                return helper.GetSubquery(Builder, this, parentObject);
            }

            private interface ISubQueryHelper
            {
                Expression GetSubquery(
                    ExpressionBuilder builder,
                    AssociatedTableContext tableContext,
                    ParameterExpression parentObject);
            }

            private class SubQueryHelper<T> : ISubQueryHelper
                where T : class
            {
                public Expression GetSubquery(
                    ExpressionBuilder builder,
                    AssociatedTableContext tableContext,
                    ParameterExpression parentObject)
                {
                    var lContext = Expression.Parameter(typeof(IDataContext), "ctx");
                    var lParent = Expression.Parameter(typeof(object), "parentObject");

                    var tableExpression = builder.DataContextInfo.DataContext.GetTable<T>();

                    var loadWith = tableContext.GetLoadWith();

                    if (loadWith != null)
                    {
                        foreach (var members in loadWith)
                        {
                            var pLoadWith = Expression.Parameter(typeof(T), "t");
                            var isPrevList = false;

                            Expression obj = pLoadWith;

                            foreach (var member in members)
                            {
                                if (isPrevList)
                                    obj = new GetItemExpression(obj);

                                obj = Expression.MakeMemberAccess(obj, member);

                                isPrevList = typeof(IEnumerable).IsSameOrParentOf(obj.Type);
                            }

                            tableExpression =
                                tableExpression.LoadWith(Expression.Lambda<Func<T, object>>(obj, pLoadWith));
                        }
                    }

                    Expression expression;

                    {
                        // Where
                        var pWhere = Expression.Parameter(typeof(T), "t");

                        Expression expr = null;

                        for (var i = 0; i < tableContext.Association.ThisKey.Length; i++)
                        {
                            Expression thisProp =
                                Expression.PropertyOrField(Expression.Convert(lParent, parentObject.Type),
                                    tableContext.Association.ThisKey[i]);
                            Expression otherProp = Expression.PropertyOrField(pWhere,
                                tableContext.Association.OtherKey[i]);

                            if (otherProp.Type != thisProp.Type)
                            {
                                if (otherProp.Type.CanConvertTo(thisProp.Type))
                                    otherProp = Expression.Convert(otherProp, thisProp.Type);
                                else if (thisProp.Type.CanConvertTo(otherProp.Type))
                                    thisProp = Expression.Convert(thisProp, otherProp.Type);
                            }

                            var ex = Expression.Equal(otherProp, thisProp);

                            expr = expr == null ? ex : Expression.AndAlso(expr, ex);
                        }

                        expression = tableExpression.Where(Expression.Lambda<Func<T, bool>>(expr, pWhere)).Expression;
                    }

                    var lambda = Expression.Lambda<Func<IDataContext, object, IEnumerable<T>>>(expression, lContext,
                        lParent);
                    var queryReader = CompiledQuery.Compile(lambda);

                    expression = Expression.Call(
                        null,
                        MemberHelper.MethodOf(() => ExecuteSubQuery(null, null, null)),
                        ExpressionBuilder.ContextParam,
                        Expression.Convert(parentObject, typeof(object)),
                        Expression.Constant(queryReader));

                    var memberType = tableContext.Association.MemberInfo.GetMemberType();

                    if (memberType == typeof(T[]))
                        return Expression.Call(null, MemberHelper.MethodOf(() => Enumerable.ToArray<T>(null)),
                            expression);

                    if (memberType.IsSameOrParentOf(typeof(List<T>)))
                        return Expression.Call(null, MemberHelper.MethodOf(() => Enumerable.ToList<T>(null)), expression);

                    var ctor = memberType.GetConstructorEx(new[] {typeof(IEnumerable<T>)});

                    if (ctor != null)
                        return Expression.New(ctor, expression);

                    var l = builder.MappingSchema.GetConvertExpression(expression.Type, memberType, false, false);

                    if (l != null)
                        return l.GetBody(expression);

                    throw new LinqToDBException("Expected constructor '{0}(IEnumerable<{1}>)'".Args(
                        memberType.Name, tableContext.ObjectType));
                }

                private static IEnumerable<T> ExecuteSubQuery(
                    QueryContext queryContext,
                    object parentObject,
                    Func<IDataContext, object, IEnumerable<T>> queryReader)
                {
                    var db = queryContext.GetDataContext();

                    try
                    {
                        foreach (var item in queryReader(db.DataContextInfo.DataContext, parentObject))
                            yield return item;
                    }
                    finally
                    {
                        queryContext.ReleaseDataContext(db);
                    }
                }
            }
        }

        #endregion

        #region TableBuilder

        int ISequenceBuilder.BuildCounter { get; set; }

        private static T Find<T>(ExpressionBuilder builder, BuildInfo buildInfo, Func<int, IBuildContext, T> action)
        {
            var expression = buildInfo.Expression;

            switch (expression.NodeType)
            {
                case ExpressionType.Constant:
                {
                    var c = (ConstantExpression) expression;
                    if (c.Value is IQueryable)
                        return action(1, null);

                    break;
                }

                case ExpressionType.Call:
                {
                    var mc = (MethodCallExpression) expression;

                    if (mc.Method.Name == "GetTable")
                        if (typeof(ITable<>).IsSameOrParentOf(expression.Type))
                            return action(2, null);

                    var attr = builder.GetTableFunctionAttribute(mc.Method);

                    if (attr != null)
                        return action(5, null);

                    break;
                }

                case ExpressionType.MemberAccess:

                    if (typeof(ITable<>).IsSameOrParentOf(expression.Type))
                        return action(3, null);

                    // Looking for association.
                    //
                    if (buildInfo.IsSubQuery && buildInfo.SelectQuery.From.Tables.Count == 0)
                    {
                        var ctx = builder.GetContext(buildInfo.Parent, expression);
                        if (ctx != null)
                            return action(4, ctx);
                    }

                    break;

                case ExpressionType.Parameter:
                {
                    if (buildInfo.IsSubQuery && buildInfo.SelectQuery.From.Tables.Count == 0)
                    {
                        var ctx = builder.GetContext(buildInfo.Parent, expression);
                        if (ctx != null)
                            return action(4, ctx);
                    }

                    break;
                }
            }

            return action(0, null);
        }

        public bool CanBuild(ExpressionBuilder builder, BuildInfo buildInfo)
        {
            return Find(builder, buildInfo, (n, _) => n > 0);
        }

        public IBuildContext BuildSequence(ExpressionBuilder builder, BuildInfo buildInfo)
        {
            return Find(builder, buildInfo, (n, ctx) =>
            {
                switch (n)
                {
                    case 0:
                        return null;
                    case 1:
                        return new TableContext(builder, buildInfo,
                            ((IQueryable) ((ConstantExpression) buildInfo.Expression).Value).ElementType);
                    case 2:
                    case 3:
                        return new TableContext(builder, buildInfo, buildInfo.Expression.Type.GetGenericArgumentsEx()[0]);
                    case 4:
                        return ctx.GetContext(buildInfo.Expression, 0, buildInfo);
                    case 5:
                        return new TableContext(builder, buildInfo);
                }

                throw new InvalidOperationException();
            });
        }

        public SequenceConvertInfo Convert(ExpressionBuilder builder, BuildInfo buildInfo, ParameterExpression param)
        {
            return null;
        }

        public bool IsSequence(ExpressionBuilder builder, BuildInfo buildInfo)
        {
            return true;
        }

        #endregion
    }
}