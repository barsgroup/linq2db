using System;
using System.Collections.Generic;

namespace LinqToDB.SqlQuery
{
    using System.Linq;

    using LinqToDB.SqlQuery.QueryElements;
    using LinqToDB.SqlQuery.QueryElements.Clauses;
    using LinqToDB.SqlQuery.QueryElements.Clauses.Interfaces;
    using LinqToDB.SqlQuery.QueryElements.Conditions;
    using LinqToDB.SqlQuery.QueryElements.Conditions.Interfaces;
    using LinqToDB.SqlQuery.QueryElements.Enums;
    using LinqToDB.SqlQuery.QueryElements.Interfaces;
    using LinqToDB.SqlQuery.QueryElements.Predicates;
    using LinqToDB.SqlQuery.QueryElements.Predicates.Interfaces;
    using LinqToDB.SqlQuery.QueryElements.SqlElements;
    using LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces;

    // The casts to object in the below code are an unfortunate necessity due to
    // C#'s restriction against a where T : Enum constraint. (There are ways around
    // this, but they're outside the scope of this simple illustration.)

    public class QueryVisitor
	{
		#region Visit

		readonly Dictionary<IQueryElement,IQueryElement> _visitedElements = new Dictionary<IQueryElement, IQueryElement>();

        public void VisitParentFirst(IQueryElement element, Func<IQueryElement, bool> action)
        {
            element.GetSelfWithChildren().Any(childElement => !action(childElement));
        }

        public static IEnumerable<TElementType> FindOnce<TElementType>(IQueryElement element, Dictionary<IQueryElement, IQueryElement> existItems = null) where TElementType : IQueryElement
        {
            if (existItems == null)
            {
                existItems = new Dictionary<IQueryElement, IQueryElement>();
            }

            Func<IEnumerable<IQueryElement>, IEnumerable<IQueryElement>> distinct = source =>
            {
                return source.Where(
                    el =>
                    {
                        if (el != null && !existItems.ContainsKey(el))
                        {
                            existItems.Add(el, el);
                            return true;
                        }
                        return false;
                    });
            };

            return distinct(element.GetSelfWithChildren()).OfType<TElementType>();
        }

        public static IEnumerable<TElementType> FindAll<TElementType>(params IQueryElement[] elements) where TElementType : IQueryElement
        {
            return elements.Where(e => e != null).SelectMany(qe => qe.GetSelfWithChildren()).OfType<TElementType>();
        }

        #endregion

        #region Find

        IQueryElement Find<T>(IEnumerable<T> arr, Func<IQueryElement, bool> find)
			where T : class, IQueryElement
		{
			if (arr == null)
				return null;

			foreach (var item in arr)
			{
				var e = Find(item, find);
				if (e != null)
					return e;
			}

			return null;
		}

