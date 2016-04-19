namespace LinqToDB.SqlQuery.Search.TypeGraph
{
    using System;

    public class CastEdge : IEquatable<CastEdge>
    {
        public TypeVertex CastFrom { get; }

        public TypeVertex CastTo { get; }

        public CastEdge(TypeVertex castFrom, TypeVertex castTo)
        {
            CastFrom = castFrom;
            CastTo = castTo;
        }

        public virtual bool Equals(CastEdge other)
        {
            return CastFrom.Type == other.CastFrom.Type && CastTo.Type == other.CastTo.Type;
        }

        public override bool Equals(object obj)
        {
            var edge = obj as CastEdge;

            return edge != null && Equals(edge);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = CastFrom?.Type.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ (CastTo?.Type.GetHashCode() ?? 0);
                return hashCode;
            }
        }

        public override string ToString()
        {
            return $"{CastFrom.Type.Name} ~> {CastTo.Type.Name}";
        }
    }
}