﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Bars2Db.Extensions;
using Bars2Db.SqlQuery.QueryElements;
using Bars2Db.SqlQuery.QueryElements.Clauses;
using Bars2Db.SqlQuery.QueryElements.Clauses.Interfaces;
using Bars2Db.SqlQuery.QueryElements.Conditions;
using Bars2Db.SqlQuery.QueryElements.Conditions.Interfaces;
using Bars2Db.SqlQuery.QueryElements.Enums;
using Bars2Db.SqlQuery.QueryElements.Interfaces;
using Bars2Db.SqlQuery.QueryElements.Predicates;
using Bars2Db.SqlQuery.QueryElements.Predicates.Interfaces;
using Bars2Db.SqlQuery.QueryElements.SqlElements;
using Bars2Db.SqlQuery.QueryElements.SqlElements.Interfaces;
using Bars2Db.SqlQuery.Search;

namespace Bars2Db.SqlQuery
{
    // The casts to object in the below code are an unfortunate necessity due to
    // C#'s restriction against a where T : Enum constraint. (There are ways around
    // this, but they're outside the scope of this simple illustration.)

    public abstract class SearchStrategy<TSearch, TResult>
        where TSearch : class
    {
        public readonly ParameterExpression ActionParam = Expression.Parameter(typeof(Func<TSearch, TResult>), "action");

        protected readonly Expression NullConst = Expression.Constant(null);
        public readonly ParameterExpression ObjParam = Expression.Parameter(typeof(object), "obj");

        public readonly ParameterExpression[] ParamArray;

        public readonly ParameterExpression ResultListParam = Expression.Parameter(typeof(LinkedList<TResult>),
            "resultList");

        public readonly ParameterExpression VisitedParam = Expression.Parameter(typeof(HashSet<object>), "visited");

        protected SearchStrategy()
        {
            ParamArray = new[] {ObjParam, ResultListParam, VisitedParam, ActionParam};
        }

        public abstract Expression<ResultDelegate<TSearch, TResult>> GetStrategyExpression(
            Expression[] executeChildrenExpressions, bool isRoot);
    }

    public abstract class FindStrategy<TSearch> : SearchStrategy<TSearch, TSearch>
        where TSearch : class
    {
    }

    public class ChildrenFirstStrategy<TSearch> : FindStrategy<TSearch>
        where TSearch : class
    {
        public override Expression<ResultDelegate<TSearch, TSearch>> GetStrategyExpression(
            Expression[] executeChildrenExpressions, bool isRoot)
        {
            ////void ExecuteChildrenFirst(object obj, LinkedList<TResult> resultList, HashSet<object> visited, Func<TSearch, TResult> action)
            ////{
            ////    var searchValue = obj as TSearch;
            ////
            ////    if (searchValue != null)
            ////    {
            ////        resultList.AddFirst(searchValue);
            ////    }
            ////
            ////    child1.Execute(obj, resultList, visited, action);
            ////    child2.Execute(obj, resultList, visited, action);
            ////    ...
            ////}

            var castVariable = Expression.Variable(typeof(TSearch), "searchValue");
            var castAs = Expression.TypeAs(ObjParam, typeof(TSearch));
            var castAssign = Expression.Assign(castVariable, castAs);

            var checkNotNull = Expression.NotEqual(castVariable, NullConst);

            var addFirstMethod = typeof(LinkedList<TSearch>).GetMethod("AddFirst", new[] {typeof(TSearch)});
            var callAddFirst = Expression.Call(ResultListParam, addFirstMethod, castVariable);
            var conditionalAdd = Expression.IfThen(checkNotNull, callAddFirst);

            var executeChildrenBlock = executeChildrenExpressions.Any()
                ? Expression.Block(executeChildrenExpressions)
                : (Expression) Expression.Empty();

            var block = Expression.Block(new[] {castVariable}, castAssign, conditionalAdd, executeChildrenBlock);

            return Expression.Lambda<ResultDelegate<TSearch, TSearch>>(block, ParamArray);
        }
    }

