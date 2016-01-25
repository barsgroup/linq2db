namespace LinqToDB.SqlQuery
{
    using LinqToDB.SqlQuery.QueryElements;
    using LinqToDB.SqlQuery.QueryElements.Conditions;
    using LinqToDB.SqlQuery.SqlElements.Interfaces;

    public static class Extensions
	{
		public static Join InnerJoin    (this ISqlTableSource table,               params Join[] joins) { return SelectQuery.InnerJoin    (table,        joins); }
		public static Join InnerJoin    (this ISqlTableSource table, string alias, params Join[] joins) { return SelectQuery.InnerJoin    (table, alias, joins); }
		public static Join LeftJoin     (this ISqlTableSource table,               params Join[] joins) { return SelectQuery.LeftJoin     (table,        joins); }
		public static Join LeftJoin     (this ISqlTableSource table, string alias, params Join[] joins) { return SelectQuery.LeftJoin     (table, alias, joins); }
		public static Join Join         (this ISqlTableSource table,               params Join[] joins) { return SelectQuery.Join         (table,        joins); }
		public static Join Join         (this ISqlTableSource table, string alias, params Join[] joins) { return SelectQuery.Join         (table, alias, joins); }
		public static Join CrossApply   (this ISqlTableSource table,               params Join[] joins) { return SelectQuery.CrossApply   (table,        joins); }
		public static Join CrossApply   (this ISqlTableSource table, string alias, params Join[] joins) { return SelectQuery.CrossApply   (table, alias, joins); }
		public static Join OuterApply   (this ISqlTableSource table,               params Join[] joins) { return SelectQuery.OuterApply   (table,        joins); }
		public static Join OuterApply   (this ISqlTableSource table, string alias, params Join[] joins) { return SelectQuery.OuterApply   (table, alias, joins); }

		public static Join WeakInnerJoin(this ISqlTableSource table,               params Join[] joins) { return SelectQuery.WeakInnerJoin(table,        joins); }
		public static Join WeakInnerJoin(this ISqlTableSource table, string alias, params Join[] joins) { return SelectQuery.WeakInnerJoin(table, alias, joins); }
		public static Join WeakLeftJoin (this ISqlTableSource table,               params Join[] joins) { return SelectQuery.WeakLeftJoin (table,        joins); }
		public static Join WeakLeftJoin (this ISqlTableSource table, string alias, params Join[] joins) { return SelectQuery.WeakLeftJoin (table, alias, joins); }
		public static Join WeakJoin     (this ISqlTableSource table,               params Join[] joins) { return SelectQuery.WeakJoin     (table,        joins); }
		public static Join WeakJoin     (this ISqlTableSource table, string alias, params Join[] joins) { return SelectQuery.WeakJoin     (table, alias, joins); }
	}
}
