using System;

namespace LinqToDB.SqlProvider
{
	using System.Collections.Generic;
	using System.Linq;

	using Extensions;

	using LinqToDB.SqlQuery.QueryElements;
	using LinqToDB.SqlQuery.QueryElements.Conditions;
	using LinqToDB.SqlQuery.QueryElements.Enums;
	using LinqToDB.SqlQuery.QueryElements.Interfaces;
	using LinqToDB.SqlQuery.QueryElements.Predicates;
	using LinqToDB.SqlQuery.QueryElements.Predicates.Interfaces;
	using LinqToDB.SqlQuery.QueryElements.SqlElements;
	using LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces;

	using SqlQuery;

	public class BasicSqlOptimizer : ISqlOptimizer
	{
		#region Init

		protected BasicSqlOptimizer(SqlProviderFlags sqlProviderFlags)
		{
			SqlProviderFlags = sqlProviderFlags;
		}

		public SqlProviderFlags SqlProviderFlags { get; private set; }

		#endregion

		#region ISqlOptimizer Members

		public virtual ISelectQuery Finalize(ISelectQuery selectQuery)
		{
			new SelectQueryOptimizer(SqlProviderFlags, selectQuery).FinalizeAndValidate(
				SqlProviderFlags.IsApplyJoinSupported,
				SqlProviderFlags.IsGroupByExpressionSupported);

			if (!SqlProviderFlags.IsCountSubQuerySupported)  selectQuery = MoveCountSubQuery (selectQuery);
			if (!SqlProviderFlags.IsSubQueryColumnSupported) selectQuery = MoveSubQueryColumn(selectQuery);

			if (!SqlProviderFlags.IsCountSubQuerySupported || !SqlProviderFlags.IsSubQueryColumnSupported)
				new SelectQueryOptimizer(SqlProviderFlags, selectQuery).FinalizeAndValidate(
					SqlProviderFlags.IsApplyJoinSupported,
					SqlProviderFlags.IsGroupByExpressionSupported);

			return selectQuery;
		}

        ISelectQuery MoveCountSubQuery(ISelectQuery selectQuery)
		{
		    foreach (var query in QueryVisitor.FindOnce<ISelectQuery>(selectQuery))
		    {
                MoveCountSubSubQuery(query);
		    }

			return selectQuery;
		}

		void MoveCountSubSubQuery(ISelectQuery query)
		{
			for (var i = 0; i < query.Select.Columns.Count; i++)
			{
				var col = query.Select.Columns[i];

				// The column is a subquery.
				//
				if (col.Expression.ElementType == EQueryElementType.SqlQuery)
				{
					var subQuery = (ISelectQuery)col.Expression;
					var isCount  = false;

					// Check if subquery is Count subquery.
					//
					if (subQuery.Select.Columns.Count == 1)
					{
						var subCol = subQuery.Select.Columns[0];

						if (subCol.Expression.ElementType == EQueryElementType.SqlFunction)
							isCount = ((ISqlFunction)subCol.Expression).Name == "Count";
					}

					if (!isCount)
						continue;

					// Check if subquery where clause does not have ORs.
					//
					SelectQueryOptimizer.OptimizeSearchCondition(subQuery.Where.SearchCondition);

					var allAnd = true;

					for (var j = 0; allAnd && j < subQuery.Where.SearchCondition.Conditions.Count - 1; j++)
					{
						var cond = subQuery.Where.SearchCondition.Conditions[j];

						if (cond.IsOr)
							allAnd = false;
					}

					if (!allAnd || !ConvertCountSubQuery(subQuery))
						continue;

					// Collect tables.
					//
					var allTables   = new HashSet<ISqlTableSource>();
					var levelTables = new HashSet<ISqlTableSource>();

				    foreach (var item in QueryVisitor.FindOnce<ISqlTableSource>(subQuery))
				    {
				        allTables.Add(item);

                        if (subQuery.From.IsChild(item))
                            levelTables.Add(item);
                    }


				    Func<IQueryElement,bool> checkTable = e =>
					{
						switch (e.ElementType)
						{
							case EQueryElementType.SqlField : return !allTables.Contains(((ISqlField)       e).Table);
							case EQueryElementType.Column   : return !allTables.Contains(((IColumn)e).Parent);
						}
						return false;
					};

					var join = SelectQuery.LeftJoin(subQuery);

					query.From.Tables[0].Joins.Add(join.JoinedTable);

					for (var j = 0; j < subQuery.Where.SearchCondition.Conditions.Count; j++)
					{
						var cond = subQuery.Where.SearchCondition.Conditions[j];

						if (new QueryVisitor().Find(cond, checkTable) == null)
							continue;

						var replaced = new Dictionary<IQueryElement,IQueryElement>();

						var nc = new QueryVisitor().Convert(cond, e =>
						{
							var ne = e;

							switch (e.ElementType)
							{
								case EQueryElementType.SqlField :
									if (replaced.TryGetValue(e, out ne))
										return ne;

									if (levelTables.Contains(((ISqlField)e).Table))
									{
										subQuery.GroupBy.Expr((ISqlField)e);
										ne = subQuery.Select.Columns[subQuery.Select.Add((ISqlField)e)];
									}

									break;

								case EQueryElementType.Column   :
									if (replaced.TryGetValue(e, out ne))
										return ne;

									if (levelTables.Contains(((IColumn)e).Parent))
									{
										subQuery.GroupBy.Expr((IColumn)e);
										ne = subQuery.Select.Columns[subQuery.Select.Add((IColumn)e)];
									}

									break;
							}

							if (!ReferenceEquals(e, ne))
								replaced.Add(e, ne);

							return ne;
						});

						if (nc != null && !ReferenceEquals(nc, cond))
						{
							join.JoinedTable.Condition.Conditions.Add(nc);
							subQuery.Where.SearchCondition.Conditions.RemoveAt(j);
							j--;
						}
					}

					if (!query.GroupBy.IsEmpty/* && subQuery.Select.Columns.Count > 1*/)
					{
						var oldFunc = (ISqlFunction)subQuery.Select.Columns[0].Expression;

						subQuery.Select.Columns.RemoveAt(0);

						query.Select.Columns[i].Expression = 
							new SqlFunction(oldFunc.SystemType, oldFunc.Name, subQuery.Select.Columns[0]);
					}
					else
					{
						query.Select.Columns[i].Expression = subQuery.Select.Columns[0];
					}
				}
			}
		}