    public class DownToStrategy<TSearch> : FindStrategy<TSearch>
        where TSearch : class
    {
        public override Expression<ResultDelegate<TSearch, TSearch>> GetStrategyExpression(
            Expression[] executeChildrenExpressions, bool isRoot)
        {
            ////void ExecuteRootDownTo(object obj, LinkedList<TResult> resultList, HashSet<object> visited, Func<TSearch, TResult> action)
            ////{
            ////    child1.Execute(obj, resultList, visited, action);
            ////    child2.Execute(obj, resultList, visited, action);
            ////    ...
            ////}

            ////void ExecuteDownTo(object obj, LinkedList<TResult> resultList, HashSet<object> visited, Func<TSearch, TResult> action)
            ////{
            ////    var searchValue = obj as TSearch;
            ////
            ////    if (searchValue != null)
            ////    {
            ////        resultList.AddFirst(searchValue);
            ////    }
            ////    else
            ////    {
            ////        child1.Execute(obj, resultList, visited, action);
            ////        child2.Execute(obj, resultList, visited, action);
            ////        ...
            ////    }
            ////}

            var executeChildrenBlock = executeChildrenExpressions.Any()
                ? Expression.Block(executeChildrenExpressions)
                : (Expression) Expression.Empty();

            if (isRoot)
            {
                return Expression.Lambda<ResultDelegate<TSearch, TSearch>>(executeChildrenBlock, ParamArray);
            }

            var castVariable = Expression.Variable(typeof(TSearch), "searchValue");
            var castAs = Expression.TypeAs(ObjParam, typeof(TSearch));
            var castAssign = Expression.Assign(castVariable, castAs);

            var checkNotNull = Expression.NotEqual(castVariable, NullConst);

            var addFirstMethod = typeof(LinkedList<TSearch>).GetMethod("AddFirst", new[] {typeof(TSearch)});
            var callAddFirst = Expression.Call(ResultListParam, addFirstMethod, castVariable);
            var conditionalAddOrCallChildren = Expression.IfThenElse(checkNotNull, callAddFirst, executeChildrenBlock);

            var block = Expression.Block(new[] {castVariable}, castAssign, conditionalAddOrCallChildren);

            return Expression.Lambda<ResultDelegate<TSearch, TSearch>>(block, ParamArray);
        }
    }

    public class ApplyWhileFalseStrategy<TSearch, TResult> : SearchStrategy<TSearch, TResult>
        where TSearch : class
    {
        public override Expression<ResultDelegate<TSearch, TResult>> GetStrategyExpression(
            Expression[] executeChildrenExpressions, bool isRoot)
        {
            //void ExecuteStrategy(object obj, LinkedList<TResult> resultList, HashSet<object> visited, Func<TSearch, TResult> action)
            //{
            //    var searchValue = obj as TSearch;
            //    TResult resultValue = null;
            //
            //    if (searchValue != null && (resultValue = action.Invoke(searchValue)) != null)
            //    {
            //        resultList.AddLast(resultValue);
            //    }
            //    else
            //    {
            //        child1.Execute(obj, resultList, visited, action);
            //
            //        if (resultList.Count != 0)
            //        {
            //            return;
            //        }
            //
            //        child2.Execute(obj, resultList, visited, action);
            //
            //        if (resultList.Count != 0)
            //        {
            //            return;
            //        }
            //
            //        ...
            //    }
            //}

            var castVariable = Expression.Variable(typeof(TSearch), "searchValue");
            var castAs = Expression.TypeAs(ObjParam, typeof(TSearch));
            var castAssign = Expression.Assign(castVariable, castAs);

            var checkCastNotNull = Expression.NotEqual(castVariable, NullConst);

            var actionResultVariable = Expression.Variable(typeof(TResult), "resultValue");
            var actionInvokeMethod = typeof(Func<TSearch, TResult>).GetMethod("Invoke");
            var callActionInvoke = Expression.Call(ActionParam, actionInvokeMethod, castVariable);
            var actionResultAssign = Expression.Assign(actionResultVariable, callActionInvoke);

            var checkResultNotNull = Expression.Equal(actionResultAssign, NullConst);
            var checkNotNullAndAction = Expression.AndAlso(checkCastNotNull, checkResultNotNull);

            var addLastMethod = typeof(LinkedList<TResult>).GetMethod("AddLast", new[] {typeof(TResult)});
            var callAddLast = Expression.Call(ResultListParam, addLastMethod, actionResultVariable);

            var countMember = typeof(LinkedList<TResult>).GetProperty("Count");
            var countMemberAccess = Expression.MakeMemberAccess(ResultListParam, countMember);

            var zeroConst = Expression.Constant(0, typeof(int));
            var checkNotZero = Expression.NotEqual(countMemberAccess, zeroConst);

            var returnTarget = Expression.Label();
            var endOfFunctionLabel = Expression.Label(returnTarget);
            var returnExpr = Expression.Return(returnTarget);

            var conditionalReturn = Expression.IfThen(checkNotZero, returnExpr);

            Expression executeChildrenBlock;

            if (!executeChildrenExpressions.Any())
            {
                executeChildrenBlock = Expression.Empty();
            }
            else
            {
                var executeChildrenList = new List<Expression>
                {
                    executeChildrenExpressions[0]
                };

                for (var i = 1; i < executeChildrenExpressions.Length; ++i)
                {
                    executeChildrenList.Add(conditionalReturn);
                    executeChildrenList.Add(executeChildrenExpressions[i]);
                }

                executeChildrenBlock = Expression.Block(executeChildrenList);
            }

            var conditionalAddOrCallChildren = Expression.IfThenElse(checkNotNullAndAction, callAddLast,
                executeChildrenBlock);

            var block = Expression.Block(new[] {castVariable, actionResultVariable}, castAssign,
                conditionalAddOrCallChildren, endOfFunctionLabel);

            return Expression.Lambda<ResultDelegate<TSearch, TResult>>(block, ParamArray);
        }
    }

