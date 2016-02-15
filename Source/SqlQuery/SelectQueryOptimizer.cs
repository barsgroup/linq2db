﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace LinqToDB.SqlQuery
{
    using LinqToDB.SqlQuery.QueryElements;
    using LinqToDB.SqlQuery.QueryElements.Clauses;
    using LinqToDB.SqlQuery.QueryElements.Conditions;
    using LinqToDB.SqlQuery.QueryElements.Conditions.Interfaces;
    using LinqToDB.SqlQuery.QueryElements.Enums;
    using LinqToDB.SqlQuery.QueryElements.Interfaces;
    using LinqToDB.SqlQuery.QueryElements.Predicates;
    using LinqToDB.SqlQuery.QueryElements.Predicates.Interfaces;
    using LinqToDB.SqlQuery.QueryElements.SqlElements;
    using LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces;

    using SqlProvider;

	class SelectQueryOptimizer
	{
		public SelectQueryOptimizer(SqlProviderFlags flags, ISelectQuery selectQuery)
		{
			_flags       = flags;
			_selectQuery = selectQuery;
		}

		readonly SqlProviderFlags _flags;
		readonly ISelectQuery _selectQuery;

		public void FinalizeAndValidate(bool isApplySupported, bool optimizeColumns)
		{
//#if DEBUG
//			if (_selectQuery.IsUpdate)
//			{
//			}

//			var sqlText = _selectQuery.SqlText;

//			var dic = new Dictionary<SelectQuery,SelectQuery>();

//		    foreach (var element in QueryVisitor.FindAll<SelectQuery>(_selectQuery))
//		    {
//                if (dic.ContainsKey(element))
//                    throw new InvalidOperationException("SqlQuery circle reference detected.");

//                dic.Add(element, element);
//            }
//#endif

			OptimizeUnions();
			FinalizeAndValidateInternal(isApplySupported, optimizeColumns, new List<ISqlTableSource>());
			ResolveFields();
			_selectQuery.SetAliases();

//#if DEBUG
//			sqlText = _selectQuery.SqlText;
//#endif
		}

		class QueryData
		{
			public ISelectQuery Query;
			public List<IQueryExpression> Fields  = new List<IQueryExpression>();
			public List<QueryData>      Queries = new List<QueryData>();
		}

		void ResolveFields()
		{
			var root = GetQueryData(_selectQuery);

			ResolveFields(root);
		}

		static QueryData GetQueryData(ISelectQuery selectQuery)
		{
			var data = new QueryData { Query = selectQuery };

			new QueryVisitor().FindParentFirst(selectQuery, e =>
			{
				switch (e.ElementType)
				{
					case EQueryElementType.SqlField :
						{
							var field = (ISqlField)e;

							if (field.Name.Length != 1 || field.Name[0] != '*')
								data.Fields.Add(field);

							break;
						}

					case EQueryElementType.SqlQuery :
						{
							if (e != selectQuery)
							{
								data.Queries.Add(GetQueryData((ISelectQuery)e));
								return false;
							}

							break;
						}

					case EQueryElementType.Column :
						return ((IColumn)e).Parent == selectQuery;

					case EQueryElementType.SqlTable :
						return false;
				}

				return true;
			});

			return data;
		}

		static ITableSource FindField(ISqlField field, ITableSource table)
		{
			if (field.Table == table.Source)
				return table;

			foreach (var @join in table.Joins)
			{
				var t = FindField(field, @join.Table);

				if (t != null)
					return @join.Table;
			}

			return null;
		}

		static IQueryExpression GetColumn(QueryData data, ISqlField field)
		{
			foreach (var query in data.Queries)
			{
				var q = query.Query;

				foreach (var table in q.From.Tables)
				{
					var t = FindField(field, table);

					if (t != null)
					{
						var n   = q.Select.Columns.Count;
						var idx = q.Select.Add(field);

						if (n != q.Select.Columns.Count)
							if (!q.GroupBy.IsEmpty || q.Select.Columns.Any(c => IsAggregationFunction(c.Expression)))
								q.GroupBy.Items.Add(field);

						return q.Select.Columns[idx];
					}
				}
			}

			return null;
		}

		static void ResolveFields(QueryData data)
		{
			if (data.Queries.Count == 0)
				return;

			var dic = new Dictionary<IQueryExpression,IQueryExpression>();

			foreach (ISqlField field in data.Fields)
			{
				if (dic.ContainsKey(field))
					continue;

				var found = false;

				foreach (var table in data.Query.From.Tables)
				{
					found = FindField(field, table) != null;

					if (found)
						break;
				}

				if (!found)
				{
					var expr = GetColumn(data, field);

					if (expr != null)
						dic.Add(field, expr);
				}
			}

			if (dic.Count > 0)
				new QueryVisitor().FindParentFirst(data.Query, e =>
				{
					IQueryExpression ex;

					switch (e.ElementType)
					{
						case EQueryElementType.SqlQuery :
							return e == data.Query;

						case EQueryElementType.SqlFunction :
							{
								var parms = ((ISqlFunction)e).Parameters;

								for (var i = 0; i < parms.Length; i++)
									if (dic.TryGetValue(parms[i], out ex))
										parms[i] = ex;

								break;
							}

						case EQueryElementType.SqlExpression :
							{
								var parms = ((ISqlExpression)e).Parameters;

								for (var i = 0; i < parms.Length; i++)
									if (dic.TryGetValue(parms[i], out ex))
										parms[i] = ex;

								break;
							}

						case EQueryElementType.SqlBinaryExpression :
							{
								var expr = (ISqlBinaryExpression)e;
								if (dic.TryGetValue(expr.Expr1, out ex)) expr.Expr1 = ex;
								if (dic.TryGetValue(expr.Expr2, out ex)) expr.Expr2 = ex;
								break;
							}

						case EQueryElementType.ExprPredicate       :
						case EQueryElementType.NotExprPredicate    :
						case EQueryElementType.IsNullPredicate     :
						case EQueryElementType.InSubQueryPredicate :
							{
								var expr = (IExpr)e;
								if (dic.TryGetValue(expr.Expr1, out ex)) expr.Expr1 = ex;
								break;
							}

						case EQueryElementType.ExprExprPredicate :
							{
								var expr = (IExprExpr)e;
								if (dic.TryGetValue(expr.Expr1, out ex)) expr.Expr1 = ex;
								if (dic.TryGetValue(expr.Expr2, out ex)) expr.Expr2 = ex;
								break;
							}

						case EQueryElementType.LikePredicate :
							{
								var expr = (ILike)e;
								if (dic.TryGetValue(expr.Expr1,  out ex)) expr.Expr1  = ex;
								if (dic.TryGetValue(expr.Expr2,  out ex)) expr.Expr2  = ex;
								if (dic.TryGetValue(expr.Escape, out ex)) expr.Escape = ex;
								break;
							}

						case EQueryElementType.BetweenPredicate :
							{
								var expr = (Between)e;
								if (dic.TryGetValue(expr.Expr1, out ex)) expr.Expr1 = ex;
								if (dic.TryGetValue(expr.Expr2, out ex)) expr.Expr2 = ex;
								if (dic.TryGetValue(expr.Expr3, out ex)) expr.Expr3 = ex;
								break;
							}

						case EQueryElementType.InListPredicate :
							{
								var expr = (IInList)e;

								if (dic.TryGetValue(expr.Expr1, out ex)) expr.Expr1 = ex;

								for (var i = 0; i < expr.Values.Count; i++)
									if (dic.TryGetValue(expr.Values[i], out ex))
										expr.Values[i] = ex;

								break;
							}

						case EQueryElementType.Column :
							{
								var expr = (IColumn)e;

								if (expr.Parent != data.Query)
									return false;

								if (dic.TryGetValue(expr.Expression, out ex)) expr.Expression = ex;

								break;
							}

						case EQueryElementType.SetExpression :
							{
								var expr = (ISetExpression)e;
								if (dic.TryGetValue(expr.Expression, out ex)) expr.Expression = ex;
								break;
							}

						case EQueryElementType.GroupByClause :
							{
								var expr = (GroupByClause)e;

								for (var i = 0; i < expr.Items.Count; i++)
									if (dic.TryGetValue(expr.Items[i], out ex))
										expr.Items[i] = ex;

								break;
							}

						case EQueryElementType.OrderByItem :
							{
								var expr = (IOrderByItem)e;
								if (dic.TryGetValue(expr.Expression, out ex)) expr.Expression = ex;
								break;
							}
					}

					return true;
				});

			foreach (var query in data.Queries)
				if (query.Queries.Count > 0)
					ResolveFields(query);
		}

	    void OptimizeUnions()
	    {
	        var exprs = new Dictionary<IQueryExpression, IQueryExpression>();

	        foreach (var element in QueryVisitor.FindOnce<ISelectQuery>(_selectQuery))
	        {
	            if (element.From.Tables.Count != 1 || !element.IsSimple || element.IsInsert || element.IsUpdate || element.IsDelete)
	                return;

	            var table = element.From.Tables[0];

	            var selectQuery = table.Source as ISelectQuery;
	            if (table.Joins.Count != 0 || selectQuery == null)
	                return;

	            if (!selectQuery.HasUnion)
	                return;

	            for (var i = 0; i < element.Select.Columns.Count; i++)
	            {
	                var scol = element.Select.Columns[i];
	                var ucol = selectQuery.Select.Columns[i];

	                if (scol.Expression != ucol)
	                    return;
	            }

	            exprs.Add(selectQuery, element);

	            for (var i = 0; i < element.Select.Columns.Count; i++)
	            {
	                var scol = element.Select.Columns[i];
	                var ucol = selectQuery.Select.Columns[i];

	                scol.Expression = ucol.Expression;
	                scol.Alias = ucol.Alias;

	                exprs.Add(ucol, scol);
	            }

	            for (var i = element.Select.Columns.Count; i < selectQuery.Select.Columns.Count; i++)
	                element.Select.Expr(selectQuery.Select.Columns[i].Expression);

	            element.From.Tables.Clear();
	            element.From.Tables.AddRange(selectQuery.From.Tables);

	            element.Where.SearchCondition.Conditions.AddRange(selectQuery.Where.SearchCondition.Conditions);
	            element.Having.SearchCondition.Conditions.AddRange(selectQuery.Having.SearchCondition.Conditions);
	            element.GroupBy.Items.AddRange(selectQuery.GroupBy.Items);
	            element.OrderBy.Items.AddRange(selectQuery.OrderBy.Items);
	            element.Unions.InsertRange(0, selectQuery.Unions);
	        }

	        _selectQuery.Walk(
	            false,
	            expr =>
	            {
	                IQueryExpression e;

	                if (exprs.TryGetValue(expr, out e))
	                    return e;

	                return expr;
	            });
	    }

	    void FinalizeAndValidateInternal(bool isApplySupported, bool optimizeColumns, List<ISqlTableSource> tables)
	    {
	        OptimizeSearchCondition(_selectQuery.Where.SearchCondition);
	        OptimizeSearchCondition(_selectQuery.Having.SearchCondition);

	        foreach (var joinTable in QueryVisitor.FindOnce<IJoinedTable>(_selectQuery))
	        {
	            OptimizeSearchCondition(joinTable.Condition);
	        }

	        foreach (var query in QueryVisitor.FindDownTo<ISelectQuery>(_selectQuery).Where(item => item != _selectQuery))
	        {
                query.ParentSelect = _selectQuery;

                new SelectQueryOptimizer(_flags, query).FinalizeAndValidateInternal(isApplySupported, optimizeColumns, tables);

                if (query.IsParameterDependent)
                    _selectQuery.IsParameterDependent = true;
            }

	        ResolveWeakJoins(tables);
	        OptimizeColumns();
	        OptimizeApplies(isApplySupported, optimizeColumns);
	        OptimizeSubQueries(isApplySupported, optimizeColumns);
	        OptimizeApplies(isApplySupported, optimizeColumns);

	        foreach (var item in QueryVisitor.FindAll<ISelectQuery>(_selectQuery).Where(item => item != _selectQuery))
	        {
	            RemoveOrderBy(item);
	        }

	    }

	    internal static void OptimizeSearchCondition(ISearchCondition searchCondition)
		{
			// This 'if' could be replaced by one simple match:
			//
			// match (searchCondition.Conditions)
			// {
			// | [SearchCondition(true, _) sc] =>
			//     searchCondition.Conditions = sc.Conditions;
			//     OptimizeSearchCondition(searchCodition)
			//
			// | [SearchCondition(false, [SearchCondition(true, [ExprExpr]) sc])] => ...
			//
			// | [Expr(true,  SqlValue(true))]
			// | [Expr(false, SqlValue(false))]
			//     searchCondition.Conditions = []
			// }
			//
			// One day I am going to rewrite all this crap in Nemerle.
			//
			if (searchCondition.Conditions.Count == 1)
			{
				var cond = searchCondition.Conditions[0];

			    var condition = cond.Predicate as ISearchCondition;
			    if (condition != null)
				{
					if (!cond.IsNot)
					{
						searchCondition.Conditions.Clear();
						searchCondition.Conditions.AddRange(condition.Conditions);

						OptimizeSearchCondition(searchCondition);
						return;
					}

					if (condition.Conditions.Count == 1)
					{
						var c1 = condition.Conditions[0];

					    var predicate = c1.Predicate as IExprExpr;
					    if (!c1.IsNot && predicate != null)
						{
						    EOperator op;

							switch (predicate.EOperator)
							{
								case EOperator.Equal          : op = EOperator.NotEqual;       break;
								case EOperator.NotEqual       : op = EOperator.Equal;          break;
								case EOperator.Greater        : op = EOperator.LessOrEqual;    break;
								case EOperator.NotLess        :
								case EOperator.GreaterOrEqual : op = EOperator.Less;           break;
								case EOperator.Less           : op = EOperator.GreaterOrEqual; break;
								case EOperator.NotGreater     :
								case EOperator.LessOrEqual    : op = EOperator.Greater;        break;
								default: throw new InvalidOperationException();
							}

							c1.Predicate = new ExprExpr(predicate.Expr1, op, predicate.Expr2);

							searchCondition.Conditions.Clear();
							searchCondition.Conditions.AddRange(condition.Conditions);

							OptimizeSearchCondition(searchCondition);
							return;
						}
					}
				}

				if (cond.Predicate.ElementType == EQueryElementType.ExprPredicate)
				{
					var expr = (IExpr)cond.Predicate;

				    var sqlValue = expr.Expr1 as ISqlValue;

				    if (sqlValue?.Value is bool)
				        if (cond.IsNot ? !(bool)sqlValue.Value : (bool)sqlValue.Value)
				            searchCondition.Conditions.Clear();
				}
			}

			for (var i = 0; i < searchCondition.Conditions.Count; i++)
			{
				var cond = searchCondition.Conditions[i];

				if (cond.Predicate.ElementType == EQueryElementType.ExprPredicate)
				{
					var expr = (IExpr)cond.Predicate;

				    var sqlValue = expr.Expr1 as ISqlValue;

				    if (sqlValue?.Value is bool)
				    {
				        if (cond.IsNot ? !(bool)sqlValue.Value : (bool)sqlValue.Value)
				        {
				            if (i > 0 && searchCondition.Conditions[i - 1].IsOr)
				            {
				                searchCondition.Conditions.RemoveRange(0, i);
				                OptimizeSearchCondition(searchCondition);

				                break;
				            }
				        }
				    }
				}
				else
				{
				    var condition = cond.Predicate as ISearchCondition;
				    if (condition != null)
				    {
				        OptimizeSearchCondition(condition);
				    }
				}
			}
		}

		static void RemoveOrderBy(ISelectQuery selectQuery)
		{
			if (selectQuery.OrderBy.Items.Count > 0 && selectQuery.Select.SkipValue == null && selectQuery.Select.TakeValue == null)
				selectQuery.OrderBy.Items.Clear();
		}

		internal void ResolveWeakJoins(List<ISqlTableSource> tables)
		{
			Func<ITableSource, bool> findTable = null; findTable = table =>
			{
				if (tables.Contains(table.Source))
					return true;

				foreach (var join in table.Joins)
				{
					if (findTable(join.Table))
					{
						join.IsWeak = false;
						return true;
					}
				}

			    var selectQuery = table.Source as ISelectQuery;
			    return selectQuery != null && selectQuery.From.Tables.Any(t => findTable(t));
			};

			var areTablesCollected = false;

			_selectQuery.ForEachTable(table =>
			{
				for (var i = 0; i < table.Joins.Count; i++)
				{
					var join = table.Joins[i];

					if (join.IsWeak)
					{
						if (!areTablesCollected)
						{
							areTablesCollected = true;

						    var items = new IQueryElement[]
						                {
						                    _selectQuery.Select,
						                    _selectQuery.Where,
						                    _selectQuery.GroupBy,
						                    _selectQuery.Having,
						                    _selectQuery.OrderBy,
                                            _selectQuery.IsInsert ? _selectQuery.Insert : null,
                                            _selectQuery.IsUpdate ? _selectQuery.Update : null,
                                            _selectQuery.IsDelete ? _selectQuery.Delete : null,
                                        };

						    var tableArguments = QueryVisitor.FindAll<ISqlTable>(_selectQuery.From).Where(t => t.TableArguments != null).SelectMany(t => t.TableArguments);

                            var newFileds = QueryVisitor.FindAll<ISqlField>(items.Union(tableArguments).ToArray())
                            .Where(field => !tables.Contains(field.Table));

                            tables.AddRange(newFileds.Select(f => f.Table));

						}

						if (findTable(join.Table))
						{
							join.IsWeak = false;
						}
						else
						{
							table.Joins.RemoveAt(i);
							i--;
						}
					}
				}
			}, new HashSet<ISelectQuery>());
		}

        ITableSource OptimizeSubQuery(
            ITableSource source,
			bool optimizeWhere,
			bool allColumns,
			bool isApplySupported,
			bool optimizeValues,
			bool optimizeColumns)
		{
			foreach (var jt in source.Joins)
			{
				var table = OptimizeSubQuery(
					jt.Table,
					jt.JoinType == EJoinType.Inner || jt.JoinType == EJoinType.CrossApply,
					false,
					isApplySupported,
					jt.JoinType == EJoinType.Inner || jt.JoinType == EJoinType.CrossApply,
					optimizeColumns);

				if (table != jt.Table)
				{
					var sql = jt.Table.Source as ISelectQuery;

					if (sql != null && sql.OrderBy.Items.Count > 0)
						foreach (var item in sql.OrderBy.Items)
							_selectQuery.OrderBy.Expr(item.Expression, item.IsDescending);

					jt.Table = table;
				}
			}

			return source.Source is ISelectQuery ?
				RemoveSubQuery(source, optimizeWhere, allColumns && !isApplySupported, optimizeValues, optimizeColumns) :
				source;
		}

	    static bool CheckColumn(IColumn column, IQueryExpression expr, ISelectQuery query, bool optimizeValues, bool optimizeColumns)
	    {
	        if (expr is ISqlField || expr is IColumn)
	            return false;

	        var sqlValue = expr as ISqlValue;
	        if (sqlValue != null)
	            return !optimizeValues && 1.Equals(sqlValue.Value);

	        var sqlBinaryExpression = expr as ISqlBinaryExpression;
	        if (sqlBinaryExpression != null)
	        {
	            var e = sqlBinaryExpression;

	            var expr1 = e.Expr1 as ISqlValue;
	            if (e.Operation == "*" && expr1 != null)
	            {
	                if (expr1.Value is int && (int)expr1.Value == -1)
	                    return CheckColumn(column, e.Expr2, query, optimizeValues, optimizeColumns);
	            }
	        }

	        var visitor = new QueryVisitor();

	        if (optimizeColumns && visitor.Find(expr, e => e is ISelectQuery || IsAggregationFunction(e)) == null)
	        {
	            var q = query.ParentSelect ?? query;
	            var count = QueryVisitor.FindAll<IColumn>(q).Count(e => e == column);

	            return count > 2;
	        }

	        return true;
	    }

        ITableSource RemoveSubQuery(
            ITableSource childSource,
			bool concatWhere,
			bool allColumns,
			bool optimizeValues,
			bool optimizeColumns)
		{
			var query = (ISelectQuery)childSource. Source;

			var isQueryOK = query.From.Tables.Count == 1;

			isQueryOK = isQueryOK && (concatWhere || query.Where.IsEmpty && query.Having.IsEmpty);
			isQueryOK = isQueryOK && !query.HasUnion && query.GroupBy.IsEmpty && !query.Select.HasModifier;
			//isQueryOK = isQueryOK && (_flags.IsDistinctOrderBySupported || query.Select.IsDistinct );

			if (!isQueryOK)
				return childSource;

			var isColumnsOK =
				(allColumns && !query.Select.Columns.Any(c => IsAggregationFunction(c.Expression))) ||
				!query.Select.Columns.Any(c => CheckColumn(c, c.Expression, query, optimizeValues, optimizeColumns));

			if (!isColumnsOK)
				return childSource;

			var map = new Dictionary<IQueryExpression,IQueryExpression>(query.Select.Columns.Count);

			foreach (var c in query.Select.Columns)
				map.Add(c, c.Expression);

			var top = _selectQuery;

			while (top.ParentSelect != null)
				top = top.ParentSelect;

			top.Walk(false, expr =>
			{
				IQueryExpression fld;
				return map.TryGetValue(expr, out fld) ? fld : expr;
			});

	        foreach (var expr in QueryVisitor.FindOnce<IInList>(top).Where(p => p.Expr1 == query))
	        {
                expr.Expr1 = query.From.Tables[0];
            }

			query.From.Tables[0].Joins.AddRange(childSource.Joins);

			if (query.From.Tables[0].Alias == null)
				query.From.Tables[0].Alias = childSource.Alias;

			if (!query.Where. IsEmpty) ConcatSearchCondition(_selectQuery.Where,  query.Where);
			if (!query.Having.IsEmpty) ConcatSearchCondition(_selectQuery.Having, query.Having);

            foreach (var item in QueryVisitor.FindOnce<ISelectQuery>(top).Where(q => q.ParentSelect == query))
            {
                item.ParentSelect = query.ParentSelect ?? _selectQuery;
            }

			return query.From.Tables[0];
		}

		static bool IsAggregationFunction(IQueryElement expr)
		{
		    var sqlFunction = expr as ISqlFunction;
		    if (sqlFunction != null)
		        switch (sqlFunction.Name)
		        {
		            case "Count":
		            case "Average":
		            case "Min":
		            case "Max":
		            case "Sum":
		                return true;
		        }

		    return false;
		}

		void OptimizeApply(ITableSource tableSource, IJoinedTable joinTable, bool isApplySupported, bool optimizeColumns)
		{
			var joinSource = joinTable.Table;

			foreach (var join in joinSource.Joins)
				if (join.JoinType == EJoinType.CrossApply || join.JoinType == EJoinType.OuterApply)
					OptimizeApply(joinSource, join, isApplySupported, optimizeColumns);

			if (isApplySupported && !joinTable.CanConvertApply)
				return;

			if (joinSource.Source.ElementType == EQueryElementType.SqlQuery)
			{
				var sql   = (ISelectQuery)joinSource.Source;
				var isAgg = sql.Select.Columns.Any(c => IsAggregationFunction(c.Expression));

				if (isApplySupported && (isAgg || sql.Select.TakeValue != null || sql.Select.SkipValue != null))
					return;

				var searchCondition = new List<ICondition>(sql.Where.SearchCondition.Conditions);

				sql.Where.SearchCondition.Conditions.Clear();

				if (!ContainsTable(tableSource.Source, sql))
				{
					joinTable.JoinType = joinTable.JoinType == EJoinType.CrossApply ? EJoinType.Inner : EJoinType.Left;
					joinTable.Condition.Conditions.AddRange(searchCondition);
				}
				else
				{
					sql.Where.SearchCondition.Conditions.AddRange(searchCondition);

					var table = OptimizeSubQuery(
						joinTable.Table,
						joinTable.JoinType == EJoinType.Inner || joinTable.JoinType == EJoinType.CrossApply,
						joinTable.JoinType == EJoinType.CrossApply,
						isApplySupported,
						joinTable.JoinType == EJoinType.Inner || joinTable.JoinType == EJoinType.CrossApply,
						optimizeColumns);

					if (table != joinTable.Table)
					{
						var q = joinTable.Table.Source as ISelectQuery;

						if (q != null && q.OrderBy.Items.Count > 0)
							foreach (var item in q.OrderBy.Items)
								_selectQuery.OrderBy.Expr(item.Expression, item.IsDescending);

						joinTable.Table = table;

						OptimizeApply(tableSource, joinTable, isApplySupported, optimizeColumns);
					}
				}
			}
			else
			{
				if (!ContainsTable(tableSource.Source, joinSource.Source))
					joinTable.JoinType = joinTable.JoinType == EJoinType.CrossApply ? EJoinType.Inner : EJoinType.Left;
			}
		}

		static bool ContainsTable(ISqlTableSource table, IQueryElement sql)
		{
			return null != new QueryVisitor().Find(sql, e =>
				e == table ||
				e.ElementType == EQueryElementType.SqlField && table == ((ISqlField)e).Table ||
				e.ElementType == EQueryElementType.Column   && table == ((IColumn)  e).Parent);
		}

		static void ConcatSearchCondition(IWhereClause where1, IWhereClause where2)
		{
			if (where1.IsEmpty)
			{
				where1.SearchCondition.Conditions.AddRange(where2.SearchCondition.Conditions);
			}
			else
			{
				if (where1.SearchCondition.Precedence < Precedence.LogicalConjunction)
				{
					var sc1 = new SearchCondition();

					sc1.Conditions.AddRange(where1.SearchCondition.Conditions);

					where1.SearchCondition.Conditions.Clear();
					where1.SearchCondition.Conditions.Add(new Condition(false, sc1));
				}

				if (where2.SearchCondition.Precedence < Precedence.LogicalConjunction)
				{
					var sc2 = new SearchCondition();

					sc2.Conditions.AddRange(where2.SearchCondition.Conditions);

					where1.SearchCondition.Conditions.Add(new Condition(false, sc2));
				}
				else
					where1.SearchCondition.Conditions.AddRange(where2.SearchCondition.Conditions);
			}
		}

		void OptimizeSubQueries(bool isApplySupported, bool optimizeColumns)
		{
			for (var i = 0; i < _selectQuery.From.Tables.Count; i++)
			{
				var table = OptimizeSubQuery(_selectQuery.From.Tables[i], true, false, isApplySupported, true, optimizeColumns);

				if (table != _selectQuery.From.Tables[i])
				{
					var sql = _selectQuery.From.Tables[i].Source as ISelectQuery;

					if (!_selectQuery.Select.Columns.All(c => IsAggregationFunction(c.Expression)))
						if (sql != null && sql.OrderBy.Items.Count > 0)
							foreach (var item in sql.OrderBy.Items)
								_selectQuery.OrderBy.Expr(item.Expression, item.IsDescending);

					_selectQuery.From.Tables[i] = table;
				}
			}
		}

		void OptimizeApplies(bool isApplySupported, bool optimizeColumns)
		{
			foreach (var table in _selectQuery.From.Tables)
				foreach (var join in table.Joins)
					if (join.JoinType == EJoinType.CrossApply || join.JoinType == EJoinType.OuterApply)
						OptimizeApply(table, join, isApplySupported, optimizeColumns);
		}

		void OptimizeColumns()
		{
			_selectQuery.Select.Walk(false, expr =>
			{
				var query = expr as ISelectQuery;
					
				if (query != null && query.From.Tables.Count == 0 && query.Select.Columns.Count == 1)
				{

				    foreach (var q in QueryVisitor.FindAll<ISelectQuery>(query.Select.Columns[0].Expression).Where(q => q.ParentSelect == query))
				    {
                        q.ParentSelect = query.ParentSelect;
                    }

					return query.Select.Columns[0].Expression;
				}

				return expr;
			});
		}
	}
}