		public virtual bool ConvertCountSubQuery(ISelectQuery subQuery)
		{
			return true;
		}

        ISelectQuery MoveSubQueryColumn(ISelectQuery selectQuery)
		{
			var dic = new Dictionary<IQueryElement,IQueryElement>();

		    foreach (var query in QueryVisitor.FindOnce<ISelectQuery>(selectQuery))
		    {
		        
				for (var i = 0; i < query.Select.Columns.Count; i++)
				{
					var col = query.Select.Columns[i];

					if (col.Expression.ElementType == EQueryElementType.SqlQuery)
					{
						var subQuery    = (ISelectQuery)col.Expression;
						var allTables   = new HashSet<ISqlTableSource>();
						var levelTables = new HashSet<ISqlTableSource>();

						Func<IQueryElement,bool> checkTable = e =>
						{
							switch (e.ElementType)
							{
								case EQueryElementType.SqlField : return !allTables.Contains(((ISqlField)e).Table);
								case EQueryElementType.Column   : return !allTables.Contains(((Column)e).Parent);
							}
							return false;
						};

                        foreach (var item in QueryVisitor.FindOnce<ISqlTableSource>(subQuery))
					    {
					        allTables.Add(item);
					        if (subQuery.From.IsChild(item))
					        {
					            levelTables.Add(item);
                            }
                        }

					    if (SqlProviderFlags.IsSubQueryColumnSupported && new QueryVisitor().Find(subQuery, checkTable) == null)
							continue;

						var join = SelectQuery.LeftJoin(subQuery);

						query.From.Tables[0].Joins.Add(join.JoinedTable);

						SelectQueryOptimizer.OptimizeSearchCondition(subQuery.Where.SearchCondition);

						var isCount      = false;
						var isAggregated = false;
						
						if (subQuery.Select.Columns.Count == 1)
						{
							var subCol = subQuery.Select.Columns[0];

							if (subCol.Expression.ElementType == EQueryElementType.SqlFunction)
							{
								switch (((ISqlFunction)subCol.Expression).Name)
								{
									case "Min"     :
									case "Max"     :
									case "Sum"     :
									case "Average" : isAggregated = true;                 break;
									case "Count"   : isAggregated = true; isCount = true; break;
								}
							}
						}

						if (SqlProviderFlags.IsSubQueryColumnSupported && !isCount)
							continue;

						var allAnd = true;

						for (var j = 0; allAnd && j < subQuery.Where.SearchCondition.Conditions.Count - 1; j++)
						{
							var cond = subQuery.Where.SearchCondition.Conditions[j];

							if (cond.IsOr)
								allAnd = false;
						}

						if (!allAnd)
							continue;

						var modified = false;

						for (var j = 0; j < subQuery.Where.SearchCondition.Conditions.Count; j++)
						{
							var cond = subQuery.Where.SearchCondition.Conditions[j];

							if (new QueryVisitor().Find(cond, checkTable) == null)
								continue;

							var replaced = new Dictionary<IQueryElement,IQueryElement>();

							var nc = new QueryVisitor().Convert(cond, delegate(IQueryElement e)
							{
								var ne = e;

								switch (e.ElementType)
								{
									case EQueryElementType.SqlField :
										if (replaced.TryGetValue(e, out ne))
											return ne;

										if (levelTables.Contains(((ISqlField)e).Table))
										{
											if (isAggregated)
												subQuery.GroupBy.Expr((ISqlField)e);
											ne = subQuery.Select.Columns[subQuery.Select.Add((ISqlField)e)];
										}

										break;

									case EQueryElementType.Column   :
										if (replaced.TryGetValue(e, out ne))
											return ne;

										if (levelTables.Contains(((IColumn)e).Parent))
										{
											if (isAggregated)
												subQuery.GroupBy.Expr((IColumn)e);
											ne = subQuery.Select.Columns[subQuery.Select.Add((IColumn)e)];
										}

										break;
								}

								if (!ReferenceEquals(e, ne))
									replaced.Add(e, ne);

								return ne;
							});

							if (nc != null && !ReferenceEquals(nc, cond))
							{
								modified = true;

								join.JoinedTable.Condition.Conditions.Add(nc);
								subQuery.Where.SearchCondition.Conditions.RemoveAt(j);
								j--;
							}
						}

						if (modified || isAggregated)
						{
							if (isCount && !query.GroupBy.IsEmpty)
							{
								var oldFunc = (ISqlFunction)subQuery.Select.Columns[0].Expression;

								subQuery.Select.Columns.RemoveAt(0);

								query.Select.Columns[i] = new Column(
									query,
									new SqlFunction(oldFunc.SystemType, oldFunc.Name, subQuery.Select.Columns[0]));
							}
							else if (isAggregated && !query.GroupBy.IsEmpty)
							{
								var oldFunc = (ISqlFunction)subQuery.Select.Columns[0].Expression;

								subQuery.Select.Columns.RemoveAt(0);

								var idx = subQuery.Select.Add(oldFunc.Parameters[0]);

								query.Select.Columns[i] = new Column(
									query,
									new SqlFunction(oldFunc.SystemType, oldFunc.Name, subQuery.Select.Columns[idx]));
							}
							else
							{
								query.Select.Columns[i] = new Column(query, subQuery.Select.Columns[0]);
							}

							dic.Add(col, query.Select.Columns[i]);
						}
					}
				}
			}

			selectQuery = new QueryVisitor().Convert(selectQuery, e =>
			{
				IQueryElement ne;
				return dic.TryGetValue(e, out ne) ? ne : e;
			});

			return selectQuery;
		}

