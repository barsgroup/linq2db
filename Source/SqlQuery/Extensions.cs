using Bars2Db.SqlQuery.QueryElements;
using Bars2Db.SqlQuery.QueryElements.Conditions.Interfaces;
using Bars2Db.SqlQuery.QueryElements.SqlElements.Interfaces;

namespace Bars2Db.SqlQuery
{
    public static class Extensions
    {
        public static IJoin InnerJoin(this ISqlTableSource table, params IJoin[] joins)
        {
            return SelectQuery.InnerJoin(table, joins);
        }

        public static IJoin CrossApply(this ISqlTableSource table, params IJoin[] joins)
        {
            return SelectQuery.CrossApply(table, joins);
        }

        public static IJoin WeakInnerJoin(this ISqlTableSource table, params IJoin[] joins)
        {
            return SelectQuery.WeakInnerJoin(table, joins);
        }

        public static IJoin WeakLeftJoin(this ISqlTableSource table, params IJoin[] joins)
        {
            return SelectQuery.WeakLeftJoin(table, joins);
        }
    }
}