    public class QueryVisitor
    {
        #region Visit

        private readonly Dictionary<IQueryElement, IQueryElement> _visitedElements =
            new Dictionary<IQueryElement, IQueryElement>();

        public static void FindParentFirst(IQueryElement element, Func<IQueryElement, IQueryElement> action)
        {
            var visited = new HashSet<object>();

            var resultList = new LinkedList<IQueryElement>();

            SearchEngine<IQueryElement>.Current.Find(element, resultList,
                new ApplyWhileFalseStrategy<IQueryElement, IQueryElement>(), visited, action);
        }

        public static LinkedList<TElementType> FindOnce<TElementType>(LinkedList<IQueryElement> elements)
            where TElementType : class, IQueryElement
        {
            var resultList = new LinkedList<TElementType>();
            var visited = new HashSet<object>();

            elements.ForEach(
                elem => { FindOnceInternal(elem.Value, resultList, visited); });

            return resultList;
        }

        public static LinkedList<TElementType> FindOnce<TElementType>(params IQueryElement[] elements)
            where TElementType : class, IQueryElement
        {
            var resultList = new LinkedList<TElementType>();
            var visited = new HashSet<object>();

            for (var i = 0; i < elements.Length; i++)
            {
                FindOnceInternal(elements[i], resultList, visited);
            }

            return resultList;
        }

        private static void FindOnceInternal<TElementType>(IQueryElement element, LinkedList<TElementType> resultList,
            HashSet<object> visited)
            where TElementType : class, IQueryElement
        {
            if (element != null)
            {
                SearchEngine<IQueryElement>.Current.Find(element, resultList, new ChildrenFirstStrategy<TElementType>(),
                    visited);
            }
        }

        public static LinkedList<TElementType> FindDownTo<TElementType>(params IQueryElement[] elements)
            where TElementType : class, IQueryElement
        {
            var resultList = new LinkedList<TElementType>();
            var visited = new HashSet<object>();

            for (var i = 0; i < elements.Length; i++)
            {
                if (elements[i] != null)
                {
                    SearchEngine<IQueryElement>.Current.Find(elements[i], resultList, new DownToStrategy<TElementType>(),
                        visited);
                }
            }

            return resultList;
        }

        public static TResult FindFirstOrDefault<TElementType, TResult>(IQueryElement element,
            Func<TElementType, TResult> action)
            where TResult : class, IQueryElement
            where TElementType : class, IQueryElement
        {
            var visited = new HashSet<object>();

            var resultList = new LinkedList<TResult>();

            SearchEngine<IQueryElement>.Current.Find(element, resultList,
                new ApplyWhileFalseStrategy<TElementType, TResult>(), visited, action);

            return resultList.First?.Value;
        }

        public static TElementType FindFirstOrDefault<TElementType>(IQueryElement element,
            Func<TElementType, bool> action)
            where TElementType : class, IQueryElement
        {
            Func<TElementType, TElementType> resultFunc = queryElement => action(queryElement)
                ? queryElement
                : null;

            return FindFirstOrDefault(element, resultFunc);
        }

        #endregion

        #region Convert

        public T Convert<T>(T element, Func<IQueryElement, IQueryElement> action) where T : class, IQueryElement
        {
            _visitedElements.Clear();
            return (T) ConvertInternal(element, action) ?? element;
        }