		public virtual IQueryExpression ConvertExpression(IQueryExpression expression)
		{
			switch (expression.ElementType)
			{
				case EQueryElementType.SqlBinaryExpression:

					#region SqlBinaryExpression

					{
						var be = (ISqlBinaryExpression)expression;

						switch (be.Operation)
						{
							case "+":
						        var value2 = be.Expr1 as ISqlValue;
						        if (value2 != null)
								{
								    if (value2.Value is int    && (int)   value2.Value == 0 ||
										value2.Value is string && (string)value2.Value == "") return be.Expr2;
								}

						        var expr4 = be.Expr2 as ISqlValue;
						        if (expr4 != null)
								{
								    if (expr4.Value is int)
									{
										if ((int)expr4.Value == 0) return be.Expr1;

									    var binaryExpression = be.Expr1 as ISqlBinaryExpression;
									    var be1 = binaryExpression;

									    var be1v2 = be1?.Expr2 as ISqlValue;

									    if (be1v2?.Value is int)
									    {
									        switch (be1.Operation)
									        {
									            case "+":
									            {
									                var value = (int)be1v2.Value + (int)expr4.Value;
									                var oper  = be1.Operation;

									                if (value < 0)
									                {
									                    value = - value;
									                    oper  = "-";
									                }

									                return new SqlBinaryExpression(be.SystemType, be1.Expr1, oper, new SqlValue(value), be.Precedence);
									            }

									            case "-":
									            {
									                var value = (int)be1v2.Value - (int)expr4.Value;
									                var oper  = be1.Operation;

									                if (value < 0)
									                {
									                    value = - value;
									                    oper  = "+";
									                }

									                return new SqlBinaryExpression(be.SystemType, be1.Expr1, oper, new SqlValue(value), be.Precedence);
									            }
									        }
									    }
									}
									else if (expr4.Value is string)
									{
										if ((string)expr4.Value == "") return be.Expr1;

									    var be1 = be.Expr1 as ISqlBinaryExpression;
									    var value = (be1?.Expr2 as ISqlValue)?.Value;

									    if (value is string)
									        return new SqlBinaryExpression(
									            be1.SystemType,
									            be1.Expr1,
									            be1.Operation,
									            new SqlValue(string.Concat(value, expr4.Value)));
									}
								}

						        var sqlValue2 = be.Expr1 as ISqlValue;
						        var v3 = be.Expr2 as ISqlValue;
						        if (sqlValue2 != null && v3 != null)
								{
								    if (sqlValue2.Value is int    && v3.Value is int)    return new SqlValue((int)sqlValue2.Value + (int)v3.Value);
									if (sqlValue2.Value is string || v3.Value is string) return new SqlValue(sqlValue2.Value.ToString() + v3.Value);
								}

								if (be.Expr1.SystemType == typeof(string) && be.Expr2.SystemType != typeof(string))
								{
									var len = be.Expr2.SystemType == null ? 100 : SqlDataType.GetMaxDisplaySize(SqlDataType.GetDataType(be.Expr2.SystemType).DataType);

									if (len <= 0)
										len = 100;

									return new SqlBinaryExpression(
										be.SystemType,
										be.Expr1,
										be.Operation,
										ConvertExpression(new SqlFunction(typeof(string), "Convert", new SqlDataType(DataType.VarChar, len), be.Expr2)),
										be.Precedence);
								}

								if (be.Expr1.SystemType != typeof(string) && be.Expr2.SystemType == typeof(string))
								{
									var len = be.Expr1.SystemType == null ? 100 : SqlDataType.GetMaxDisplaySize(SqlDataType.GetDataType(be.Expr1.SystemType).DataType);

									if (len <= 0)
										len = 100;

									return new SqlBinaryExpression(
										be.SystemType,
										ConvertExpression(new SqlFunction(typeof(string), "Convert", new SqlDataType(DataType.VarChar, len), be.Expr1)),
										be.Operation,
										be.Expr2,
										be.Precedence);
								}

								break;

							case "-":
						        var expr3 = be.Expr2 as ISqlValue;
						        if (expr3?.Value is int)
						        {
						            if ((int)expr3.Value == 0) return be.Expr1;

						            var binaryExpression = be.Expr1 as ISqlBinaryExpression;
						            var be1V2 = binaryExpression?.Expr2 as ISqlValue;
						            if (be1V2?.Value is int)
						            {
						                switch (binaryExpression.Operation)
						                {
						                    case "+":
						                    {
						                        var value = (int)be1V2.Value - (int)expr3.Value;
						                        var oper  = binaryExpression.Operation;

						                        if (value < 0)
						                        {
						                            value = -value;
						                            oper  = "-";
						                        }

						                        return new SqlBinaryExpression(be.SystemType, binaryExpression.Expr1, oper, new SqlValue(value), be.Precedence);
						                    }

						                    case "-":
						                    {
						                        var value = (int)be1V2.Value + (int)expr3.Value;
						                        var oper  = binaryExpression.Operation;

						                        if (value < 0)
						                        {
						                            value = -value;
						                            oper  = "+";
						                        }

						                        return new SqlBinaryExpression(be.SystemType, binaryExpression.Expr1, oper, new SqlValue(value), be.Precedence);
						                    }
						                }
						            }
						        }

						        var sqlValue1 = be.Expr1 as ISqlValue;
						        if (sqlValue1 != null && be.Expr2 is ISqlValue)
								{
								    var v2 = (ISqlValue)be.Expr2;
									if (sqlValue1.Value is int && v2.Value is int) return new SqlValue((int)sqlValue1.Value - (int)v2.Value);
								}

								break;

							case "*":
						        var value1 = be.Expr1 as ISqlValue;
						        if (value1?.Value is int)
						        {
						            var v1v = (int)value1.Value;

						            switch (v1v)
						            {
						                case  0 : return new SqlValue(0);
						                case  1 : return be.Expr2;
						                default :
						                {
						                    var be2 = be.Expr2 as ISqlBinaryExpression;

						                    var v1 = be2.Expr1 as ISqlValue;
						                    if (be2 != null && be2.Operation == "*" && v1 != null)
						                    {
						                        if (v1.Value is int)
						                            return ConvertExpression(
						                                new SqlBinaryExpression(be2.SystemType, new SqlValue(v1v * (int)v1.Value), "*", be2.Expr2));
						                    }

						                    break;
						                }

						            }
						        }

						        var expr2 = be.Expr2 as ISqlValue;
						        if (expr2 != null)
								{
								    if (expr2.Value is int && (int)expr2.Value == 1) return be.Expr1;
									if (expr2.Value is int && (int)expr2.Value == 0) return new SqlValue(0);
								}

						        var expr1 = be.Expr1 as ISqlValue;
						        var sqlValue = be.Expr2 as ISqlValue;
						        if (expr1 != null && sqlValue != null)
								{
								    if (expr1.Value is int)
									{
										if (sqlValue.Value is int)    return new SqlValue((int)   expr1.Value * (int)   sqlValue.Value);
										if (sqlValue.Value is double) return new SqlValue((int)   expr1.Value * (double)sqlValue.Value);
									}
									else if (expr1.Value is double)
									{
										if (sqlValue.Value is int)    return new SqlValue((double)expr1.Value * (int)   sqlValue.Value);
										if (sqlValue.Value is double) return new SqlValue((double)expr1.Value * (double)sqlValue.Value);
									}
								}

								break;
						}
					}

					#endregion

					break;

				case EQueryElementType.SqlFunction:

					#region SqlFunction

					{
						var func = (ISqlFunction)expression;

						switch (func.Name)
						{
							case "ConvertToCaseCompareTo":
								return ConvertExpression(new SqlFunction(func.SystemType, "CASE",
									new SearchCondition().Expr(func.Parameters[0]). Greater .Expr(func.Parameters[1]).ToExpr(), new SqlValue(1),
									new SearchCondition().Expr(func.Parameters[0]). Equal   .Expr(func.Parameters[1]).ToExpr(), new SqlValue(0),
									new SqlValue(-1)));

							case "$Convert$": return ConvertConvertion(func);
							case "Average"  : return new SqlFunction(func.SystemType, "Avg", func.Parameters);
							case "Max"      :
							case "Min"      :
								{
									if (func.SystemType == typeof(bool) || func.SystemType == typeof(bool?))
									{
										return new SqlFunction(typeof(int), func.Name,
											new SqlFunction(func.SystemType, "CASE", func.Parameters[0], new SqlValue(1), new SqlValue(0)));
									}

									break;
								}

							case "CASE"     :
								{
									var parms = func.Parameters;
									var len   = parms.Length;

									for (var i = 0; i < parms.Length - 1; i += 2)
									{
										var value = parms[i] as ISqlValue;

										if (value != null)
										{
											if ((bool)value.Value == false)
											{
												var newParms = new IQueryExpression[parms.Length - 2];

												if (i != 0)
													Array.Copy(parms, 0, newParms, 0, i);

												Array.Copy(parms, i + 2, newParms, i, parms.Length - i - 2);

												parms = newParms;
												i -= 2;
											}
											else
											{
												var newParms = new IQueryExpression[i + 1];

												if (i != 0)
													Array.Copy(parms, 0, newParms, 0, i);

												newParms[i] = parms[i + 1];

												parms = newParms;
												break;
											}
										}
									}

									if (parms.Length == 1)
										return parms[0];

									if (parms.Length != len)
										return new SqlFunction(func.SystemType, func.Name, func.Precedence, parms);
								}

								break;

							case "Convert":
								{
									var from  = func.Parameters[1] as ISqlFunction;
									var typef = func.SystemType.ToUnderlying();

									if (from != null && from.Name == "Convert" && from.Parameters[1].SystemType.ToUnderlying() == typef)
										return from.Parameters[1];

									var fe = func.Parameters[1] as ISqlExpression;

									if (fe != null && fe.Expr == "Cast({0} as {1})" && fe.Parameters[0].SystemType.ToUnderlying() == typef)
										return fe.Parameters[0];
								}

								break;
						}
					}

					#endregion

					break;

				case EQueryElementType.SearchCondition :
					SelectQueryOptimizer.OptimizeSearchCondition((ISearchCondition)expression);
					break;

				case EQueryElementType.SqlExpression   :
					{
						var se = (ISqlExpression)expression;

						if (se.Expr == "{0}" && se.Parameters.Length == 1 && se.Parameters[0] != null)
							return se.Parameters[0];
					}

					break;
			}

			return expression;
		}

