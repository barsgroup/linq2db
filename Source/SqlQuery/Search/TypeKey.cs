using System;

namespace Bars2Db.SqlQuery.Search
{
    internal struct TypeKey : IEquatable<TypeKey>
    {
        private readonly Type _strategyType;

        private readonly Type _from;

        private readonly Type _to;

        public TypeKey(Type strategyType, Type from, Type to)
        {
            _strategyType = strategyType;
            _from = from;
            _to = to;
        }

        public bool Equals(TypeKey other)
        {
            return _strategyType == other._strategyType && _from == other._from && _to == other._to;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            return obj is TypeKey && Equals((TypeKey) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = _strategyType?.GetHashCode() ?? 0;
                hashCode = (hashCode*397) ^ (_from?.GetHashCode() ?? 0);
                hashCode = (hashCode*397) ^ (_to?.GetHashCode() ?? 0);
                return hashCode;
            }
        }
    }
}