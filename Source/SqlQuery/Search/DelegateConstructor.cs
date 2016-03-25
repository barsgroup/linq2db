namespace LinqToDB.SqlQuery.Search
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;

    using System.Diagnostics.SymbolStore;
    using System.Diagnostics;

    using LinqToDB.Extensions;

    using Seterlund.CodeGuard;

    using Sigil;

    public class DelegateConstructor<TSearch>
    {
        private static readonly Delegate isInstanseOf = new Func<object, Type, object>(IsInstanseOf);

        public delegate void ResultDelegate(object obj, out LinkedList<TSearch> resultList);

        public ResultDelegate CreateResultDelegate(LinkedList<CompositPropertyVertex> vertices)
        {
            var delegateMap = new Dictionary<CompositPropertyVertex, DynamicMethod>();

            return CreateDelegate(vertices.First.Value, delegateMap);
        }

        private ResultDelegate CreateDelegate(CompositPropertyVertex vertex,
                                    Dictionary<CompositPropertyVertex, DynamicMethod> delegateMap)
        {
            Guard.That(vertex.PropertyList.First).IsNotNull();

            var method = Emit<ResultDelegate>.NewDynamicMethod(doVerify:false);
            var dynMethod = (DynamicMethod)method.GetType().GetPropertiesEx().Single(p =>p.Name == "DynMethod").GetValue(method);
            delegateMap[vertex] = dynMethod;

            method.LoadArgument(1);
            method.NewObject<LinkedList<TSearch>>();
            method.StoreIndirect<LinkedList<TSearch>>();

            method.LoadArgument(0);

            vertex.PropertyList.ForEach(
                node =>
                {
                    method.LoadConstant(node.Value.DeclaringType);
                    method.Call(isInstanseOf.Method);

                    method.DefineLabel("isInstance");
                    method.DefineLabel("endIsInstanse");

                    method.BranchIfTrue("isInstance");
                    method.LoadNull();

                    method.Branch("endIsInstanse");

                    method.MarkLabel("isInstance");

                    method.LoadArgument(0);
                    method.LoadNull();
                    method.Call(node.Value.GetGetMethod());

                    method.MarkLabel("endIsInstanse");
                });

            if (vertex.Children.First == null)
            {
                method.Pop();
            }

            vertex.Children.ForEach(
                node =>
                {
                    DynamicMethod childDynMethod;
                    if (delegateMap.TryGetValue(vertex, out childDynMethod))
                    {
                        method.Call(childDynMethod);
                    }
                    else
                    {
                        var deleg = CreateDelegate(node.Value, delegateMap);
                        method.Call(deleg.Method);
                    }
                });

            method.Return();

            string instructions;
            return method.CreateDelegate(out instructions, OptimizationOptions.None);
        }

        public static object IsInstanseOf(object obj, Type declaringType)
        {
            Guard.That(obj).IsNotNull();
            Guard.That(declaringType).IsNotNull();

            return declaringType.IsInstanceOfType(obj);
        }
    }
}