		public virtual ISqlPredicate ConvertPredicate(ISelectQuery selectQuery, ISqlPredicate predicate)
		{
			switch (predicate.ElementType)
			{
				case EQueryElementType.ExprExprPredicate:
					{
						var expr = (IExprExpr)predicate;

					    var field = expr.Expr1 as ISqlField;
					    var parameter = expr.Expr2 as ISqlParameter;
					    if (field != null && parameter != null)
						{
							if (parameter.DataType == DataType.Undefined)
								parameter.DataType = field.DataType;
						}
						else
					    {
					        var expr2 = expr.Expr2 as ISqlField;
					        var expr1 = expr.Expr1 as ISqlParameter;
					        if (expr2 != null && expr1 != null)
					        {
					            if (expr1.DataType == DataType.Undefined)
					                expr1.DataType = expr2.DataType;
					        }
					    }

					    var sqlValue = expr.Expr1 as ISqlValue;
					    var sqlValue1 = expr.Expr2 as ISqlValue;
					    if (expr.EOperator == EOperator.Equal && sqlValue != null && sqlValue1 != null)
						{
							var value = Equals(sqlValue.Value, sqlValue1.Value);
							return new Expr(new SqlValue(value), Precedence.Comparison);
						}

						switch (expr.EOperator)
						{
							case EOperator.Equal          :
							case EOperator.NotEqual       :
							case EOperator.Greater        :
							case EOperator.GreaterOrEqual :
							case EOperator.Less           :
							case EOperator.LessOrEqual    :
								predicate = OptimizeCase(selectQuery, expr);
								break;
						}

					    var exprExpr = predicate as IExprExpr;
					    if (exprExpr != null)
						{
							expr = exprExpr;

							switch (expr.EOperator)
							{
								case EOperator.Equal      :
								case EOperator.NotEqual   :
									var expr1 = expr.Expr1;
									var expr2 = expr.Expr2;

									if (expr1.CanBeNull() && expr2.CanBeNull())
									{
										if (expr1 is ISqlParameter || expr2 is ISqlParameter)
											selectQuery.IsParameterDependent = true;
										else
											if (expr1 is IColumn || expr1 is ISqlField)
											if (expr2 is IColumn || expr2 is ISqlField)
												predicate = ConvertEqualPredicate(expr);
									}

									break;
							}
						}
					}

					break;

				case EQueryElementType.NotExprPredicate:
					{
						var expr = (INotExpr)predicate;

					    var searchCondition = expr.Expr1 as ISearchCondition;
					    if (expr.IsNot && searchCondition != null)
						{
						    if (searchCondition.Conditions.Count == 1)
							{
								var cond = searchCondition.Conditions[0];

								if (cond.IsNot)
									return cond.Predicate;

							    var exprExpr = cond.Predicate as IExprExpr;
							    if (exprExpr != null)
								{
									if (exprExpr.EOperator == EOperator.Equal)
										return new ExprExpr(exprExpr.Expr1, EOperator.NotEqual, exprExpr.Expr2);

									if (exprExpr.EOperator == EOperator.NotEqual)
										return new ExprExpr(exprExpr.Expr1, EOperator.Equal, exprExpr.Expr2);
								}
							}
						}
					}

					break;
			}

			return predicate;
		}