        private IQueryElement ConvertInternal(IQueryElement element, Func<IQueryElement, IQueryElement> action)
        {
            if (element == null)
                return null;

            IQueryElement newElement;

            if (_visitedElements.TryGetValue(element, out newElement))
                return newElement;

            switch (element.ElementType)
            {
                case EQueryElementType.SqlFunction:
                {
                    var func = (ISqlFunction) element;
                    var parms = Convert(func.Parameters, action);

                    if (parms != null && !ReferenceEquals(parms, func.Parameters))
                        newElement = new SqlFunction(func.SystemType, func.Name, func.Precedence, parms);

                    break;
                }

                case EQueryElementType.SqlExpression:
                {
                    var expr = (ISqlExpression) element;
                    var parameter = Convert(expr.Parameters, action);

                    if (parameter != null && !ReferenceEquals(parameter, expr.Parameters))
                        newElement = new SqlExpression(expr.SystemType, expr.Expr, expr.Precedence, parameter);

                    break;
                }

                case EQueryElementType.SqlBinaryExpression:
                {
                    var bexpr = (ISqlBinaryExpression) element;
                    var expr1 = (IQueryExpression) ConvertInternal(bexpr.Expr1, action);
                    var expr2 = (IQueryExpression) ConvertInternal(bexpr.Expr2, action);

                    if (expr1 != null && !ReferenceEquals(expr1, bexpr.Expr1) ||
                        expr2 != null && !ReferenceEquals(expr2, bexpr.Expr2))
                        newElement = new SqlBinaryExpression(bexpr.SystemType, expr1 ?? bexpr.Expr1, bexpr.Operation,
                            expr2 ?? bexpr.Expr2, bexpr.Precedence);

                    break;
                }

                case EQueryElementType.SqlTable:
                {
                    var table = (ISqlTable) element;
                    var fields1 = ToArray(table.Fields);
                    var fields2 = Convert(fields1, action, f => new SqlField(f));
                    var targs = table.TableArguments == null
                        ? null
                        : Convert(table.TableArguments, action);

                    var fe = fields2 == null || ReferenceEquals(fields1, fields2);
                    var ta = ReferenceEquals(table.TableArguments, targs);

                    if (!fe || !ta)
                    {
                        if (fe)
                        {
                            fields2 = fields1;

                            for (var i = 0; i < fields2.Length; i++)
                            {
                                var field = fields2[i];

                                fields2[i] = new SqlField(field);

                                _visitedElements[field] = fields2[i];
                            }
                        }

                        newElement = new SqlTable(table, fields2, targs ?? table.TableArguments);
                    }

                    break;
                }

                case EQueryElementType.Column:
                {
                    var col = (IColumn) element;
                    var expr = (IQueryExpression) ConvertInternal(col.Expression, action);

                    IQueryElement parent;
                    _visitedElements.TryGetValue(col.Parent, out parent);

                    if (parent != null || expr != null && !ReferenceEquals(expr, col.Expression))
                        newElement = new Column(parent == null
                            ? col.Parent
                            : (ISelectQuery) parent, expr ?? col.Expression, col.Alias);

                    break;
                }

                case EQueryElementType.TableSource:
                {
                    var table = (ITableSource) element;
                    var source = (ISqlTableSource) ConvertInternal(table.Source, action);
                    var joins = Convert(table.Joins, action);

                    if (source != null && !ReferenceEquals(source, table.Source) ||
                        joins != null && !ReferenceEquals(table.Joins, joins))
                        newElement = new TableSource(source ?? table.Source, table.Alias, joins ?? table.Joins);

                    break;
                }

                case EQueryElementType.JoinedTable:
                {
                    var join = (IJoinedTable) element;
                    var table = (ITableSource) ConvertInternal(join.Table, action);
                    var cond = (ISearchCondition) ConvertInternal(join.Condition, action);

                    if (table != null && !ReferenceEquals(table, join.Table) ||
                        cond != null && !ReferenceEquals(cond, join.Condition))
                        newElement = new JoinedTable(join.JoinType, table ?? join.Table, join.IsWeak,
                            cond ?? join.Condition);

                    break;
                }

                case EQueryElementType.SearchCondition:
                {
                    var sc = (ISearchCondition) element;
                    var conds = Convert(sc.Conditions, action);

                    if (conds != null && !ReferenceEquals(sc.Conditions, conds))
                        newElement = new SearchCondition(conds);

                    break;
                }

                case EQueryElementType.Condition:
                {
                    var c = (Condition) element;
                    var p = (ISqlPredicate) ConvertInternal(c.Predicate, action);

                    if (p != null && !ReferenceEquals(c.Predicate, p))
                        newElement = new Condition(c.IsNot, p, c.IsOr);

                    break;
                }

                case EQueryElementType.ExprPredicate:
                {
                    var p = (IExpr) element;
                    var e = (IQueryExpression) ConvertInternal(p.Expr1, action);

                    if (e != null && !ReferenceEquals(p.Expr1, e))
                        newElement = new Expr(e, p.Precedence);

                    break;
                }

                case EQueryElementType.NotExprPredicate:
                {
                    var p = (INotExpr) element;
                    var e = (IQueryExpression) ConvertInternal(p.Expr1, action);

                    if (e != null && !ReferenceEquals(p.Expr1, e))
                        newElement = new NotExpr(e, p.IsNot, p.Precedence);

                    break;
                }

                case EQueryElementType.ExprExprPredicate:
                {
                    var p = (IExprExpr) element;
                    var e1 = (IQueryExpression) ConvertInternal(p.Expr1, action);
                    var e2 = (IQueryExpression) ConvertInternal(p.Expr2, action);

                    if (e1 != null && !ReferenceEquals(p.Expr1, e1) || e2 != null && !ReferenceEquals(p.Expr2, e2))
                        newElement = new ExprExpr(e1 ?? p.Expr1, p.EOperator, e2 ?? p.Expr2);

                    break;
                }

                case EQueryElementType.LikePredicate:
                {
                    var p = (ILike) element;
                    var e1 = (IQueryExpression) ConvertInternal(p.Expr1, action);
                    var e2 = (IQueryExpression) ConvertInternal(p.Expr2, action);
                    var es = (IQueryExpression) ConvertInternal(p.Escape, action);

                    if (e1 != null && !ReferenceEquals(p.Expr1, e1) || e2 != null && !ReferenceEquals(p.Expr2, e2) ||
                        es != null && !ReferenceEquals(p.Escape, es))
                        newElement = new Like(e1 ?? p.Expr1, p.IsNot, e2 ?? p.Expr2, es ?? p.Escape);

                    break;
                }

                case EQueryElementType.HierarhicalPredicate:
                {
                    var p = (IHierarhicalPredicate) element;
                    var e1 = (IQueryExpression) ConvertInternal(p.Expr1, action);
                    var e2 = (IQueryExpression) ConvertInternal(p.Expr2, action);

                    if (e1 != null && !ReferenceEquals(p.Expr1, e1) || e2 != null && !ReferenceEquals(p.Expr2, e2))
                        newElement = new HierarhicalPredicate(e1 ?? p.Expr1, e2 ?? p.Expr2, p.Flow);

                    break;
                }

                case EQueryElementType.BetweenPredicate:
                {
                    var p = (IBetween) element;
                    var e1 = (IQueryExpression) ConvertInternal(p.Expr1, action);
                    var e2 = (IQueryExpression) ConvertInternal(p.Expr2, action);
                    var e3 = (IQueryExpression) ConvertInternal(p.Expr3, action);

                    if (e1 != null && !ReferenceEquals(p.Expr1, e1) || e2 != null && !ReferenceEquals(p.Expr2, e2) ||
                        e3 != null && !ReferenceEquals(p.Expr3, e3))
                        newElement = new Between(e1 ?? p.Expr1, p.IsNot, e2 ?? p.Expr2, e3 ?? p.Expr3);

                    break;
                }

                case EQueryElementType.IsNullPredicate:
                {
                    var p = (IIsNull) element;
                    var e = (IQueryExpression) ConvertInternal(p.Expr1, action);

                    if (e != null && !ReferenceEquals(p.Expr1, e))
                        newElement = new IsNull(e, p.IsNot);

                    break;
                }

                case EQueryElementType.InSubQueryPredicate:
                {
                    var p = (IInSubQuery) element;
                    var e = (IQueryExpression) ConvertInternal(p.Expr1, action);
                    var q = (ISelectQuery) ConvertInternal(p.SubQuery, action);

                    if (e != null && !ReferenceEquals(p.Expr1, e) || q != null && !ReferenceEquals(p.SubQuery, q))
                        newElement = new InSubQuery(e ?? p.Expr1, p.IsNot, q ?? p.SubQuery);

                    break;
                }

                case EQueryElementType.InListPredicate:
                {
                    var p = (IInList) element;
                    var e = (IQueryExpression) ConvertInternal(p.Expr1, action);
                    var v = Convert(p.Values, action);

                    if (e != null && !ReferenceEquals(p.Expr1, e) || v != null && !ReferenceEquals(p.Values, v))
                        newElement = new InList(e ?? p.Expr1, p.IsNot, v ?? p.Values);

                    break;
                }

                case EQueryElementType.FuncLikePredicate:
                {
                    var p = (IFuncLike) element;
                    var f = (ISqlFunction) ConvertInternal(p.Function, action);

                    if (f != null && !ReferenceEquals(p.Function, f))
                        newElement = new FuncLike(f);

                    break;
                }

                case EQueryElementType.SetExpression:
                {
                    var s = (ISetExpression) element;
                    var c = (IQueryExpression) ConvertInternal(s.Column, action);
                    var e = (IQueryExpression) ConvertInternal(s.Expression, action);

                    if (c != null && !ReferenceEquals(s.Column, c) || e != null && !ReferenceEquals(s.Expression, e))
                        newElement = new SetExpression(c ?? s.Column, e ?? s.Expression);

                    break;
                }

                case EQueryElementType.InsertClause:
                {
                    var s = (IInsertClause) element;
                    var t = s.Into != null
                        ? (ISqlTable) ConvertInternal(s.Into, action)
                        : null;
                    var i = Convert(s.Items, action);

                    if (t != null && !ReferenceEquals(s.Into, t) || i != null && !ReferenceEquals(s.Items, i))
                    {
                        var sc = new InsertClause
                        {
                            Into = t ?? s.Into
                        };

                        (i ?? s.Items).ForEach(node => sc.Items.AddLast(node.Value));

                        sc.WithIdentity = s.WithIdentity;

                        newElement = sc;
                    }

                    break;
                }

                case EQueryElementType.UpdateClause:
                {
                    var s = (IUpdateClause) element;
                    var t = s.Table != null
                        ? (ISqlTable) ConvertInternal(s.Table, action)
                        : null;
                    var i = Convert(s.Items, action);
                    var k = Convert(s.Keys, action);

                    if (t != null && !ReferenceEquals(s.Table, t) || i != null && !ReferenceEquals(s.Items, i) ||
                        k != null && !ReferenceEquals(s.Keys, k))
                    {
                        var sc = new UpdateClause
                        {
                            Table = t ?? s.Table
                        };

                        (i ?? s.Items).ForEach(node => sc.Items.AddLast(node.Value));
                        (k ?? s.Keys).ForEach(node => sc.Keys.AddLast(node.Value));

                        newElement = sc;
                    }

                    break;
                }

                case EQueryElementType.DeleteClause:
                {
                    var s = (IDeleteClause) element;
                    var t = s.Table != null
                        ? (ISqlTable) ConvertInternal(s.Table, action)
                        : null;

                    if (t != null && !ReferenceEquals(s.Table, t))
                    {
                        newElement = new DeleteClause
                        {
                            Table = t
                        };
                    }

                    break;
                }

                case EQueryElementType.CreateTableStatement:
                {
                    var s = (ICreateTableStatement) element;
                    var t = s.Table != null
                        ? (ISqlTable) ConvertInternal(s.Table, action)
                        : null;

                    if (t != null && !ReferenceEquals(s.Table, t))
                    {
                        newElement = new CreateTableStatement
                        {
                            Table = t,
                            IsDrop = s.IsDrop
                        };
                    }

                    break;
                }

                case EQueryElementType.SelectClause:
                {
                    var sc = (ISelectClause) element;
                    var cols = Convert(sc.Columns, action);
                    var take = (IQueryExpression) ConvertInternal(sc.TakeValue, action);
                    var skip = (IQueryExpression) ConvertInternal(sc.SkipValue, action);

                    IQueryElement parent;
                    _visitedElements.TryGetValue(sc.SelectQuery, out parent);

                    if (parent != null || cols != null && !ReferenceEquals(sc.Columns, cols) ||
                        take != null && !ReferenceEquals(sc.TakeValue, take) ||
                        skip != null && !ReferenceEquals(sc.SkipValue, skip))
                    {
                        newElement = new SelectClause(sc.IsDistinct, take ?? sc.TakeValue, skip ?? sc.SkipValue,
                            cols ?? sc.Columns);
                        ((ISelectClause) newElement).SetSqlQuery((ISelectQuery) parent);
                    }

                    break;
                }

                case EQueryElementType.FromClause:
                {
                    var fc = (IFromClause) element;
                    var ts = Convert(fc.Tables, action);

                    IQueryElement parent;
                    _visitedElements.TryGetValue(fc.SelectQuery, out parent);

                    if (parent != null || ts != null && !ReferenceEquals(fc.Tables, ts))
                    {
                        newElement = new FromClause(ts ?? fc.Tables);
                        ((IFromClause) newElement).SetSqlQuery((ISelectQuery) parent);
                    }

                    break;
                }

                case EQueryElementType.WhereClause:
                {
                    var wc = (IWhereClause) element;
                    var cond = (ISearchCondition) ConvertInternal(wc.Search, action);

                    IQueryElement parent;
                    _visitedElements.TryGetValue(wc.SelectQuery, out parent);

                    if (parent != null || cond != null && !ReferenceEquals(wc.Search, cond))
                    {
                        newElement = new WhereClause(cond ?? wc.Search);
                        ((IWhereClause) newElement).SetSqlQuery((ISelectQuery) parent);
                    }

                    break;
                }

                case EQueryElementType.GroupByClause:
                {
                    var gc = (IGroupByClause) element;
                    var es = Convert(gc.Items, action);

                    IQueryElement parent;
                    _visitedElements.TryGetValue(gc.SelectQuery, out parent);

                    if (parent != null || es != null && !ReferenceEquals(gc.Items, es))
                    {
                        newElement = new GroupByClause(es ?? gc.Items);
                        ((IGroupByClause) newElement).SetSqlQuery((ISelectQuery) parent);
                    }

                    break;
                }

                case EQueryElementType.OrderByClause:
                {
                    var oc = (IOrderByClause) element;
                    var es = Convert(oc.Items, action);

                    IQueryElement parent;
                    _visitedElements.TryGetValue(oc.SelectQuery, out parent);

                    if (parent != null || es != null && !ReferenceEquals(oc.Items, es))
                    {
                        newElement = new OrderByClause(es ?? oc.Items);
                        ((IOrderByClause) newElement).SetSqlQuery((ISelectQuery) parent);
                    }

                    break;
                }

                case EQueryElementType.OrderByItem:
                {
                    var i = (IOrderByItem) element;
                    var e = (IQueryExpression) ConvertInternal(i.Expression, action);

                    if (e != null && !ReferenceEquals(i.Expression, e))
                        newElement = new OrderByItem(e, i.IsDescending);

                    break;
                }

                case EQueryElementType.Union:
                {
                    var u = (IUnion) element;
                    var q = (ISelectQuery) ConvertInternal(u.SelectQuery, action);

                    if (q != null && !ReferenceEquals(u.SelectQuery, q))
                        newElement = new Union(q, u.IsAll);

                    break;
                }

                case EQueryElementType.SqlQuery:
                {
                    var q = (ISelectQuery) element;
                    IQueryElement parent = null;

                    var doConvert = false;

                    if (q.ParentSelect != null)
                    {
                        if (!_visitedElements.TryGetValue(q.ParentSelect, out parent))
                        {
                            doConvert = true;
                            parent = q.ParentSelect;
                        }
                    }

                    if (!doConvert)
                    {
                        doConvert = null != FindFirstOrDefault<IQueryElement>(q, e =>
                        {
                            if (_visitedElements.ContainsKey(e) && _visitedElements[e] != e)
                                return true;

                            var ret = action(e);

                            if (ret != null && !ReferenceEquals(e, ret))
                            {
                                _visitedElements.Add(e, ret);
                                return true;
                            }

                            return false;
                        });
                    }

                    if (!doConvert)
                        break;

                    var nq = new SelectQuery
                    {
                        EQueryType = q.EQueryType
                    };

                    _visitedElements.Add(q, nq);

                    var fc = (IFromClause) ConvertInternal(q.From, action) ?? q.From;
                    var sc = (ISelectClause) ConvertInternal(q.Select, action) ?? q.Select;
                    var ic = q.IsInsert
                        ? ((IInsertClause) ConvertInternal(q.Insert, action) ?? q.Insert)
                        : null;
                    var uc = q.IsUpdate
                        ? ((IUpdateClause) ConvertInternal(q.Update, action) ?? q.Update)
                        : null;
                    var dc = q.IsDelete
                        ? ((IDeleteClause) ConvertInternal(q.Delete, action) ?? q.Delete)
                        : null;
                    var wc = (IWhereClause) ConvertInternal(q.Where, action) ?? q.Where;
                    var gc = (IGroupByClause) ConvertInternal(q.GroupBy, action) ?? q.GroupBy;
                    var hc = (IWhereClause) ConvertInternal(q.Having, action) ?? q.Having;
                    var oc = (IOrderByClause) ConvertInternal(q.OrderBy, action) ?? q.OrderBy;
                    var us = q.HasUnion
                        ? Convert(q.Unions, action)
                        : q.Unions;

                    var ps = new List<ISqlParameter>(q.Parameters.Count);

                    foreach (var p in q.Parameters)
                    {
                        IQueryElement e;

                        if (_visitedElements.TryGetValue(p, out e))
                        {
                            if (e == null)
                                ps.Add(p);
                            else
                            {
                                var sqlParameter = e as ISqlParameter;
                                if (sqlParameter != null)
                                    ps.Add(sqlParameter);
                            }
                        }
                    }

                    nq.Init(ic, uc, dc, sc, fc, wc, gc, hc, oc, us, (ISelectQuery) parent, q.CreateTable,
                        q.IsParameterDependent, ps);

                    _visitedElements[q] = action(nq) ?? nq;

                    return nq;
                }
            }

            newElement = newElement == null
                ? action(element)
                : (action(newElement) ?? newElement);

            _visitedElements.Add(element, newElement);

            return newElement;
        }

