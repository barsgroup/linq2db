namespace LinqToDB.SqlQuery
{
    using LinqToDB.SqlQuery.QueryElements;
    using LinqToDB.SqlQuery.QueryElements.Conditions;
    using LinqToDB.SqlQuery.QueryElements.Conditions.Interfaces;
    using LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces;

    public static class Extensions
    {
        public static IJoin InnerJoin    (this ISqlTableSource table,               params IJoin[] joins) { return SelectQuery.InnerJoin    (table,        joins); }
        public static IJoin InnerJoin    (this ISqlTableSource table, string alias, params IJoin[] joins) { return SelectQuery.InnerJoin    (table, alias, joins); }
        public static IJoin LeftJoin     (this ISqlTableSource table,               params IJoin[] joins) { return SelectQuery.LeftJoin     (table,        joins); }
        public static IJoin LeftJoin     (this ISqlTableSource table, string alias, params IJoin[] joins) { return SelectQuery.LeftJoin     (table, alias, joins); }
        public static IJoin Join         (this ISqlTableSource table,               params IJoin[] joins) { return SelectQuery.Join         (table,        joins); }
        public static IJoin Join         (this ISqlTableSource table, string alias, params IJoin[] joins) { return SelectQuery.Join         (table, alias, joins); }
        public static IJoin CrossApply   (this ISqlTableSource table,               params IJoin[] joins) { return SelectQuery.CrossApply   (table,        joins); }
        public static IJoin CrossApply   (this ISqlTableSource table, string alias, params IJoin[] joins) { return SelectQuery.CrossApply   (table, alias, joins); }
        public static IJoin OuterApply   (this ISqlTableSource table,               params IJoin[] joins) { return SelectQuery.OuterApply   (table,        joins); }
        public static IJoin OuterApply   (this ISqlTableSource table, string alias, params IJoin[] joins) { return SelectQuery.OuterApply   (table, alias, joins); }

        public static IJoin WeakInnerJoin(this ISqlTableSource table,               params IJoin[] joins) { return SelectQuery.WeakInnerJoin(table,        joins); }
        public static IJoin WeakInnerJoin(this ISqlTableSource table, string alias, params IJoin[] joins) { return SelectQuery.WeakInnerJoin(table, alias, joins); }
        public static IJoin WeakLeftJoin (this ISqlTableSource table,               params IJoin[] joins) { return SelectQuery.WeakLeftJoin (table,        joins); }
        public static IJoin WeakLeftJoin (this ISqlTableSource table, string alias, params IJoin[] joins) { return SelectQuery.WeakLeftJoin (table, alias, joins); }
        public static IJoin WeakJoin     (this ISqlTableSource table,               params IJoin[] joins) { return SelectQuery.WeakJoin     (table,        joins); }
        public static IJoin WeakJoin     (this ISqlTableSource table, string alias, params IJoin[] joins) { return SelectQuery.WeakJoin     (table, alias, joins); }
    }
}