		protected ISqlPredicate ConvertEqualPredicate(IExprExpr expr)
		{
			var expr1 = expr.Expr1;
			var expr2 = expr.Expr2;
			var cond  = new SearchCondition();

			if (expr.EOperator == EOperator.Equal)
				cond
					.Expr(expr1).IsNull.    And .Expr(expr2).IsNull. Or
					/*.Expr(expr1).IsNotNull. And .Expr(expr2).IsNotNull. And */.Expr(expr1).Equal.Expr(expr2);
			else
				cond
					.Expr(expr1).IsNull.    And .Expr(expr2).IsNotNull. Or
					.Expr(expr1).IsNotNull. And .Expr(expr2).IsNull.    Or
					.Expr(expr1).NotEqual.Expr(expr2);

			return cond;
		}

		static EOperator InvertOperator(EOperator op, bool skipEqual)
		{
			switch (op)
			{
				case EOperator.Equal          : return skipEqual ? op : EOperator.NotEqual;
				case EOperator.NotEqual       : return skipEqual ? op : EOperator.Equal;
				case EOperator.Greater        : return EOperator.LessOrEqual;
				case EOperator.NotLess        :
				case EOperator.GreaterOrEqual : return EOperator.Less;
				case EOperator.Less           : return EOperator.GreaterOrEqual;
				case EOperator.NotGreater     :
				case EOperator.LessOrEqual    : return EOperator.Greater;
				default: throw new InvalidOperationException();
			}
		}