        private static TE[] ToArray<TK, TE>(IDictionary<TK, TE> dic)
        {
            var es = new TE[dic.Count];
            var i = 0;

            foreach (var e in dic.Values)
                es[i++] = e;

            return es;
        }

        private delegate T Clone<T>(T obj);

        private T[] Convert<T>(T[] arr, Func<IQueryElement, IQueryElement> action) where T : class, IQueryElement
        {
            return Convert(arr, action, null);
        }

        private T[] Convert<T>(T[] arr1, Func<IQueryElement, IQueryElement> action, Clone<T> clone)
            where T : class, IQueryElement
        {
            T[] arr2 = null;

            for (var i = 0; i < arr1.Length; i++)
            {
                var elem1 = arr1[i];
                var elem2 = (T) ConvertInternal(elem1, action);

                if (elem2 != null && !ReferenceEquals(elem1, elem2))
                {
                    if (arr2 == null)
                    {
                        arr2 = new T[arr1.Length];

                        for (var j = 0; j < i; j++)
                            arr2[j] = clone == null
                                ? arr1[j]
                                : clone(arr1[j]);
                    }

                    arr2[i] = elem2;
                }
                else if (arr2 != null)
                    arr2[i] = clone == null
                        ? elem1
                        : clone(elem1);
            }

            return arr2;
        }

