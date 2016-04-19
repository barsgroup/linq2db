namespace LinqToDB.SqlEntities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    using LinqToDB.Extensions;
    using LinqToDB.Mapping;
    using LinqToDB.SqlQuery.QueryElements.SqlElements;
    using LinqToDB.SqlQuery.QueryElements.SqlElements.Enums;
    using LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces;

    partial class Sql
    {
        [Serializable]
        [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
        public class TableFunctionAttribute : Attribute
        {
            public TableFunctionAttribute()
            {
            }

            public TableFunctionAttribute(string name)
            {
                Name = name;
            }

            public TableFunctionAttribute(string name, params int[] argIndices)
            {
                Name        = name;
                ArgIndices  = argIndices;
            }

            public TableFunctionAttribute(string configuration, string name)
            {
                Configuration = configuration;
                Name          = name;
            }

            public TableFunctionAttribute(string configuration, string name, params int[] argIndices)
            {
                Configuration = configuration;
                Name          = name;
                ArgIndices    = argIndices;
            }

            public string Configuration { get; set; }
            public string Name          { get; set; }
            public string Schema        { get; set; }
            public string Database      { get; set; }
            public int[]  ArgIndices    { get; set; }

            protected IQueryExpression[] ConvertArgs(MemberInfo member, IQueryExpression[] args)
            {
                var methodInfo = member as MethodInfo;
                if (methodInfo != null)
                {
                    if (methodInfo.DeclaringType.IsGenericTypeEx())
                        args = args.Concat(methodInfo.DeclaringType.GetGenericArgumentsEx().Select(t => (IQueryExpression)SqlDataType.GetDataType(t))).ToArray();

                    if (methodInfo.IsGenericMethod)
                        args = args.Concat(methodInfo.GetGenericArguments().Select(t => (IQueryExpression)SqlDataType.GetDataType(t))).ToArray();
                }

                if (ArgIndices != null)
                {
                    var idxs = new IQueryExpression[ArgIndices.Length];

                    for (var i = 0; i < ArgIndices.Length; i++)
                        idxs[i] = args[ArgIndices[i]];

                    return idxs;
                }

                return args;
            }

            public virtual void SetTable(MappingSchema mappingSchema, ISqlTable table, MemberInfo member, IEnumerable<Expression> arguments, IEnumerable<IQueryExpression> sqlArgs)
            {
                table.SqlTableType   = ESqlTableType.Function;
                table.Name           = Name ?? member.Name;
                table.PhysicalName   = Name ?? member.Name;

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
