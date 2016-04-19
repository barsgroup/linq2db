namespace LinqToDB.SqlQuery.Search
{
    using System;

    internal struct TypeKey : IEquatable<TypeKey>
    {
        private readonly Type _from;

        private readonly Type _to;

        public TypeKey(Type from, Type to)
        {
            _from = from;
            _to = to;
        }

        public bool Equals(TypeKey other)
        {
            return _from == other._from && _to == other._to;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            return obj is TypeKey && Equals((TypeKey)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((_from?.GetHashCode() ?? 0) * 397) ^ (_to?.GetHashCode() ?? 0);
            }
        }
    }
}