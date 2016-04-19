namespace LinqToDB.SqlEntities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    using LinqToDB.Mapping;
    using LinqToDB.SqlQuery.QueryElements.SqlElements;
    using LinqToDB.SqlQuery.QueryElements.SqlElements.Enums;
    using LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces;

    partial class Sql
    {
        [Serializable]
        [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
        public class TableExpressionAttribute : TableFunctionAttribute
        {
            public TableExpressionAttribute(string expression)
                : base(expression)
            {
            }

            public TableExpressionAttribute(string expression, params int[] argIndices)
                : base(expression, argIndices)
            {
            }

            public TableExpressionAttribute(string sqlProvider, string expression)
                : base(sqlProvider, expression)
            {
            }

            public TableExpressionAttribute(string sqlProvider, string expression, params int[] argIndices)
                : base(sqlProvider, expression, argIndices)
            {
            }

            protected new string Name => base.Name;

            public string Expression
            {
                get { return base.Name;  }
                set { base.Name = value; }
            }

            public override void SetTable(MappingSchema mappingSchema, ISqlTable table, MemberInfo member, IEnumerable<Expression> arguments, IEnumerable<IQueryExpression> sqlArgs)
            {
                table.SqlTableType   = ESqlTableType.Expression;
                table.Name           = Expression ?? member.Name;

                var args = ConvertArgs(member, sqlArgs.ToArray());
                for (var i = 0; i < args.Length; i++)
                {
                    table.TableArguments.AddLast(args[i]);
                }

                if (Schema   != null) table.Owner    = Schema;
                if (Database != null) table.Database = Database;
            }
        }
    }
}