		ISqlPredicate OptimizeCase(ISelectQuery selectQuery, IExprExpr expr)
		{
			var value = expr.Expr1 as ISqlValue;
			var func  = expr.Expr2 as ISqlFunction;
			var valueFirst = false;

			if (value != null && func != null)
			{
				valueFirst = true;
			}
			else
			{
				value = expr.Expr2 as ISqlValue;
				func  = expr.Expr1 as ISqlFunction;
			}

			if (value != null && func != null && func.Name == "CASE")
			{
				if (value.Value is int && func.Parameters.Length == 5)
				{
					var c1 = func.Parameters[0] as ISearchCondition;
					var v1 = func.Parameters[1] as ISqlValue;
					var c2 = func.Parameters[2] as ISearchCondition;
					var v2 = func.Parameters[3] as ISqlValue;
					var v3 = func.Parameters[4] as ISqlValue;

					if (c1 != null && c1.Conditions.Count == 1 && v1?.Value is int && c2 != null && c2.Conditions.Count == 1 && v2?.Value is int && v3?.Value is int)
					{
						var ee1 = c1.Conditions[0].Predicate as IExprExpr;
						var ee2 = c2.Conditions[0].Predicate as IExprExpr;

						if (ee1 != null && ee2 != null && ee1.Expr1.Equals(ee2.Expr1) && ee1.Expr2.Equals(ee2.Expr2))
						{
							int e = 0, g = 0, l = 0;

							if (ee1.EOperator == EOperator.Equal   || ee2.EOperator == EOperator.Equal)   e = 1;
							if (ee1.EOperator == EOperator.Greater || ee2.EOperator == EOperator.Greater) g = 1;
							if (ee1.EOperator == EOperator.Less    || ee2.EOperator == EOperator.Less)    l = 1;

							if (e + g + l == 2)
							{
								var n  = (int)value.Value;
								var i1 = (int)v1.Value;
								var i2 = (int)v2.Value;
								var i3 = (int)v3.Value;

								var n1 = Compare(valueFirst ? n : i1, valueFirst ? i1 : n, expr.EOperator) ? 1 : 0;
								var n2 = Compare(valueFirst ? n : i2, valueFirst ? i2 : n, expr.EOperator) ? 1 : 0;
								var n3 = Compare(valueFirst ? n : i3, valueFirst ? i3 : n, expr.EOperator) ? 1 : 0;

								if (n1 + n2 + n3 == 1)
								{
									if (n1 == 1) return ee1;
									if (n2 == 1) return ee2;

									return ConvertPredicate(
										selectQuery,
										new ExprExpr(
											ee1.Expr1,
											e == 0 ? EOperator.Equal :
											g == 0 ? EOperator.Greater :
													 EOperator.Less,
											ee1.Expr2));
								}

								//	CASE
								//		WHEN [p].[FirstName] > 'John'
								//			THEN 1
								//		WHEN [p].[FirstName] = 'John'
								//			THEN 0
								//		ELSE -1
								//	END <= 0
								if (ee1.EOperator == EOperator.Greater && i1 == 1 &&
									ee2.EOperator == EOperator.Equal   && i2 == 0 &&
									i3 == -1 && n == 0)
								{
									return ConvertPredicate(
										selectQuery,
										new ExprExpr(
											ee1.Expr1,
											valueFirst ? InvertOperator(expr.EOperator, true) : expr.EOperator,
											ee1.Expr2));
								}
							}
						}
					}
				}
				else if (value.Value is bool && func.Parameters.Length == 3)
				{
					var c1 = func.Parameters[0] as ISearchCondition;
					var v1 = func.Parameters[1] as ISqlValue;
					var v2 = func.Parameters[2] as ISqlValue;

					if (c1 != null && c1.Conditions.Count == 1 && v1?.Value is bool && v2?.Value is bool)
					{
						var bv  = (bool)value.Value;
						var bv1 = (bool)v1.Value;
						var bv2 = (bool)v2.Value;

						if (bv == bv1 && expr.EOperator == EOperator.Equal ||
							bv != bv1 && expr.EOperator == EOperator.NotEqual)
						{
							return c1;
						}

						if (bv == bv2 && expr.EOperator == EOperator.NotEqual ||
							bv != bv1 && expr.EOperator == EOperator.Equal)
						{
							var ee = c1.Conditions[0].Predicate as IExprExpr;

							if (ee != null)
							{
								var op = InvertOperator(ee.EOperator, false);
								return new ExprExpr(ee.Expr1, op, ee.Expr2);
							}

							var sc = new SearchCondition();

							sc.Conditions.Add(new Condition(true, c1));

							return sc;
						}
					}
				}
				else if (expr.EOperator == EOperator.Equal && func.Parameters.Length == 3)
				{
					var sc = func.Parameters[0] as ISearchCondition;
					var v1 = func.Parameters[1] as ISqlValue;
					var v2 = func.Parameters[2] as ISqlValue;

					if (sc != null && v1 != null && v2 != null)
					{
						if (Equals(value.Value, v1.Value))
							return sc;

						if (Equals(value.Value, v2.Value) && !sc.CanBeNull())
							return ConvertPredicate(
								selectQuery,
								new NotExpr(sc, true, Precedence.LogicalNegation));
					}
				}
			}

			return expr;
		}

		static bool Compare(int v1, int v2, EOperator op)
		{
			switch (op)
			{
				case EOperator.Equal:           return v1 == v2;
				case EOperator.NotEqual:        return v1 != v2;
				case EOperator.Greater:         return v1 >  v2;
				case EOperator.NotLess:
				case EOperator.GreaterOrEqual:  return v1 >= v2;
				case EOperator.Less:            return v1 <  v2;
				case EOperator.NotGreater:
				case EOperator.LessOrEqual:     return v1 <= v2;
			}

			throw new InvalidOperationException();
		}

		#endregion

		#region DataTypes

		protected virtual int GetMaxLength     (ISqlDataType type) { return SqlDataType.GetMaxLength     (type.DataType); }
		protected virtual int GetMaxPrecision  (ISqlDataType type) { return SqlDataType.GetMaxPrecision  (type.DataType); }
		protected virtual int GetMaxScale      (ISqlDataType type) { return SqlDataType.GetMaxScale      (type.DataType); }
		protected virtual int GetMaxDisplaySize(ISqlDataType type) { return SqlDataType.GetMaxDisplaySize(type.DataType); }

