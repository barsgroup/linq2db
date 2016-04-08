namespace LinqToDB.SqlQuery
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using LinqToDB.Extensions;
    using LinqToDB.SqlProvider;
    using LinqToDB.SqlQuery.QueryElements.Clauses.Interfaces;
    using LinqToDB.SqlQuery.QueryElements.Conditions;
    using LinqToDB.SqlQuery.QueryElements.Conditions.Interfaces;
    using LinqToDB.SqlQuery.QueryElements.Enums;
    using LinqToDB.SqlQuery.QueryElements.Interfaces;
    using LinqToDB.SqlQuery.QueryElements.Predicates;
    using LinqToDB.SqlQuery.QueryElements.Predicates.Interfaces;
    using LinqToDB.SqlQuery.QueryElements.SqlElements;
    using LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces;

    internal class SelectQueryOptimizer
    {
        private readonly SqlProviderFlags _flags;

        private readonly ISelectQuery _selectQuery;

        public SelectQueryOptimizer(SqlProviderFlags flags, ISelectQuery selectQuery)
        {
            _flags = flags;
            _selectQuery = selectQuery;
        }

        public void FinalizeAndValidate(bool isApplySupported, bool optimizeColumns)
        {
            OptimizeUnions();

            FinalizeAndValidateInternal(isApplySupported, optimizeColumns, new HashSet<ISqlTableSource>());

            QueryVisitor.FindOnce<ISelectQuery>(_selectQuery).ForEach(
                node =>
                    {
                        var item = node.Value;
                        if (item != _selectQuery)
                        {
                            RemoveOrderBy(item);
                        }
                    });

            ResolveFields();
            _selectQuery.SetAliases();


            //#if DEBUG
            //			sqlText = _selectQuery.SqlText;
            //#endif
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
                var cond = searchCondition.Conditions.First.Value;

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
                        var c1 = condition.Conditions.First.Value;

                        var predicate = c1.Predicate as IExprExpr;
                        if (!c1.IsNot && predicate != null)
                        {
                            EOperator op;

                            switch (predicate.EOperator)
                            {
                                case EOperator.Equal:
                                    op = EOperator.NotEqual;
                                    break;
                                case EOperator.NotEqual:
                                    op = EOperator.Equal;
                                    break;
                                case EOperator.Greater:
                                    op = EOperator.LessOrEqual;
                                    break;
                                case EOperator.NotLess:
                                case EOperator.GreaterOrEqual:
                                    op = EOperator.Less;
                                    break;
                                case EOperator.Less:
                                    op = EOperator.GreaterOrEqual;
                                    break;
                                case EOperator.NotGreater:
                                case EOperator.LessOrEqual:
                                    op = EOperator.Greater;
                                    break;
                                default:
                                    throw new InvalidOperationException();
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
                    {
                        if (cond.IsNot
                                ? !(bool)sqlValue.Value
                                : (bool)sqlValue.Value)
                        {
                            searchCondition.Conditions.Clear();
                        }
                    }
                }
            }

            searchCondition.Conditions.ApplyUntilNonDefaultResult(
                node =>
                    {
                        var cond = node.Value;

                        if (cond.Predicate.ElementType == EQueryElementType.ExprPredicate)
                        {
                            var expr = (IExpr)cond.Predicate;

                            var sqlValue = expr.Expr1 as ISqlValue;

                            if (sqlValue?.Value is bool)
                            {
                                if (cond.IsNot
                                        ? !(bool)sqlValue.Value
                                        : (bool)sqlValue.Value)
                                {
                                    if (node.Previous != null && node.Previous.Value.IsOr)
                                    {
                                        node.ReverseEach(listNode => searchCondition.Conditions.Remove(listNode));

                                        OptimizeSearchCondition(searchCondition);

                                        return false;
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

                        return true;
                    });
        }

        internal void ResolveWeakJoins(HashSet<ISqlTableSource> tables)
        {
            Func<ITableSource, bool> findTable = null;
            findTable = table =>
            {
                if (tables.Contains(table.Source))
                {
                    return true;
                }

                var result = table.Joins.ApplyUntilNonDefaultResult(
                    node =>
                        {
                            if (findTable(node.Value.Table))
                            {
                                node.Value.IsWeak = false;
                                return true;
                            }

                            return false;
                        });

                if (result)
                {
                    return true;
                }

                var selectQuery = table.Source as ISelectQuery;
                return selectQuery != null && selectQuery.From.Tables.Any(t => findTable(t));
            };

            var areTablesCollected = false;
            
            QueryVisitor.FindOnce<ITableSource>(_selectQuery).ForEach(
                item =>
                    {
                        var table = item.Value;
                        table.Joins.ForEach(
                            node =>
                                {
                                    var join = node.Value;

                                    if (!join.IsWeak)
                                    {
                                        return;
                                    }

                                    if (!areTablesCollected)
                                    {
                                        areTablesCollected = true;

                                        var items = new LinkedList<IQueryElement>();
                                        items.AddLast(_selectQuery.Select);
                                        items.AddLast(_selectQuery.Where);
                                        items.AddLast(_selectQuery.GroupBy);
                                        items.AddLast(_selectQuery.Having);
                                        items.AddLast(_selectQuery.OrderBy);
                                        if (_selectQuery.IsInsert)
                                        {
                                            items.AddLast(_selectQuery.Insert);
                                        }
                                        if (_selectQuery.IsUpdate)
                                        {
                                            items.AddLast(_selectQuery.Update);
                                        }
                                        if (_selectQuery.IsDelete)
                                        {
                                            items.AddLast(_selectQuery.Delete);
                                        }
                                        
                                        QueryVisitor.FindOnce<ISqlTable>(_selectQuery.From).ForEach(
                                            fromTable =>
                                                {
                                                    var tableArguments = fromTable.Value.TableArguments;

                                                    if (tableArguments == null)
                                                    {
                                                        return;
                                                    }

                                                    items.AddRange(tableArguments);
                                                });

                                        QueryVisitor.FindOnce<ISqlField>(items).ForEach(
                                            field =>
                                                {
                                                    tables.Add(field.Value.Table);
                                                });
                                    }

                                    if (findTable(join.Table))
                                    {
                                        join.IsWeak = false;
                                    }
                                    else
                                    {
                                        table.Joins.Remove(join);
                                    }
                                });
                    });
        }

        private static bool CheckColumn(IColumn column, IQueryExpression expr, ISelectQuery query, bool optimizeValues, bool optimizeColumns)
        {
            if (expr is ISqlField || expr is IColumn)
            {
                return false;
            }

            var sqlValue = expr as ISqlValue;
            if (sqlValue != null)
            {
                return !optimizeValues && 1.Equals(sqlValue.Value);
            }

            var sqlBinaryExpression = expr as ISqlBinaryExpression;
            if (sqlBinaryExpression != null)
            {
                var e = sqlBinaryExpression;

                var expr1 = e.Expr1 as ISqlValue;
                if (e.Operation == "*" && expr1 != null)
                {
                    if (expr1.Value is int && (int)expr1.Value == -1)
                    {
                        return CheckColumn(column, e.Expr2, query, optimizeValues, optimizeColumns);
                    }
                }
            }

            if (optimizeColumns && QueryVisitor.Find(expr, e => e is ISelectQuery || IsAggregationFunction(e)) == null)
            {
                var q = query.ParentSelect ?? query;
                var count = QueryVisitor.FindOnce<IColumn>(q).Count(e => e == column);

                return count > 2;
            }

            return true;
        }

        private static void ConcatSearchCondition(IHaveSearchCondition fromCondition, IHaveSearchCondition ToCondition)
        {
            if (fromCondition.IsEmpty)
            {
                fromCondition.Search.Conditions.AddRange(ToCondition.Search.Conditions);
            }
            else
            {
                if (fromCondition.Search.Precedence < Precedence.LogicalConjunction)
                {
                    var sc1 = new SearchCondition();

                    sc1.Conditions.AddRange(fromCondition.Search.Conditions);

                    fromCondition.Search.Conditions.Clear();
                    fromCondition.Search.Conditions.AddLast(new Condition(false, sc1));
                }

                if (ToCondition.Search.Precedence < Precedence.LogicalConjunction)
                {
                    var sc2 = new SearchCondition();

                    sc2.Conditions.AddRange(ToCondition.Search.Conditions);

                    fromCondition.Search.Conditions.AddLast(new Condition(false, sc2));
                }
                else
                {
                    fromCondition.Search.Conditions.AddRange(ToCondition.Search.Conditions);
                }
            }
        }

        private static bool ContainsTable(ISqlTableSource table, IQueryElement sql)
        {
            return null !=
                   QueryVisitor.Find(
                       sql,
                       e =>
                       e == table || e.ElementType == EQueryElementType.SqlField && table == ((ISqlField)e).Table ||
                       e.ElementType == EQueryElementType.Column && table == ((IColumn)e).Parent);
        }

        private void FinalizeAndValidateInternal(bool isApplySupported, bool optimizeColumns, HashSet<ISqlTableSource> tables)
        {
            OptimizeSearchCondition(_selectQuery.Where.Search);
            OptimizeSearchCondition(_selectQuery.Having.Search);

            QueryVisitor.FindOnce<IJoinedTable>(_selectQuery).ForEach(
                joinTable =>
                    {
                        OptimizeSearchCondition(joinTable.Value.Condition);
                    });

            QueryVisitor.FindDownTo<ISelectQuery>(_selectQuery).ForEach(
                node =>
                    {
                        var query = node.Value;

                        if (query == _selectQuery)
                        {
                            return;
                        }

                        query.ParentSelect = _selectQuery;

                        new SelectQueryOptimizer(_flags, query).FinalizeAndValidateInternal(isApplySupported, optimizeColumns, tables);

                        if (query.IsParameterDependent)
                        {
                            _selectQuery.IsParameterDependent = true;
                        }
                    });

            ResolveWeakJoins(tables);
            OptimizeColumns();
            OptimizeApplies(isApplySupported, optimizeColumns);
            OptimizeSubQueries(isApplySupported, optimizeColumns);
            OptimizeApplies(isApplySupported, optimizeColumns);

        }

        private static ITableSource FindField(ISqlField field, ITableSource table)
        {
            if (field.Table == table.Source)
            {
                return table;
            }

            return table.Joins.ApplyUntilNonDefaultResult(
                node =>
                    {
                        var t = FindField(field, node.Value.Table);

                        return t != null
                                   ? node.Value.Table
                                   : null;
                    });

        }

        private static IQueryExpression GetColumn(QueryData data, ISqlField field)
        {
            foreach (var query in data.Queries)
            {
                var q = query.Query;

                foreach (var table in q.From.Tables)
                {
                    var t = FindField(field, table);

                    if (t != null)
                    {
                        var n = q.Select.Columns.Count;
                        var idx = q.Select.Add(field);

                        if (n != q.Select.Columns.Count)
                        {
                            if (!q.GroupBy.IsEmpty || q.Select.Columns.Any(c => IsAggregationFunction(c.Expression)))
                            {
                                q.GroupBy.Items.AddLast(field);
                            }
                        }

                        return q.Select.Columns[idx];
                    }
                }
            }

            return null;
        }

        private static QueryData GetQueryData(ISelectQuery selectQuery)
        {
            var data = new QueryData
                       {
                           Query = selectQuery
                       };

            QueryVisitor.FindParentFirst(
                selectQuery,
                e =>
                {
                    switch (e.ElementType)
                    {
                        case EQueryElementType.SqlField:
                        {
                            var field = (ISqlField)e;

                            if (field.Name.Length != 1 || field.Name[0] != '*')
                            {
                                data.Fields.Add(field);
                            }

                            break;
                        }

                        case EQueryElementType.SqlQuery:
                        {
                            if (e != selectQuery)
                            {
                                data.Queries.Add(GetQueryData((ISelectQuery)e));
                                return false;
                            }

                            break;
                        }

                        case EQueryElementType.Column:
                            return ((IColumn)e).Parent == selectQuery;

                        case EQueryElementType.SqlTable:
                            return false;
                    }

                    return true;
                });

            return data;
        }

        private static bool IsAggregationFunction(IQueryElement expr)
        {
            var sqlFunction = expr as ISqlFunction;
            if (sqlFunction != null)
            {
                switch (sqlFunction.Name)
                {
                    case "Count":
                    case "Average":
                    case "Min":
                    case "Max":
                    case "Sum":
                        return true;
                }
            }

            return false;
        }

        private void OptimizeApplies(bool isApplySupported, bool optimizeColumns)
        {
            _selectQuery.From.Tables.ForEach(
                tableNode =>
                {
                    var table = tableNode.Value;
                    table.Joins.ForEach(
                        joinNode =>
                        {
                            var join = joinNode.Value;
                            if (join.JoinType == EJoinType.CrossApply || join.JoinType == EJoinType.OuterApply)
                            {
                                OptimizeApply(table, join, isApplySupported, optimizeColumns);
                            }
                        });
                });
        }

        private void OptimizeApply(ITableSource tableSource, IJoinedTable joinTable, bool isApplySupported, bool optimizeColumns)
        {
            var joinSource = joinTable.Table;

            joinSource.Joins.ForEach(
                        joinNode =>
                        {
                            var join = joinNode.Value;
                            if (join.JoinType == EJoinType.CrossApply || join.JoinType == EJoinType.OuterApply)
                            {
                                OptimizeApply(joinSource, join, isApplySupported, optimizeColumns);
                            }
                        });

            if (isApplySupported && !joinTable.CanConvertApply)
            {
                return;
            }

            if (joinSource.Source.ElementType == EQueryElementType.SqlQuery)
            {
                var sql = (ISelectQuery)joinSource.Source;
                var isAgg = sql.Select.Columns.Any(c => IsAggregationFunction(c.Expression));

                if (isApplySupported && (isAgg || sql.Select.TakeValue != null || sql.Select.SkipValue != null))
                {
                    return;
                }

                sql.Where.Search.Conditions.Clear();

                if (!ContainsTable(tableSource.Source, sql))
                {
                    joinTable.JoinType = joinTable.JoinType == EJoinType.CrossApply
                                             ? EJoinType.Inner
                                             : EJoinType.Left;
                    joinTable.Condition.Conditions.AddRange(sql.Where.Search.Conditions);
                }
                else
                {
                    sql.Where.Search.Conditions.AddRange(sql.Where.Search.Conditions);

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
                        {
                            foreach (var item in q.OrderBy.Items)
                            {
                                _selectQuery.OrderBy.Expr(item.Expression, item.IsDescending);
                            }
                        }

                        joinTable.Table = table;

                        OptimizeApply(tableSource, joinTable, isApplySupported, optimizeColumns);
                    }
                }
            }
            else
            {
                if (!ContainsTable(tableSource.Source, joinSource.Source))
                {
                    joinTable.JoinType = joinTable.JoinType == EJoinType.CrossApply
                                             ? EJoinType.Inner
                                             : EJoinType.Left;
                }
            }
        }

        private void OptimizeColumns()
        {
            _selectQuery.Select.Walk(
                false,
                expr =>
                {
                    var query = expr as ISelectQuery;

                    if (query != null && query.From.Tables.Count == 0 && query.Select.Columns.Count == 1)
                    {
                        QueryVisitor.FindOnce<ISelectQuery>(query.Select.Columns[0].Expression).ForEach(
                            node =>
                                {
                                    if (node.Value.ParentSelect == query)
                                    {
                                        node.Value.ParentSelect = query.ParentSelect;
                                    }
                                });


                        return query.Select.Columns[0].Expression;
                    }

                    return expr;
                });
        }

        private void OptimizeSubQueries(bool isApplySupported, bool optimizeColumns)
        {
            _selectQuery.From.Tables.ForEach(
                source =>
                {
                    var value = source.Value;

                    var table = OptimizeSubQuery(value, true, false, isApplySupported, true, optimizeColumns);

                    if (table != value)
                    {
                        var sql = value.Source as ISelectQuery;

                        if (!_selectQuery.Select.Columns.All(c => IsAggregationFunction(c.Expression)))
                        {
                            if (sql != null && sql.OrderBy.Items.Count > 0)
                            {
                                foreach (var item in sql.OrderBy.Items)
                                {
                                    _selectQuery.OrderBy.Expr(item.Expression, item.IsDescending);
                                }
                            }
                        }

                        source.Value = table;
                    }
                });
        }

        private ITableSource OptimizeSubQuery(ITableSource source, bool optimizeWhere, bool allColumns, bool isApplySupported, bool optimizeValues, bool optimizeColumns, IJoinedTable joinedTable = null)
        {
            source.Joins.ForEach(
                node =>
                {
                    var jt = node.Value;
                    var table = OptimizeSubQuery(
                        jt.Table,
                        jt.JoinType == EJoinType.Inner || jt.JoinType == EJoinType.CrossApply || jt.JoinType == EJoinType.Left,
                        false,
                        isApplySupported,
                        jt.JoinType == EJoinType.Inner || jt.JoinType == EJoinType.CrossApply || jt.JoinType == EJoinType.Left,
                        optimizeColumns,
                        jt
                        );

                    if (table != jt.Table)
                    {
                        var sql = jt.Table.Source as ISelectQuery;

                        if (sql != null && sql.OrderBy.Items.Count > 0)
                        {
                            foreach (var item in sql.OrderBy.Items)
                            {
                                _selectQuery.OrderBy.Expr(item.Expression, item.IsDescending);
                            }
                        }

                        jt.Table = table;
                    }
                });

            return source.Source is ISelectQuery
                       ? RemoveSubQuery(source, optimizeWhere, allColumns && !isApplySupported, optimizeValues, optimizeColumns, joinedTable)
                       : source;
        }

        private void OptimizeUnions()
        {
            var exprs = new Dictionary<IQueryExpression, IQueryExpression>();
            
            QueryVisitor.FindOnce<ISelectQuery>(_selectQuery).ForEach(
                elem =>
                    {
                        var element = elem.Value;

                        if (element.From.Tables.Count != 1 || !element.IsSimple || element.IsInsert || element.IsUpdate || element.IsDelete)
                        {
                            return;
                        }

                        var table = element.From.Tables.First.Value;

                        var selectQuery = table.Source as ISelectQuery;
                        if (table.Joins.Count != 0 || selectQuery == null)
                        {
                            return;
                        }

                        if (!selectQuery.HasUnion)
                        {
                            return;
                        }

                        bool isContinue = false;
                        for (var i = 0; i < element.Select.Columns.Count; i++)
                        {
                            var scol = element.Select.Columns[i];
                            var ucol = selectQuery.Select.Columns[i];

                            if (scol.Expression != ucol)
                            {
                                isContinue = true;
                                break;
                            }
                        }

                        if (isContinue)
                        {
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
                        {
                            element.Select.Expr(selectQuery.Select.Columns[i].Expression);
                        }

                        element.From.Tables.Clear();

                        selectQuery.From.Tables.ForEach(node => element.From.Tables.AddLast(node.Value));

                        element.Where.Search.Conditions.AddRange(selectQuery.Where.Search.Conditions);
                        element.Having.Search.Conditions.AddRange(selectQuery.Having.Search.Conditions);

                        selectQuery.GroupBy.Items.ForEach(node => element.GroupBy.Items.AddLast(node.Value));

                        element.OrderBy.Items.AddRange(selectQuery.OrderBy.Items);

                        selectQuery.Unions.Last.ReverseEach(node => element.Unions.AddFirst(node.Value));
                    });

            _selectQuery.Walk(
                false,
                expr =>
                {
                    IQueryExpression e;

                    if (exprs.TryGetValue(expr, out e))
                    {
                        return e;
                    }

                    return expr;
                });
        }

        private static void RemoveOrderBy(ISelectQuery selectQuery)
        {
            if (selectQuery.OrderBy.Items.Count > 0 && selectQuery.Select.SkipValue == null && selectQuery.Select.TakeValue == null)
            {
                selectQuery.OrderBy.Items.Clear();
            }
        }

        private ITableSource RemoveSubQuery(ITableSource childSource, bool concatWhere, bool allColumns, bool optimizeValues, bool optimizeColumns, IJoinedTable parentJoin)
        {
            var query = (ISelectQuery)childSource.Source;

            var isQueryOK = query.From.Tables.Count == 1;

            isQueryOK = isQueryOK && (concatWhere || query.Where.IsEmpty && query.Having.IsEmpty);
            isQueryOK = isQueryOK && !query.HasUnion && query.GroupBy.IsEmpty && !query.Select.HasModifier;
            //isQueryOK = isQueryOK && (_flags.IsDistinctOrderBySupported || query.Select.IsDistinct );

            if (!isQueryOK)
            {
                return childSource;
            }

            var isColumnsOK = (allColumns && !query.Select.Columns.Any(c => IsAggregationFunction(c.Expression))) ||
                              !query.Select.Columns.Any(c => CheckColumn(c, c.Expression, query, optimizeValues, optimizeColumns));

            if (!isColumnsOK)
            {
                return childSource;
            }

            var top = _selectQuery;
            while (top.ParentSelect != null)
            {
                top = top.ParentSelect;
            }

            var columns = new HashSet<IColumn>(query.Select.Columns);
            top.Walk(
                false,
                expr =>
                {
                    var col = expr as IColumn;
                    if (col == null || !columns.Contains(col))
                    {
                        return expr;
                    }

                    return col.Expression;
                });

            QueryVisitor.FindOnce<IInList>(top).ForEach(
                node =>
                {
                    if (node.Value.Expr1 == query)
                    {
                        node.Value.Expr1 = query.From.Tables.First.Value;
                    }
                });

            childSource.Joins.ForEach(node => query.From.Tables.First.Value.Joins.AddLast(node.Value));

            if (query.From.Tables.First.Value.Alias == null)
            {
                query.From.Tables.First.Value.Alias = childSource.Alias;
            }

            if (!query.Where.IsEmpty)
            {
                if (parentJoin != null && parentJoin.JoinType == EJoinType.Left)
                {
                    ConcatSearchCondition(parentJoin.Condition, query.Where);
                }
                else
                {
                    ConcatSearchCondition(_selectQuery.Where, query.Where);
                }
            }
            if (!query.Having.IsEmpty)
            {
                ConcatSearchCondition(_selectQuery.Having, query.Having);
            }
            
            QueryVisitor.FindOnce<ISelectQuery>(top).ForEach(
                node =>
                {
                    if (node.Value.ParentSelect == query)
                    {
                        node.Value.ParentSelect = query.ParentSelect ?? _selectQuery;
                    }
                });

            return query.From.Tables.First.Value;
        }

        private void ResolveFields()
        {
            var root = GetQueryData(_selectQuery);

            ResolveFields(root);
        }

        private static void ResolveFields(QueryData data)
        {
            if (data.Queries.Count == 0)
            {
                return;
            }

            var dic = new Dictionary<IQueryExpression, IQueryExpression>();

            foreach (ISqlField field in data.Fields)
            {
                if (dic.ContainsKey(field))
                {
                    continue;
                }

                var found = false;

                foreach (var table in data.Query.From.Tables)
                {
                    found = FindField(field, table) != null;

                    if (found)
                    {
                        break;
                    }
                }

                if (!found)
                {
                    var expr = GetColumn(data, field);

                    if (expr != null)
                    {
                        dic.Add(field, expr);
                    }
                }
            }

            if (dic.Count > 0)
            {
                QueryVisitor.FindParentFirst(
                    data.Query,
                    e =>
                    {
                        IQueryExpression ex;

                        switch (e.ElementType)
                        {
                            case EQueryElementType.SqlQuery:
                                return e == data.Query;

                            case EQueryElementType.SqlFunction:
                            {
                                var parms = ((ISqlFunction)e).Parameters;

                                for (var i = 0; i < parms.Length; i++)
                                {
                                    if (dic.TryGetValue(parms[i], out ex))
                                    {
                                        parms[i] = ex;
                                    }
                                }

                                break;
                            }

                            case EQueryElementType.SqlExpression:
                            {
                                var parms = ((ISqlExpression)e).Parameters;

                                for (var i = 0; i < parms.Length; i++)
                                {
                                    if (dic.TryGetValue(parms[i], out ex))
                                    {
                                        parms[i] = ex;
                                    }
                                }

                                break;
                            }

                            case EQueryElementType.SqlBinaryExpression:
                            {
                                var expr = (ISqlBinaryExpression)e;
                                if (dic.TryGetValue(expr.Expr1, out ex))
                                {
                                    expr.Expr1 = ex;
                                }
                                if (dic.TryGetValue(expr.Expr2, out ex))
                                {
                                    expr.Expr2 = ex;
                                }
                                break;
                            }

                            case EQueryElementType.ExprPredicate:
                            case EQueryElementType.NotExprPredicate:
                            case EQueryElementType.IsNullPredicate:
                            case EQueryElementType.InSubQueryPredicate:
                            {
                                var expr = (IExpr)e;
                                if (dic.TryGetValue(expr.Expr1, out ex))
                                {
                                    expr.Expr1 = ex;
                                }
                                break;
                            }

                            case EQueryElementType.ExprExprPredicate:
                            {
                                var expr = (IExprExpr)e;
                                if (dic.TryGetValue(expr.Expr1, out ex))
                                {
                                    expr.Expr1 = ex;
                                }
                                if (dic.TryGetValue(expr.Expr2, out ex))
                                {
                                    expr.Expr2 = ex;
                                }
                                break;
                            }

                            case EQueryElementType.LikePredicate:
                            {
                                var expr = (ILike)e;
                                if (dic.TryGetValue(expr.Expr1, out ex))
                                {
                                    expr.Expr1 = ex;
                                }
                                if (dic.TryGetValue(expr.Expr2, out ex))
                                {
                                    expr.Expr2 = ex;
                                }
                                if (dic.TryGetValue(expr.Escape, out ex))
                                {
                                    expr.Escape = ex;
                                }
                                break;
                            }

                            case EQueryElementType.BetweenPredicate:
                            {
                                var expr = (IBetween)e;
                                if (dic.TryGetValue(expr.Expr1, out ex))
                                {
                                    expr.Expr1 = ex;
                                }
                                if (dic.TryGetValue(expr.Expr2, out ex))
                                {
                                    expr.Expr2 = ex;
                                }
                                if (dic.TryGetValue(expr.Expr3, out ex))
                                {
                                    expr.Expr3 = ex;
                                }
                                break;
                            }

                            case EQueryElementType.InListPredicate:
                            {
                                var expr = (IInList)e;

                                if (dic.TryGetValue(expr.Expr1, out ex))
                                {
                                    expr.Expr1 = ex;
                                }

                                for (var i = 0; i < expr.Values.Count; i++)
                                {
                                    if (dic.TryGetValue(expr.Values[i], out ex))
                                    {
                                        expr.Values[i] = ex;
                                    }
                                }

                                break;
                            }

                            case EQueryElementType.Column:
                            {
                                var expr = (IColumn)e;

                                if (expr.Parent != data.Query)
                                {
                                    return false;
                                }

                                if (dic.TryGetValue(expr.Expression, out ex))
                                {
                                    expr.Expression = ex;
                                }

                                break;
                            }

                            case EQueryElementType.SetExpression:
                            {
                                var expr = (ISetExpression)e;
                                if (dic.TryGetValue(expr.Expression, out ex))
                                {
                                    expr.Expression = ex;
                                }
                                break;
                            }

                            case EQueryElementType.GroupByClause:
                            {
                                var expr = (IGroupByClause)e;

                                expr.Items.ForEach(
                                    node =>
                                    {
                                        if (dic.TryGetValue(node.Value, out ex))
                                        {
                                            node.Value = ex;
                                        }
                                    });

                                break;
                            }

                            case EQueryElementType.OrderByItem:
                            {
                                var expr = (IOrderByItem)e;
                                if (dic.TryGetValue(expr.Expression, out ex))
                                {
                                    expr.Expression = ex;
                                }
                                break;
                            }
                        }

                        return true;
                    });
            }

            foreach (var query in data.Queries)
            {
                if (query.Queries.Count > 0)
                {
                    ResolveFields(query);
                }
            }
        }

        private class QueryData
        {
            public readonly List<IQueryExpression> Fields = new List<IQueryExpression>();

            public readonly List<QueryData> Queries = new List<QueryData>();

            public ISelectQuery Query;
        }
    }
}