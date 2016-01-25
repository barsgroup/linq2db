namespace LinqToDB.SqlQuery.QueryElements
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading;

    using LinqToDB.Extensions;
    using LinqToDB.Reflection;
    using LinqToDB.SqlQuery.QueryElements.Clauses;
    using LinqToDB.SqlQuery.QueryElements.Conditions;
    using LinqToDB.SqlQuery.QueryElements.Interfaces;
    using LinqToDB.SqlQuery.QueryElements.Predicates;
    using LinqToDB.SqlQuery.SqlElements;
    using LinqToDB.SqlQuery.SqlElements.Interfaces;

    [DebuggerDisplay("SQL = {SqlText}")]
	public class SelectQuery : BaseQueryElement, ISqlTableSource
	{
		#region Init

		static readonly Dictionary<string,object> _reservedWords = new Dictionary<string,object>();

		static SelectQuery()
		{
#if NETFX_CORE
			using (var stream = typeof(SelectQuery).AssemblyEx().GetManifestResourceStream("ReservedWords.txt"))
#else
			using (var stream = typeof(SelectQuery).AssemblyEx().GetManifestResourceStream(typeof(SelectQuery), "ReservedWords.txt"))
#endif
			using (var reader = new StreamReader(stream))
			{
				string s;
				while ((s = reader.ReadLine()) != null)
					_reservedWords.Add(s, s);
			}
		}

		public SelectQuery()
		{
			SourceID = Interlocked.Increment(ref SourceIDCounter);

			Select  = new SelectClause (this);
			From    = new FromClause   (this);
			Where   = new WhereClause  (this);
			GroupBy = new GroupByClause(this);
			Having  = new WhereClause  (this);
			OrderBy = new OrderByClause(this);
		}

		internal SelectQuery(int id)
		{
			SourceID = id;
		}

		internal void Init(
			InsertClause         insert,
			UpdateClause         update,
			DeleteClause         delete,
			SelectClause         select,
			FromClause           from,
			WhereClause          where,
			GroupByClause        groupBy,
			WhereClause          having,
			OrderByClause        orderBy,
			List<Union>          unions,
			SelectQuery          parentSelect,
			CreateTableStatement createTable,
			bool                 parameterDependent,
			List<SqlParameter>   parameters)
		{
			Insert              = insert;
			Update              = update;
			Delete              = delete;
			Select              = select;
			From                = from;
			Where               = where;
			GroupBy             = groupBy;
			Having              = having;
			OrderBy             = orderBy;
			Unions              = unions;
			ParentSelect         = parentSelect;
			CreateTable         = createTable;
			IsParameterDependent = parameterDependent;

			Parameters.AddRange(parameters);

			foreach (var col in select.Columns)
				col.Parent = this;

			Select. SetSqlQuery(this);
			From.   SetSqlQuery(this);
			Where.  SetSqlQuery(this);
			GroupBy.SetSqlQuery(this);
			Having. SetSqlQuery(this);
			OrderBy.SetSqlQuery(this);
		}

        public   List<SqlParameter>  Parameters { get; } = new List<SqlParameter>();

		public  List<object>  Properties { get; } = new List<object>();

	    public bool        IsParameterDependent { get; set; }
		public SelectQuery ParentSelect         { get; set; }

		public bool IsSimple => !Select.HasModifier && Where.IsEmpty && GroupBy.IsEmpty && Having.IsEmpty && OrderBy.IsEmpty;

        public  QueryType  QueryType { get; set; } = QueryType.Select;

        public bool IsCreateTable => QueryType == QueryType.CreateTable;

	    public bool IsSelect => QueryType == QueryType.Select;

	    public bool IsDelete => QueryType == QueryType.Delete;

	    public bool IsInsertOrUpdate => QueryType == QueryType.InsertOrUpdate;

	    public bool IsInsert => QueryType == QueryType.Insert || QueryType == QueryType.InsertOrUpdate;

	    public bool IsUpdate => QueryType == QueryType.Update || QueryType == QueryType.InsertOrUpdate;

	    #endregion

        public  SelectClause  Select { get; private set; }

        public CreateTableStatement CreateTable { get; private set; } = new CreateTableStatement();


        public InsertClause Insert { get; private set; } = new InsertClause();

	    public void ClearInsert()
		{
			Insert = null;
		}


        public UpdateClause Update { get; private set; } = new UpdateClause();

        public void ClearUpdate()
        {
            Update = null;
        }

		public  DeleteClause  Delete { get; private set; } = new DeleteClause();

	    public void ClearDelete()
		{
            Update = null;
		}

		#region FromClause

        public static Join InnerJoin    (ISqlTableSource table,               params Join[] joins) { return new Join(JoinType.Inner,      table, null,  false, joins); }
		public static Join InnerJoin    (ISqlTableSource table, string alias, params Join[] joins) { return new Join(JoinType.Inner,      table, alias, false, joins); }
		public static Join LeftJoin     (ISqlTableSource table,               params Join[] joins) { return new Join(JoinType.Left,       table, null,  false, joins); }
		public static Join LeftJoin     (ISqlTableSource table, string alias, params Join[] joins) { return new Join(JoinType.Left,       table, alias, false, joins); }
		public static Join Join         (ISqlTableSource table,               params Join[] joins) { return new Join(JoinType.Auto,       table, null,  false, joins); }
		public static Join Join         (ISqlTableSource table, string alias, params Join[] joins) { return new Join(JoinType.Auto,       table, alias, false, joins); }
		public static Join CrossApply   (ISqlTableSource table,               params Join[] joins) { return new Join(JoinType.CrossApply, table, null,  false, joins); }
		public static Join CrossApply   (ISqlTableSource table, string alias, params Join[] joins) { return new Join(JoinType.CrossApply, table, alias, false, joins); }
		public static Join OuterApply   (ISqlTableSource table,               params Join[] joins) { return new Join(JoinType.OuterApply, table, null,  false, joins); }
		public static Join OuterApply   (ISqlTableSource table, string alias, params Join[] joins) { return new Join(JoinType.OuterApply, table, alias, false, joins); }

		public static Join WeakInnerJoin(ISqlTableSource table,               params Join[] joins) { return new Join(JoinType.Inner,      table, null,  true,  joins); }
		public static Join WeakInnerJoin(ISqlTableSource table, string alias, params Join[] joins) { return new Join(JoinType.Inner,      table, alias, true,  joins); }
		public static Join WeakLeftJoin (ISqlTableSource table,               params Join[] joins) { return new Join(JoinType.Left,       table, null,  true,  joins); }
		public static Join WeakLeftJoin (ISqlTableSource table, string alias, params Join[] joins) { return new Join(JoinType.Left,       table, alias, true,  joins); }
		public static Join WeakJoin     (ISqlTableSource table,               params Join[] joins) { return new Join(JoinType.Auto,       table, null,  true,  joins); }
		public static Join WeakJoin     (ISqlTableSource table, string alias, params Join[] joins) { return new Join(JoinType.Auto,       table, alias, true,  joins); }

        public  FromClause  From { get; private set; }

        #endregion

        public  WhereClause  Where { get; private set; }

        public  GroupByClause  GroupBy { get; private set; }

        public  WhereClause  Having { get; private set; }

        public  OrderByClause  OrderBy { get; private set; }

        public List<Union> Unions { get; private set; } = new List<Union>();

	    public bool HasUnion => Unions != null && Unions.Count > 0;

	    public void AddUnion(SelectQuery union, bool isAll)
		{
			Unions.Add(new Union(union, isAll));
		}

		#region ProcessParameters

        public SelectQuery ProcessParameters()
        {
            if (!IsParameterDependent)
            {
                return this;
            }

            var query = new QueryVisitor().Convert(
                this,
                e =>
                {
                    switch (e.ElementType)
                    {
                        case QueryElementType.SqlParameter:
                        {
                            var p = (SqlParameter)e;

                            if (p.Value == null)
                                return new SqlValue(null);
                        }

                            break;

                        case QueryElementType.ExprExprPredicate:
                        {
                            var ee = (ExprExpr)e;

                            if (ee.Operator == Operator.Equal || ee.Operator == Operator.NotEqual)
                            {
                                object value1;
                                object value2;

                                if (ee.Expr1 is SqlValue)
                                    value1 = ((SqlValue)ee.Expr1).Value;
                                else if (ee.Expr1 is SqlParameter)
                                    value1 = ((SqlParameter)ee.Expr1).Value;
                                else
                                    break;

                                if (ee.Expr2 is SqlValue)
                                    value2 = ((SqlValue)ee.Expr2).Value;
                                else if (ee.Expr2 is SqlParameter)
                                    value2 = ((SqlParameter)ee.Expr2).Value;
                                else
                                    break;

                                var value = Equals(value1, value2);

                                if (ee.Operator == Operator.NotEqual)
                                    value = !value;

                                return new Expr(new SqlValue(value), SqlQuery.Precedence.Comparison);
                            }
                        }

                            break;

                        case QueryElementType.InListPredicate:
                            return ConvertInListPredicate((InList)e);
                    }

                    return null;
                });

            if (query == this)
            {
                return query;
            }

            query.Parameters.Clear();

            foreach (var parameter in QueryVisitor.FindAll<SqlParameter>(query).Where(p => p.IsQueryParameter))
            {
                query.Parameters.Add(parameter);
            }

            return query;
        }

        static Predicate ConvertInListPredicate(InList p)
		{
			if (p.Values == null || p.Values.Count == 0)
				return new Expr(new SqlValue(p.IsNot));

			if (p.Values.Count == 1 && p.Values[0] is SqlParameter)
			{
				var pr = (SqlParameter)p.Values[0];

				if (pr.Value == null)
					return new Expr(new SqlValue(p.IsNot));

				if (pr.Value is IEnumerable)
				{
					var items = (IEnumerable)pr.Value;

					if (p.Expr1 is ISqlTableSource)
					{
						var table = (ISqlTableSource)p.Expr1;
						var keys  = table.GetKeys(true);

						if (keys == null || keys.Count == 0)
							throw new SqlException("Cant create IN expression.");

						if (keys.Count == 1)
						{
							var values = new List<ISqlExpression>();
							var field  = GetUnderlayingField(keys[0]);
							var cd     = field.ColumnDescriptor;

							foreach (var item in items)
							{
								var value = cd.MemberAccessor.GetValue(item);
								values.Add(cd.MappingSchema.GetSqlValue(cd.MemberType, value));
							}

							if (values.Count == 0)
								return new Expr(new SqlValue(p.IsNot));

							return new InList(keys[0], p.IsNot, values);
						}

						{
							var sc = new SearchCondition();

							foreach (var item in items)
							{
								var itemCond = new SearchCondition();

								foreach (var key in keys)
								{
									var field = GetUnderlayingField(key);
									var cd    = field.ColumnDescriptor;
									var value = cd.MemberAccessor.GetValue(item);
									var cond  = value == null ?
										new Condition(false, new IsNull  (field, false)) :
										new Condition(false, new ExprExpr(field, Operator.Equal, cd.MappingSchema.GetSqlValue(value)));

									itemCond.Conditions.Add(cond);
								}

								sc.Conditions.Add(new Condition(false, new Expr(itemCond), true));
							}

							if (sc.Conditions.Count == 0)
								return new Expr(new SqlValue(p.IsNot));

							if (p.IsNot)
								return new NotExpr(sc, true, SqlQuery.Precedence.LogicalNegation);

							return new Expr(sc, SqlQuery.Precedence.LogicalDisjunction);
						}
					}

					if (p.Expr1 is SqlExpression)
					{
						var expr = (SqlExpression)p.Expr1;

						if (expr.Expr.Length > 1 && expr.Expr[0] == '\x1')
						{
							var type  = items.GetListItemType();
							var ta    = TypeAccessor.GetAccessor(type);
							var names = expr.Expr.Substring(1).Split(',');

							if (expr.Parameters.Length == 1)
							{
								var values = new List<ISqlExpression>();

								foreach (var item in items)
								{
									var ma    = ta[names[0]];
									var value = ma.GetValue(item);
									values.Add(new SqlValue(value));
								}

								if (values.Count == 0)
									return new Expr(new SqlValue(p.IsNot));

								return new InList(expr.Parameters[0], p.IsNot, values);
							}

							{
								var sc = new SearchCondition();

								foreach (var item in items)
								{
									var itemCond = new SearchCondition();

									for (var i = 0; i < expr.Parameters.Length; i++)
									{
										var sql   = expr.Parameters[i];
										var value = ta[names[i]].GetValue(item);
										var cond  = value == null ?
											new Condition(false, new IsNull  (sql, false)) :
											new Condition(false, new ExprExpr(sql, Operator.Equal, new SqlValue(value)));

										itemCond.Conditions.Add(cond);
									}

									sc.Conditions.Add(new Condition(false, new Expr(itemCond), true));
								}

								if (sc.Conditions.Count == 0)
									return new Expr(new SqlValue(p.IsNot));

								if (p.IsNot)
									return new NotExpr(sc, true, SqlQuery.Precedence.LogicalNegation);

								return new Expr(sc, SqlQuery.Precedence.LogicalDisjunction);
							}
						}
					}
				}
			}

			return null;
		}

		static SqlField GetUnderlayingField(ISqlExpression expr)
		{
			switch (expr.ElementType)
			{
				case QueryElementType.SqlField: return (SqlField)expr;
				case QueryElementType.Column  : return GetUnderlayingField(((Column)expr).Expression);
			}

			throw new InvalidOperationException();
		}

		#endregion

		#region Clone

		SelectQuery(SelectQuery clone, Dictionary<ICloneableElement,ICloneableElement> objectTree, Predicate<ICloneableElement> doClone)
		{
			objectTree.Add(clone,     this);
			objectTree.Add(clone.All, All);

			SourceID = Interlocked.Increment(ref SourceIDCounter);

			ICloneableElement parentClone;

			if (clone.ParentSelect != null)
				ParentSelect = objectTree.TryGetValue(clone.ParentSelect, out parentClone) ? (SelectQuery)parentClone : clone.ParentSelect;

			QueryType = clone.QueryType;

			if (IsInsert) Insert = (InsertClause)clone.Insert.Clone(objectTree, doClone);
			if (IsUpdate) Update = (UpdateClause)clone.Update.Clone(objectTree, doClone);
			if (IsDelete) Delete = (DeleteClause)clone.Delete.Clone(objectTree, doClone);

			Select  = new SelectClause (this, clone.Select,  objectTree, doClone);
			From    = new FromClause   (this, clone.From,    objectTree, doClone);
			Where   = new WhereClause  (this, clone.Where,   objectTree, doClone);
			GroupBy = new GroupByClause(this, clone.GroupBy, objectTree, doClone);
			Having  = new WhereClause  (this, clone.Having,  objectTree, doClone);
			OrderBy = new OrderByClause(this, clone.OrderBy, objectTree, doClone);

			Parameters.AddRange(clone.Parameters.Select(p => (SqlParameter)p.Clone(objectTree, doClone)));
			IsParameterDependent = clone.IsParameterDependent;

		    foreach (var query in QueryVisitor.FindAll<SelectQuery>(this).Where(sq => sq.ParentSelect == clone))
		    {
		        query.ParentSelect = this;
		    }
		}

		public SelectQuery Clone()
		{
			return (SelectQuery)Clone(new Dictionary<ICloneableElement,ICloneableElement>(), _ => true);
		}

		public SelectQuery Clone(Predicate<ICloneableElement> doClone)
		{
			return (SelectQuery)Clone(new Dictionary<ICloneableElement,ICloneableElement>(), doClone);
		}

		#endregion

		#region Aliases

		IDictionary<string,object> _aliases;

		public void RemoveAlias(string alias)
		{
			if (_aliases != null)
			{
				alias = alias.ToUpper();
				if (_aliases.ContainsKey(alias))
					_aliases.Remove(alias);
			}
		}

		public string GetAlias(string desiredAlias, string defaultAlias)
		{
			if (_aliases == null)
				_aliases = new Dictionary<string,object>();

			var alias = desiredAlias;

			if (string.IsNullOrEmpty(desiredAlias) || desiredAlias.Length > 25)
			{
				desiredAlias = defaultAlias;
				alias        = defaultAlias + "1";
			}

			for (var i = 1; ; i++)
			{
				var s = alias.ToUpper();

				if (!_aliases.ContainsKey(s) && !_reservedWords.ContainsKey(s))
				{
					_aliases.Add(s, s);
					break;
				}

				alias = desiredAlias + i;
			}

			return alias;
		}

		public string[] GetTempAliases(int n, string defaultAlias)
		{
			var aliases = new string[n];

			for (var i = 0; i < aliases.Length; i++)
				aliases[i] = GetAlias(defaultAlias, defaultAlias);

			foreach (var t in aliases)
				RemoveAlias(t);

			return aliases;
		}

        internal void SetAliases()
        {
            _aliases = null;

            var objs = new Dictionary<object, object>();

            Parameters.Clear();

            foreach (var element in QueryVisitor.FindAll<IQueryElement>(this))
            {
                switch (element.ElementType)
                {
                    case QueryElementType.SqlParameter:
                    {
                        var p = (SqlParameter)element;

                        if (p.IsQueryParameter)
                        {
                            if (!objs.ContainsKey(element))
                            {
                                objs.Add(element, element);
                                p.Name = GetAlias(p.Name, "p");
                            }

                            Parameters.Add(p);
                        }
                        else
                            IsParameterDependent = true;
                    }

                        break;

                    case QueryElementType.Column:
                    {
                        if (!objs.ContainsKey(element))
                        {
                            objs.Add(element, element);

                            var c = (Column)element;

                            if (c.Alias != "*")
                                c.Alias = GetAlias(c.Alias, "c");
                        }
                    }

                        break;

                    case QueryElementType.TableSource:
                    {
                        var table = (TableSource)element;

                        if (!objs.ContainsKey(table))
                        {
                            objs.Add(table, table);
                            table.Alias = GetAlias(table.Alias, "t");
                        }
                    }

                        break;

                    case QueryElementType.SqlQuery:
                    {
                        var sql = (SelectQuery)element;

                        if (sql.HasUnion)
                        {
                            var columns = sql.Select.Columns.ToArray();

                            for (var i = 0; i < columns.Length; i++)
                            {
                                var col = columns[i];

                                foreach (var unions in sql.Unions.Select(t => t.SelectQuery.Select.Columns.ToArray()))
                                {
                                    objs.Remove(unions[i].Alias);

                                    unions[i].Alias = col.Alias;
                                }
                            }
                        }
                    }

                        break;
                }
            }
        }

        public void ForEachTable(Action<TableSource> action, HashSet<SelectQuery> visitedQueries)
        {
            if (!visitedQueries.Add(this))
                return;

            foreach (var table in From.Tables)
                table.ForEach(action, visitedQueries);

            foreach (var query in QueryVisitor.FindAll<SelectQuery>(this).Where(q => q != this))
            {
                query.ForEachTable(action, visitedQueries);

            }
        }

        public ISqlTableSource GetTableSource(ISqlTableSource table)
		{
			var ts = From[table];

//			if (ts == null && IsUpdate && Update.Table == table)
//				return Update.Table;

			return ts == null && ParentSelect != null? ParentSelect.GetTableSource(table) : ts;
		}

		public static TableSource CheckTableSource(TableSource ts, ISqlTableSource table, string alias)
		{
			if (ts.Source == table && (alias == null || ts.Alias == alias))
				return ts;

			var jt = ts[table, alias];

			if (jt != null)
				return jt;

			if (ts.Source is SelectQuery)
			{
				var s = ((SelectQuery)ts.Source).From[table, alias];

				if (s != null)
					return s;
			}

			return null;
		}

		#endregion

		public string SqlText => ToString();

		#region ISqlExpression Members

		public bool CanBeNull()
		{
			return true;
		}

		public bool Equals(ISqlExpression other, Func<ISqlExpression,ISqlExpression,bool> comparer)
		{
			return this == other;
		}

		public int Precedence => SqlQuery.Precedence.Unknown;

	    public Type SystemType
		{
			get
			{
				if (Select.Columns.Count() == 1)
					return Select.Columns.First().SystemType;

				if (From.Tables.Count == 1 && From.Tables[0].Joins.Count == 0)
					return From.Tables[0].SystemType;

				return null;
			}
		}

		#endregion

		#region ICloneableElement Members

		public ICloneableElement Clone(Dictionary<ICloneableElement, ICloneableElement> objectTree, Predicate<ICloneableElement> doClone)
		{
			if (!doClone(this))
				return this;

			ICloneableElement clone;

			if (!objectTree.TryGetValue(this, out clone))
				clone = new SelectQuery(this, objectTree, doClone);

			return clone;
		}

		#endregion

		#region ISqlExpressionWalkable Members

		ISqlExpression ISqlExpressionWalkable.Walk(bool skipColumns, Func<ISqlExpression,ISqlExpression> func)
		{
		    ((ISqlExpressionWalkable)Insert)?.Walk(skipColumns, func);
		    ((ISqlExpressionWalkable)Update)?.Walk(skipColumns, func);
		    ((ISqlExpressionWalkable)Delete)?.Walk(skipColumns, func);

		    ((ISqlExpressionWalkable)Select) .Walk(skipColumns, func);
			((ISqlExpressionWalkable)From)   .Walk(skipColumns, func);
			((ISqlExpressionWalkable)Where)  .Walk(skipColumns, func);
			((ISqlExpressionWalkable)GroupBy).Walk(skipColumns, func);
			((ISqlExpressionWalkable)Having) .Walk(skipColumns, func);
			((ISqlExpressionWalkable)OrderBy).Walk(skipColumns, func);

			if (HasUnion)
				foreach (var union in Unions)
					((ISqlExpressionWalkable)union.SelectQuery).Walk(skipColumns, func);

			return func(this);
		}

		#endregion

		#region IEquatable<ISqlExpression> Members

		bool IEquatable<ISqlExpression>.Equals(ISqlExpression other)
		{
			return this == other;
		}

		#endregion

		#region ISqlTableSource Members

		public static int SourceIDCounter;

		public int           SourceID     { get; private set; }
		public SqlTableType  SqlTableType => SqlTableType.Table;

	    private SqlField _all;
		public  SqlField  All
		{
			get { return _all ?? (_all = new SqlField { Name = "*", PhysicalName = "*", Table = this }); }

			internal set
			{
				_all = value;

				if (_all != null)
					_all.Table = this;
			}
		}

		List<ISqlExpression> _keys;

		public IList<ISqlExpression> GetKeys(bool allIfEmpty)
		{
			if (_keys == null && From.Tables.Count == 1 && From.Tables[0].Joins.Count == 0)
			{
				_keys = new List<ISqlExpression>();

				var q =
					from key in ((ISqlTableSource)From.Tables[0]).GetKeys(allIfEmpty)
					from col in Select.Columns
					where col.Expression == key
					select col as ISqlExpression;

				_keys = q.ToList();
			}

			return _keys;
		}

		#endregion

		#region IQueryElement Members

        protected override IEnumerable<IQueryElement> GetChildItemsInternal()
        {
            switch (QueryType)
            {
                case QueryType.InsertOrUpdate:

                    yield return Insert;
                    yield return Update;

                    if (From.Tables.Count != 0)
                    {
                        yield return Select;
                    }
                    break;

                case QueryType.Update:
                    yield return Update;
                    yield return Select;
                    break;

                case QueryType.Delete:
                    yield return Delete;
                    yield return Select;
                    break;

                case QueryType.Insert:
                    yield return Insert;

                    if (From.Tables.Count != 0)
                    {
                        yield return Select;
                    }

                    break;

                default:
                    yield return Select;
                    break;
            }
            yield return From;
            yield return Where;
            yield return GroupBy;
            yield return Having;
            yield return OrderBy;

            if (HasUnion)
            {
                foreach (var union in Unions)
                {
                    if (union.SelectQuery == this)
                        throw new InvalidOperationException();

                    yield return union;
                }
            }
        }

        public override QueryElementType ElementType => QueryElementType.SqlQuery;

	    public sealed override StringBuilder ToString(StringBuilder sb, Dictionary<IQueryElement,IQueryElement> dic)
		{
			if (dic.ContainsKey(this))
				return sb.Append("...");

			dic.Add(this, this);

			sb
				.Append("(")
				.Append(SourceID)
				.Append(") ");

			((IQueryElement)Select). ToString(sb, dic);
			((IQueryElement)From).   ToString(sb, dic);
			((IQueryElement)Where).  ToString(sb, dic);
			((IQueryElement)GroupBy).ToString(sb, dic);
			((IQueryElement)Having). ToString(sb, dic);
			((IQueryElement)OrderBy).ToString(sb, dic);

			if (HasUnion)
				foreach (IQueryElement u in Unions)
					u.ToString(sb, dic);

			dic.Remove(this);

			return sb;
		}

		#endregion
	}

 
}