		protected virtual IQueryExpression ConvertConvertion(ISqlFunction func)
		{
			var from = (ISqlDataType)func.Parameters[1];
			var to   = (ISqlDataType)func.Parameters[0];

			if (to.Type == typeof(object))
				return func.Parameters[2];

			if (to.Precision > 0)
			{
				var maxPrecision = GetMaxPrecision(from);
				var maxScale     = GetMaxScale    (from);
				var newPrecision = maxPrecision >= 0 ? Math.Min(to.Precision ?? 0, maxPrecision) : to.Precision;
				var newScale     = maxScale     >= 0 ? Math.Min(to.Scale     ?? 0, maxScale)     : to.Scale;

				if (to.Precision != newPrecision || to.Scale != newScale)
					to = new SqlDataType(to.DataType, to.Type, null, newPrecision, newScale);
			}
			else if (to.Length > 0)
			{
				var maxLength = to.Type == typeof(string) ? GetMaxDisplaySize(from) : GetMaxLength(from);
				var newLength = maxLength >= 0 ? Math.Min(to.Length ?? 0, maxLength) : to.Length;

				if (to.Length != newLength)
					to = new SqlDataType(to.DataType, to.Type, newLength, null, null);
			}
			else if (from.Type == typeof(short) && to.Type == typeof(int))
				return func.Parameters[2];

			return ConvertExpression(new SqlFunction(func.SystemType, "Convert", to, func.Parameters[2]));
		}

		#endregion

		#region Alternative Builders

		protected IQueryExpression AlternativeConvertToBoolean(ISqlFunction func, int paramNumber)
		{
			var par = func.Parameters[paramNumber];

			if (par.SystemType.IsFloatType() || par.SystemType.IsIntegerType())
			{
				var sc = new SearchCondition();

				sc.Conditions.Add(
					new Condition(false, new ExprExpr(par, EOperator.Equal, new SqlValue(0))));

				return ConvertExpression(new SqlFunction(func.SystemType, "CASE", sc, new SqlValue(false), new SqlValue(true)));
			}

			return null;
		}

		protected static bool IsDateDataType(IQueryExpression expr, string dateName)
		{
			switch (expr.ElementType)
			{
				case EQueryElementType.SqlDataType   : return ((ISqlDataType)  expr).DataType == DataType.Date;
				case EQueryElementType.SqlExpression : return ((ISqlExpression)expr).Expr     == dateName;
			}

			return false;
		}

		protected static bool IsTimeDataType(IQueryExpression expr)
		{
			switch (expr.ElementType)
			{
				case EQueryElementType.SqlDataType   : return ((ISqlDataType)expr).  DataType == DataType.Time;
				case EQueryElementType.SqlExpression : return ((ISqlExpression)expr).Expr     == "Time";
			}

			return false;
		}

		protected IQueryExpression FloorBeforeConvert(ISqlFunction func)
		{
			var par1 = func.Parameters[1];

			return par1.SystemType.IsFloatType() && func.SystemType.IsIntegerType() ?
				new SqlFunction(func.SystemType, "Floor", par1) : par1;
		}

		protected ISelectQuery GetAlternativeDelete(ISelectQuery selectQuery)
		{
		    var source = selectQuery.From.Tables[0].Source as ISqlTable;
		    if (selectQuery.IsDelete && 
				(selectQuery.From.Tables.Count > 1 || selectQuery.From.Tables[0].Joins.Count > 0) && 
				source != null)
			{
				var sql = new SelectQuery { EQueryType = EQueryType.Delete, IsParameterDependent = selectQuery.IsParameterDependent };

				selectQuery.ParentSelect = sql;
				selectQuery.EQueryType = EQueryType.Select;

				var table = source;
				var copy  = new SqlTable(table) { Alias = null };

				var tableKeys = table.GetKeys(true);
				var copyKeys  = copy. GetKeys(true);

				if (selectQuery.Where.SearchCondition.Conditions.Any(c => c.IsOr))
				{
					var sc1 = new SearchCondition(selectQuery.Where.SearchCondition.Conditions);
					var sc2 = new SearchCondition();

					for (var i = 0; i < tableKeys.Count; i++)
					{
						sc2.Conditions.Add(new Condition(
							false,
							new ExprExpr(copyKeys[i], EOperator.Equal, tableKeys[i])));
					}

					selectQuery.Where.SearchCondition.Conditions.Clear();
					selectQuery.Where.SearchCondition.Conditions.Add(new Condition(false, sc1));
					selectQuery.Where.SearchCondition.Conditions.Add(new Condition(false, sc2));
				}
				else
				{
					for (var i = 0; i < tableKeys.Count; i++)
						selectQuery.Where.Expr(copyKeys[i]).Equal.Expr(tableKeys[i]);
				}

				sql.From.Table(copy).Where.Exists(selectQuery);
				sql.Parameters.AddRange(selectQuery.Parameters);

				selectQuery.Parameters.Clear();

				selectQuery = sql;
			}

			return selectQuery;
		}

