using System;
using System.Collections.Generic;

namespace Bars2Db.SqlQuery.Search.TypeGraph
{
    public class TypeVertex
    {
        public TypeVertex(Type type, int index)
        {
            Type = type;
            Index = index;
        }

        public Type Type { get; }

        public int Index { get; }

        public LinkedList<PropertyEdge> Children { get; } = new LinkedList<PropertyEdge>(); // parent == this

        public LinkedList<PropertyEdge> Parents { get; } = new LinkedList<PropertyEdge>(); // child == this

        public LinkedList<CastEdge> Casts { get; } = new LinkedList<CastEdge>();

        public override string ToString()
        {
            return $"({Index}) {Type.Name}";
        }
    }
}