		public IQueryElement Find(IQueryElement element, Func<IQueryElement, bool> find)
		{
			if (element == null || find(element))
				return element;

			switch (element.ElementType)
			{
				case EQueryElementType.SqlFunction       : return Find(((SqlFunction)                element).Parameters,      find);
				case EQueryElementType.SqlExpression     : return Find(((SqlExpression)              element).Parameters,      find);
				case EQueryElementType.Column            : return Find(((IColumn)            element).Expression,      find);
				case EQueryElementType.SearchCondition   : return Find(((ISearchCondition)   element).Conditions,      find);
				case EQueryElementType.Condition         : return Find(((ICondition)         element).Predicate,       find);
				case EQueryElementType.ExprPredicate     : return Find(((IExpr)    element).Expr1,           find);
				case EQueryElementType.NotExprPredicate  : return Find(((INotExpr) element).Expr1,           find);
				case EQueryElementType.IsNullPredicate   : return Find(((IIsNull)  element).Expr1,           find);
				case EQueryElementType.FromClause        : return Find(((IFromClause)        element).Tables,          find);
				case EQueryElementType.WhereClause       : return Find(((IWhereClause)       element).SearchCondition, find);
				case EQueryElementType.GroupByClause     : return Find(((GroupByClause)     element).Items,           find);
				case EQueryElementType.OrderByClause     : return Find(((IOrderByClause)     element).Items,           find);
				case EQueryElementType.OrderByItem       : return Find(((IOrderByItem)       element).Expression,      find);
				case EQueryElementType.Union             : return Find(((IUnion)             element).SelectQuery,        find);
				case EQueryElementType.FuncLikePredicate : return Find(((IFuncLike)element).Function,        find);

				case EQueryElementType.SqlBinaryExpression:
					{
						var bexpr = (SqlBinaryExpression)element;
						return
							Find(bexpr.Expr1, find) ??
							Find(bexpr.Expr2, find);
					}

				case EQueryElementType.SqlTable:
					{
						var table = (SqlTable)element;
						return
							Find(table.All,            find) ??
							Find(table.Fields.Values,  find) ??
							Find(table.TableArguments, find);
					}

				case EQueryElementType.TableSource:
					{
						var table = (ITableSource)element;
						return
							Find(table.Source, find) ??
							Find(table.Joins,  find);
					}

				case EQueryElementType.JoinedTable:
					{
						var join = (IJoinedTable)element;
						return
							Find(join.Table,     find) ??
							Find(join.Condition, find);
					}

				case EQueryElementType.ExprExprPredicate:
					{
						var p = (IExprExpr)element;
						return
							Find(p.Expr1, find) ??
							Find(p.Expr2, find);
					}

				case EQueryElementType.LikePredicate:
					{
						var p = (ILike)element;
						return
							Find(p.Expr1,  find) ??
							Find(p.Expr2,  find) ??
							Find(p.Escape, find);
					}

				case EQueryElementType.BetweenPredicate:
					{
						var p = (Between)element;
						return
							Find(p.Expr1, find) ??
							Find(p.Expr2, find) ??
							Find(p.Expr3, find);
					}

				case EQueryElementType.InSubQueryPredicate:
					{
						var p = (IInSubQuery)element;
						return
							Find(p.Expr1,    find) ??
							Find(p.SubQuery, find);
					}

				case EQueryElementType.InListPredicate:
					{
						var p = (IInList)element;
						return
							Find(p.Expr1,  find) ??
							Find(p.Values, find);
					}

				case EQueryElementType.SetExpression:
					{
						var s = (ISetExpression)element;
						return
							Find(s.Column,     find) ??
							Find(s.Expression, find);
					}

				case EQueryElementType.InsertClause:
					{
						var sc = (IInsertClause)element;
						return
							Find(sc.Into,  find) ??
							Find(sc.Items, find);
					}

				case EQueryElementType.UpdateClause:
					{
						var sc = (IUpdateClause)element;
						return
							Find(sc.Table, find) ??
							Find(sc.Items, find) ??
							Find(sc.Keys,  find);
					}

				case EQueryElementType.DeleteClause:
					{
						var sc = (DeleteClause)element;
						return Find(sc.Table, find);
					}

				case EQueryElementType.CreateTableStatement:
					{
						var sc = (ICreateTableStatement)element;
						return
							Find(sc.Table, find);
					}

				case EQueryElementType.SelectClause:
					{
						var sc = (ISelectClause)element;
						return
							Find(sc.TakeValue, find) ??
							Find(sc.SkipValue, find) ??
							Find(sc.Columns,   find);
					}

				case EQueryElementType.SqlQuery:
					{
						var q = (ISelectQuery)element;
						return
							Find(q.Select,  find) ??
							(q.IsInsert ? Find(q.Insert, find) : null) ??
							(q.IsUpdate ? Find(q.Update, find) : null) ??
							Find(q.From,    find) ??
							Find(q.Where,   find) ??
							Find(q.GroupBy, find) ??
							Find(q.Having,  find) ??
							Find(q.OrderBy, find) ??
							(q.HasUnion ? Find(q.Unions, find) : null);
					}
			}

			return null;
		}

		#endregion

		#region Convert

		public T Convert<T>(T element, Func<IQueryElement,IQueryElement> action)
			where T : class, IQueryElement
		{
			_visitedElements.Clear();
			return (T)ConvertInternal(element, action) ?? element;
		}

		IQueryElement ConvertInternal(IQueryElement element, Func<IQueryElement,IQueryElement> action)
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
						var func  = (SqlFunction)element;
						var parms = Convert(func.Parameters, action);

						if (parms != null && !ReferenceEquals(parms, func.Parameters))
							newElement = new SqlFunction(func.SystemType, func.Name, func.Precedence, parms);

						break;
					}

				case EQueryElementType.SqlExpression:
					{
						var expr      = (SqlExpression)element;
						var parameter = Convert(expr.Parameters, action);

						if (parameter != null && !ReferenceEquals(parameter, expr.Parameters))
							newElement = new SqlExpression(expr.SystemType, expr.Expr, expr.Precedence, parameter);

						break;
					}