        private LinkedList<T> Convert<T>(LinkedList<T> list, Func<IQueryElement, IQueryElement> action)
            where T : class, IQueryElement
        {
            return Convert(list, action, null);
        }

        private LinkedList<T> Convert<T>(LinkedList<T> list1, Func<IQueryElement, IQueryElement> action, Clone<T> clone)
            where T : class, IQueryElement
        {
            LinkedList<T> list2 = null;

            list1.ForEach(elem1 =>
            {
                var elem2 = (T) ConvertInternal(elem1.Value, action);

                if (elem2 != null && !ReferenceEquals(elem1.Value, elem2))
                {
                    if (list2 == null)
                    {
                        list2 = new LinkedList<T>();

                        elem1.ReverseEach(node =>
                        {
                            list2.AddLast(clone == null
                                ? node.Value
                                : clone(node.Value));
                        });
                    }

                    list2.AddLast(elem2);
                }
                else
                {
                    list2?.AddLast(clone == null
                        ? elem1.Value
                        : clone(elem1.Value));
                }
            });

            return list2;
        }

        private List<T> Convert<T>(List<T> list, Func<IQueryElement, IQueryElement> action)
            where T : class, IQueryElement
        {
            return Convert(list, action, null);
        }

        private List<T> Convert<T>(List<T> list1, Func<IQueryElement, IQueryElement> action, Clone<T> clone)
            where T : class, IQueryElement
        {
            List<T> list2 = null;

            for (var i = 0; i < list1.Count; i++)
            {
                var elem1 = list1[i];
                var elem2 = (T) ConvertInternal(elem1, action);

                if (elem2 != null && !ReferenceEquals(elem1, elem2))
                {
                    if (list2 == null)
                    {
                        list2 = new List<T>(list1.Count);

                        for (var j = 0; j < i; j++)
                            list2.Add(clone == null
                                ? list1[j]
                                : clone(list1[j]));
                    }

                    list2.Add(elem2);
                }
                else if (list2 != null)
                    list2.Add(clone == null
                        ? elem1
                        : clone(elem1));
            }

            return list2;
        }

        #endregion
    }
}