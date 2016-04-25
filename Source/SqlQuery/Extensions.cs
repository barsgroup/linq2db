namespace LinqToDB.SqlQuery
{
    using LinqToDB.SqlQuery.QueryElements;
    using LinqToDB.SqlQuery.QueryElements.Conditions.Interfaces;
    using LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces;

    public static class Extensions
    {
        public static IJoin InnerJoin    (this ISqlTableSource table,               params IJoin[] joins) { return SelectQuery.InnerJoin    (table,        joins); }
        public static IJoin CrossApply   (this ISqlTableSource table,               params IJoin[] joins) { return SelectQuery.CrossApply   (table,        joins); }

        public static IJoin WeakInnerJoin(this ISqlTableSource table,               params IJoin[] joins) { return SelectQuery.WeakInnerJoin(table,        joins); }
        public static IJoin WeakLeftJoin (this ISqlTableSource table,               params IJoin[] joins) { return SelectQuery.WeakLeftJoin (table,        joins); }
    }
}