				case EQueryElementType.SqlBinaryExpression:
					{
						var bexpr = (SqlBinaryExpression)element;
						var expr1 = (ISqlExpression)ConvertInternal(bexpr.Expr1, action);
						var expr2 = (ISqlExpression)ConvertInternal(bexpr.Expr2, action);

						if (expr1 != null && !ReferenceEquals(expr1, bexpr.Expr1) ||
							expr2 != null && !ReferenceEquals(expr2, bexpr.Expr2))
							newElement = new SqlBinaryExpression(bexpr.SystemType, expr1 ?? bexpr.Expr1, bexpr.Operation, expr2 ?? bexpr.Expr2, bexpr.Precedence);

						break;
					}

				case EQueryElementType.SqlTable:
					{
						var table   = (SqlTable)element;
						var fields1 = ToArray(table.Fields);
						var fields2 = Convert(fields1,     action, f => new SqlField(f));
						var targs   = table.TableArguments == null ? null : Convert(table.TableArguments, action);

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

							_visitedElements[((SqlTable)newElement).All] = table.All;
						}

						break;
					}

				case EQueryElementType.Column:
					{
						var col  = (IColumn)element;
						var expr = (ISqlExpression)ConvertInternal(col.Expression, action);

						IQueryElement parent;
						_visitedElements.TryGetValue(col.Parent, out parent);

						if (parent != null || expr != null && !ReferenceEquals(expr, col.Expression))
							newElement = new Column(parent == null ? col.Parent : (ISelectQuery)parent, expr ?? col.Expression, col.Alias);

						break;
					}

				case EQueryElementType.TableSource:
					{
						var table  = (ITableSource)element;
						var source = (ISqlTableSource)ConvertInternal(table.Source, action);
						var joins  = Convert(table.Joins, action);

						if (source != null && !ReferenceEquals(source, table.Source) ||
							joins  != null && !ReferenceEquals(table.Joins, joins))
							newElement = new TableSource(source ?? table.Source, table.Alias, joins ?? table.Joins);

						break;
					}

				case EQueryElementType.JoinedTable:
					{
						var join  = (IJoinedTable)element;
						var table = (ITableSource)    ConvertInternal(join.Table,     action);
						var cond  = (ISearchCondition)ConvertInternal(join.Condition, action);

						if (table != null && !ReferenceEquals(table, join.Table) ||
							cond  != null && !ReferenceEquals(cond,  join.Condition))
							newElement = new JoinedTable(join.JoinType, table ?? join.Table, join.IsWeak, cond ?? join.Condition);

						break;
					}

				case EQueryElementType.SearchCondition:
					{
						var sc    = (ISearchCondition)element;
						var conds = Convert(sc.Conditions, action);

						if (conds != null && !ReferenceEquals(sc.Conditions, conds))
							newElement = new SearchCondition(conds);

						break;
					}

				case EQueryElementType.Condition:
					{
						var c = (Condition)element;
						var p = (ISqlPredicate)ConvertInternal(c.Predicate, action);

						if (p != null && !ReferenceEquals(c.Predicate, p))
							newElement = new Condition(c.IsNot, p, c.IsOr);

						break;
					}

				case EQueryElementType.ExprPredicate:
					{
						var p = (IExpr)element;
						var e = (ISqlExpression)ConvertInternal(p.Expr1, action);

						if (e != null && !ReferenceEquals(p.Expr1, e))
							newElement = new Expr(e, p.Precedence);

						break;
					}

				case EQueryElementType.NotExprPredicate:
					{
						var p = (INotExpr)element;
						var e = (ISqlExpression)ConvertInternal(p.Expr1, action);

						if (e != null && !ReferenceEquals(p.Expr1, e))
							newElement = new NotExpr(e, p.IsNot, p.Precedence);

						break;
					}

				case EQueryElementType.ExprExprPredicate:
					{
						var p  = (IExprExpr)element;
						var e1 = (ISqlExpression)ConvertInternal(p.Expr1, action);
						var e2 = (ISqlExpression)ConvertInternal(p.Expr2, action);

						if (e1 != null && !ReferenceEquals(p.Expr1, e1) || e2 != null && !ReferenceEquals(p.Expr2, e2))
							newElement = new ExprExpr(e1 ?? p.Expr1, p.EOperator, e2 ?? p.Expr2);

						break;
					}

				case EQueryElementType.LikePredicate:
					{
						var p  = (ILike)element;
						var e1 = (ISqlExpression)ConvertInternal(p.Expr1,  action);
						var e2 = (ISqlExpression)ConvertInternal(p.Expr2,  action);
						var es = (ISqlExpression)ConvertInternal(p.Escape, action);

						if (e1 != null && !ReferenceEquals(p.Expr1, e1) ||
							e2 != null && !ReferenceEquals(p.Expr2, e2) ||
							es != null && !ReferenceEquals(p.Escape, es))
							newElement = new Like(e1 ?? p.Expr1, p.IsNot, e2 ?? p.Expr2, es ?? p.Escape);

						break;
					}

				case EQueryElementType.BetweenPredicate:
					{
						var p = (Between)element;
						var e1 = (ISqlExpression)ConvertInternal(p.Expr1, action);
						var e2 = (ISqlExpression)ConvertInternal(p.Expr2, action);
						var e3 = (ISqlExpression)ConvertInternal(p.Expr3, action);

						if (e1 != null && !ReferenceEquals(p.Expr1, e1) ||
							e2 != null && !ReferenceEquals(p.Expr2, e2) ||
							e3 != null && !ReferenceEquals(p.Expr3, e3))
							newElement = new Between(e1 ?? p.Expr1, p.IsNot, e2 ?? p.Expr2, e3 ?? p.Expr3);

						break;
					}

				case EQueryElementType.IsNullPredicate:
					{
						var p = (IIsNull)element;
						var e = (ISqlExpression)ConvertInternal(p.Expr1, action);

						if (e != null && !ReferenceEquals(p.Expr1, e))
							newElement = new IsNull(e, p.IsNot);

						break;
					}

				case EQueryElementType.InSubQueryPredicate:
					{
						var p = (IInSubQuery)element;
						var e = (ISqlExpression)ConvertInternal(p.Expr1,    action);
						var q = (ISelectQuery)ConvertInternal(p.SubQuery, action);

						if (e != null && !ReferenceEquals(p.Expr1, e) || q != null && !ReferenceEquals(p.SubQuery, q))
							newElement = new InSubQuery(e ?? p.Expr1, p.IsNot, q ?? p.SubQuery);

						break;
					}

				case EQueryElementType.InListPredicate:
					{
						var p = (IInList)element;
						var e = (ISqlExpression)ConvertInternal(p.Expr1,    action);
						var v = Convert(p.Values, action);

						if (e != null && !ReferenceEquals(p.Expr1, e) || v != null && !ReferenceEquals(p.Values, v))
							newElement = new InList(e ?? p.Expr1, p.IsNot, v ?? p.Values);

						break;
					}

				case EQueryElementType.FuncLikePredicate:
					{
						var p = (IFuncLike)element;
						var f = (SqlFunction)ConvertInternal(p.Function, action);

						if (f != null && !ReferenceEquals(p.Function, f))
							newElement = new FuncLike(f);

						break;
					}

				case EQueryElementType.SetExpression:
					{
						var s = (ISetExpression)element;
						var c = (ISqlExpression)ConvertInternal(s.Column,     action);
						var e = (ISqlExpression)ConvertInternal(s.Expression, action);

						if (c != null && !ReferenceEquals(s.Column, c) || e != null && !ReferenceEquals(s.Expression, e))
							newElement = new SetExpression(c ?? s.Column, e ?? s.Expression);

						break;
					}

				case EQueryElementType.InsertClause:
					{
						var s = (IInsertClause)element;
						var t = s.Into != null ? (SqlTable)ConvertInternal(s.Into, action) : null;
						var i = Convert(s.Items, action);

						if (t != null && !ReferenceEquals(s.Into, t) || i != null && !ReferenceEquals(s.Items, i))
						{
							var sc = new InsertClause { Into = t ?? s.Into };

							sc.Items.AddRange(i ?? s.Items);
							sc.WithIdentity = s.WithIdentity;

							newElement = sc;
						}

						break;
					}

				case EQueryElementType.UpdateClause:
					{
						var s = (IUpdateClause)element;
						var t = s.Table != null ? (SqlTable)ConvertInternal(s.Table, action) : null;
						var i = Convert(s.Items, action);
						var k = Convert(s.Keys,  action);

						if (t != null && !ReferenceEquals(s.Table, t) ||
							i != null && !ReferenceEquals(s.Items, i) ||
							k != null && !ReferenceEquals(s.Keys,  k))
						{
							var sc = new UpdateClause { Table = t ?? s.Table };

							sc.Items.AddRange(i ?? s.Items);
							sc.Keys. AddRange(k ?? s.Keys);

							newElement = sc;
						}

						break;
					}

				case EQueryElementType.DeleteClause:
					{
						var s = (DeleteClause)element;
						var t = s.Table != null ? (SqlTable)ConvertInternal(s.Table, action) : null;

						if (t != null && !ReferenceEquals(s.Table, t))
						{
							newElement = new DeleteClause { Table = t };
						}

						break;
					}

				case EQueryElementType.CreateTableStatement:
					{
						var s = (ICreateTableStatement)element;
						var t = s.Table != null ? (SqlTable)ConvertInternal(s.Table, action) : null;

						if (t != null && !ReferenceEquals(s.Table, t))
						{
							newElement = new CreateTableStatement { Table = t, IsDrop = s.IsDrop };
						}

						break;
					}

				case EQueryElementType.SelectClause:
					{
						var sc   = (ISelectClause)element;
						var cols = Convert(sc.Columns, action);
						var take = (ISqlExpression)ConvertInternal(sc.TakeValue, action);
						var skip = (ISqlExpression)ConvertInternal(sc.SkipValue, action);

						IQueryElement parent;
						_visitedElements.TryGetValue(sc.SelectQuery, out parent);

						if (parent != null ||
							cols != null && !ReferenceEquals(sc.Columns,   cols) ||
							take != null && !ReferenceEquals(sc.TakeValue, take) ||
							skip != null && !ReferenceEquals(sc.SkipValue, skip))
						{
							newElement = new SelectClause(sc.IsDistinct, take ?? sc.TakeValue, skip ?? sc.SkipValue, cols ?? sc.Columns);
							((ISelectClause)newElement).SetSqlQuery((ISelectQuery)parent);
						}

						break;
					}

				case EQueryElementType.FromClause:
					{
						var fc   = (IFromClause)element;
						var ts = Convert(fc.Tables, action);

						IQueryElement parent;
						_visitedElements.TryGetValue(fc.SelectQuery, out parent);

						if (parent != null || ts != null && !ReferenceEquals(fc.Tables, ts))
						{
							newElement = new FromClause(ts ?? fc.Tables);
							((IFromClause)newElement).SetSqlQuery((ISelectQuery)parent);
						}

						break;
					}

				case EQueryElementType.WhereClause:
					{
						var wc   = (IWhereClause)element;
						var cond = (ISearchCondition)ConvertInternal(wc.SearchCondition, action);

						IQueryElement parent;
						_visitedElements.TryGetValue(wc.SelectQuery, out parent);

						if (parent != null || cond != null && !ReferenceEquals(wc.SearchCondition, cond))
						{
							newElement = new WhereClause(cond ?? wc.SearchCondition);
							((IWhereClause)newElement).SetSqlQuery((ISelectQuery)parent);
						}

						break;
					}

				case EQueryElementType.GroupByClause:
					{
						var gc = (GroupByClause)element;
						var es = Convert(gc.Items, action);

						IQueryElement parent;
						_visitedElements.TryGetValue(gc.SelectQuery, out parent);

						if (parent != null || es != null && !ReferenceEquals(gc.Items, es))
						{
							newElement = new GroupByClause(es ?? gc.Items);
							((GroupByClause)newElement).SetSqlQuery((ISelectQuery)parent);
						}

						break;
					}

				case EQueryElementType.OrderByClause:
					{
						var oc = (IOrderByClause)element;
						var es = Convert(oc.Items, action);

						IQueryElement parent;
						_visitedElements.TryGetValue(oc.SelectQuery, out parent);

						if (parent != null || es != null && !ReferenceEquals(oc.Items, es))
						{
							newElement = new OrderByClause(es ?? oc.Items);
							((IOrderByClause)newElement).SetSqlQuery((ISelectQuery)parent);
						}

						break;
					}

				case EQueryElementType.OrderByItem:
					{
						var i = (IOrderByItem)element;
						var e = (ISqlExpression)ConvertInternal(i.Expression, action);

						if (e != null && !ReferenceEquals(i.Expression, e))
							newElement = new OrderByItem(e, i.IsDescending);

						break;
					}

				case EQueryElementType.Union:
					{
						var u = (IUnion)element;
						var q = (ISelectQuery)ConvertInternal(u.SelectQuery, action);

						if (q != null && !ReferenceEquals(u.SelectQuery, q))
							newElement = new Union(q, u.IsAll);

						break;
					}

				case EQueryElementType.SqlQuery:
					{
						var q = (ISelectQuery)element;
						IQueryElement parent = null;

						var doConvert = false;

						if (q.ParentSelect != null)
						{
							if (!_visitedElements.TryGetValue(q.ParentSelect, out parent))
							{
								doConvert = true;
								parent    = q.ParentSelect;
							}
						}

						if (!doConvert)
						{
							doConvert = null != Find(q, e =>
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

						var nq = new SelectQuery { EQueryType = q.EQueryType };

						_visitedElements.Add(q,     nq);
						_visitedElements.Add(q.All, nq.All);

						var fc = (IFromClause)   ConvertInternal(q.From,    action) ?? q.From;
						var sc = (ISelectClause) ConvertInternal(q.Select,  action) ?? q.Select;
						var ic = q.IsInsert ? ((IInsertClause)ConvertInternal(q.Insert, action) ?? q.Insert) : null;
						var uc = q.IsUpdate ? ((IUpdateClause)ConvertInternal(q.Update, action) ?? q.Update) : null;
						var dc = q.IsDelete ? ((DeleteClause)ConvertInternal(q.Delete, action) ?? q.Delete) : null;
						var wc = (IWhereClause)  ConvertInternal(q.Where,   action) ?? q.Where;
						var gc = (GroupByClause)ConvertInternal(q.GroupBy, action) ?? q.GroupBy;
						var hc = (IWhereClause)  ConvertInternal(q.Having,  action) ?? q.Having;
						var oc = (IOrderByClause)ConvertInternal(q.OrderBy, action) ?? q.OrderBy;
						var us = q.HasUnion ? Convert(q.Unions, action) : q.Unions;

						var ps = new List<SqlParameter>(q.Parameters.Count);

						foreach (var p in q.Parameters)
						{
							IQueryElement e;

							if (_visitedElements.TryGetValue(p, out e))
							{
								if (e == null)
									ps.Add(p);
								else if (e is SqlParameter)
									ps.Add((SqlParameter)e);
							}
						}

						nq.Init(ic, uc, dc, sc, fc, wc, gc, hc, oc, us,
							(ISelectQuery)parent,
							q.CreateTable,
							q.IsParameterDependent,
							ps);

						_visitedElements[q] = action(nq) ?? nq;

						return nq;
					}
			}

			newElement = newElement == null ? action(element) : (action(newElement) ?? newElement);

			_visitedElements.Add(element, newElement);

			return newElement;
		}

		static TE[] ToArray<TK,TE>(IDictionary<TK,TE> dic)
		{
			var es = new TE[dic.Count];
			var i  = 0;

			foreach (var e in dic.Values)
				es[i++] = e;

			return es;
		}

		delegate T Clone<T>(T obj);

		T[] Convert<T>(T[] arr, Func<IQueryElement, IQueryElement> action)
			where T : class, IQueryElement
		{
			return Convert(arr, action, null);
		}

		T[] Convert<T>(T[] arr1, Func<IQueryElement, IQueryElement> action, Clone<T> clone)
			where T : class, IQueryElement
		{
			T[] arr2 = null;

			for (var i = 0; i < arr1.Length; i++)
			{
				var elem1 = arr1[i];
				var elem2 = (T)ConvertInternal(elem1, action);

				if (elem2 != null && !ReferenceEquals(elem1, elem2))
				{
					if (arr2 == null)
					{
						arr2 = new T[arr1.Length];

						for (var j = 0; j < i; j++)
							arr2[j] = clone == null ? arr1[j] : clone(arr1[j]);
					}

					arr2[i] = elem2;
				}
				else if (arr2 != null)
					arr2[i] = clone == null ? elem1 : clone(elem1);
			}

			return arr2;
		}

		List<T> Convert<T>(List<T> list, Func<IQueryElement, IQueryElement> action)
			where T : class, IQueryElement
		{
			return Convert(list, action, null);
		}

		List<T> Convert<T>(List<T> list1, Func<IQueryElement, IQueryElement> action, Clone<T> clone)
			where T : class, IQueryElement
		{
			List<T> list2 = null;

			for (var i = 0; i < list1.Count; i++)
			{
				var elem1 = list1[i];
				var elem2 = (T)ConvertInternal(elem1, action);

				if (elem2 != null && !ReferenceEquals(elem1, elem2))
				{
					if (list2 == null)
					{
						list2 = new List<T>(list1.Count);

						for (var j = 0; j < i; j++)
							list2.Add(clone == null ? list1[j] : clone(list1[j]));
					}

					list2.Add(elem2);
				}
				else if (list2 != null)
					list2.Add(clone == null ? elem1 : clone(elem1));
			}

			return list2;
		}

		#endregion
	}
}