		protected ISelectQuery GetAlternativeUpdate(ISelectQuery selectQuery)
		{
		    var source = selectQuery.From.Tables[0].Source as ISqlTable;
		    if (selectQuery.IsUpdate && (source != null || selectQuery.Update.Table != null))
			{
				if (selectQuery.From.Tables.Count > 1 || selectQuery.From.Tables[0].Joins.Count > 0)
				{
					var sql = new SelectQuery { EQueryType = EQueryType.Update, IsParameterDependent = selectQuery.IsParameterDependent  };

					selectQuery.ParentSelect = sql;
					selectQuery.EQueryType = EQueryType.Select;

					var table = selectQuery.Update.Table ?? (ISqlTable)selectQuery.From.Tables[0].Source;

					if (selectQuery.Update.Table != null)
						if (new QueryVisitor().Find(selectQuery.From, t => t == table) == null)
							table = (ISqlTable)new QueryVisitor().Find(selectQuery.From,
							    ex =>
							    {
							        var sqlTable = ex as ISqlTable;
							        return sqlTable != null && sqlTable.ObjectType == table.ObjectType;
							    }) ?? table;

					var copy = new SqlTable(table);

					var tableKeys = table.GetKeys(true);
					var copyKeys  = copy. GetKeys(true);

					for (var i = 0; i < tableKeys.Count; i++)
						selectQuery.Where
							.Expr(copyKeys[i]).Equal.Expr(tableKeys[i]);

					sql.From.Table(copy).Where.Exists(selectQuery);

					var map = new Dictionary<ISqlField, ISqlField>(table.Fields.Count);

					foreach (var field in table.Fields.Values)
						map.Add(field, copy[field.Name]);

					foreach (var item in selectQuery.Update.Items)
					{
						var ex = new QueryVisitor().Convert(item, expr =>
						{
							var fld = expr as ISqlField;
							return fld != null && map.TryGetValue(fld, out fld) ? fld : expr;
						});

						sql.Update.Items.Add(ex);
					}

					sql.Parameters.AddRange(selectQuery.Parameters);
					sql.Update.Table = selectQuery.Update.Table;

					selectQuery.Parameters.Clear();
					selectQuery.Update.Items.Clear();

					selectQuery = sql;
				}

				selectQuery.From.Tables[0].Alias = "$";
			}

			return selectQuery;
		}

		#endregion

		#region Helpers

		static string SetAlias(string alias, int maxLen)
		{
			if (alias == null)
				return null;

			alias = alias.TrimStart('_');

			var cs      = alias.ToCharArray();
			var replace = false;

			for (var i = 0; i < cs.Length; i++)
			{
				var c = cs[i];

				if (c >= 'a' && c <= 'z' || c >= 'A' && c <= 'Z' || c >= '0' && c <= '9' || c == '_')
					continue;

				cs[i] = ' ';
				replace = true;
			}

			if (replace)
				alias = new string(cs).Replace(" ", "");

			return alias.Length == 0 || alias.Length > maxLen ? null : alias;
		}

		protected void CheckAliases(ISelectQuery selectQuery, int maxLen)
		{
		    foreach (var elementType in QueryVisitor.FindOnce<IQueryElement>(selectQuery))
		    {
                switch (elementType.ElementType)
                {
                    case EQueryElementType.SqlField: ((ISqlField)elementType).Alias = SetAlias(((ISqlField)elementType).Alias, maxLen); break;
                    case EQueryElementType.SqlParameter: ((ISqlParameter)elementType).Name = SetAlias(((ISqlParameter)elementType).Name, maxLen); break;
                    case EQueryElementType.SqlTable: ((ISqlTable)elementType).Alias = SetAlias(((ISqlTable)elementType).Alias, maxLen); break;
                    case EQueryElementType.Column: ((IColumn)elementType).Alias = SetAlias(((IColumn)elementType).Alias, maxLen); break;
                    case EQueryElementType.TableSource: ((ITableSource)elementType).Alias = SetAlias(((ITableSource)elementType).Alias, maxLen); break;
                }
            }
		}

		public IQueryExpression Add(IQueryExpression expr1, IQueryExpression expr2, Type type)
		{
			return ConvertExpression(new SqlBinaryExpression(type, expr1, "+", expr2, Precedence.Additive));
		}

		public IQueryExpression Add<T>(IQueryExpression expr1, IQueryExpression expr2)
		{
			return Add(expr1, expr2, typeof(T));
		}

		public IQueryExpression Add(IQueryExpression expr1, int value)
		{
			return Add<int>(expr1, new SqlValue(value));
		}

		public IQueryExpression Inc(IQueryExpression expr1)
		{
			return Add(expr1, 1);
		}

		public IQueryExpression Sub(IQueryExpression expr1, IQueryExpression expr2, Type type)
		{
			return ConvertExpression(new SqlBinaryExpression(type, expr1, "-", expr2, Precedence.Subtraction));
		}

		public IQueryExpression Sub<T>(IQueryExpression expr1, IQueryExpression expr2)
		{
			return Sub(expr1, expr2, typeof(T));
		}

		public IQueryExpression Sub(IQueryExpression expr1, int value)
		{
			return Sub<int>(expr1, new SqlValue(value));
		}

		public IQueryExpression Dec(IQueryExpression expr1)
		{
			return Sub(expr1, 1);
		}

		public IQueryExpression Mul(IQueryExpression expr1, IQueryExpression expr2, Type type)
		{
			return ConvertExpression(new SqlBinaryExpression(type, expr1, "*", expr2, Precedence.Multiplicative));
		}

		public IQueryExpression Mul<T>(IQueryExpression expr1, IQueryExpression expr2)
		{
			return Mul(expr1, expr2, typeof(T));
		}

		public IQueryExpression Mul(IQueryExpression expr1, int value)
		{
			return Mul<int>(expr1, new SqlValue(value));
		}

		public IQueryExpression Div(IQueryExpression expr1, IQueryExpression expr2, Type type)
		{
			return ConvertExpression(new SqlBinaryExpression(type, expr1, "/", expr2, Precedence.Multiplicative));
		}

		public IQueryExpression Div<T>(IQueryExpression expr1, IQueryExpression expr2)
		{
			return Div(expr1, expr2, typeof(T));
		}

		public IQueryExpression Div(IQueryExpression expr1, int value)
		{
			return Div<int>(expr1, new SqlValue(value));
		}

		#endregion
	}
}
