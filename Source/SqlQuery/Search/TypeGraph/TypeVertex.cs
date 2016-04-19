namespace LinqToDB.SqlQuery.Search.TypeGraph
{
    using System;
    using System.Collections.Generic;

    public class TypeVertex
    {
        public Type Type { get; }

        public int Index { get; }

        public LinkedList<PropertyEdge> Children { get; } = new LinkedList<PropertyEdge>(); // parent == this

        public LinkedList<PropertyEdge> Parents { get; } = new LinkedList<PropertyEdge>(); // child == this

        public LinkedList<CastEdge> Casts { get; } = new LinkedList<CastEdge>();

        public TypeVertex(Type type, int index)
        {
            Type = type;
            Index = index;
        }

        public override string ToString()
        {
            return $"({Index}) {Type.Name}";
        }
    }
}