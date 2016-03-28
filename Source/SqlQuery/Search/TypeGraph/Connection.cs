namespace LinqToDB.SqlQuery.Search.TypeGraph
{
    using System;

    public struct Connection : IEquatable<Connection>
    {
        public Connection(ConnectionType startType, ConnectionType endType)
        {
            if ((startType == ConnectionType.None && endType != ConnectionType.None) || 
                (startType != ConnectionType.None && endType == ConnectionType.None))
            {
                throw new ArgumentException("start type and end type mismatch");
            }

            StartType = startType;
            EndType = endType;
        }

        public ConnectionType StartType { get; }

        public ConnectionType EndType { get; }

        public bool IsEmpty()
        {
            return StartType == ConnectionType.None && EndType == ConnectionType.None;
        }
        
        public Connection Union(ConnectionType startType, ConnectionType endType)
        {
            return new Connection(StartType | startType, EndType | endType);
        }

        public bool StartsWith(ConnectionType type)
        {
            return StartType.HasFlag(type);
        }

        public bool EndsWith(ConnectionType type)
        {
            return EndType.HasFlag(type);
        }

        public bool Equals(Connection other)
        {
            return StartType == other.StartType && EndType == other.EndType;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            return obj is Connection && Equals((Connection)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((int)StartType * 397) ^ (int)EndType;
            }
        }

        public override string ToString()
        {
            return GetString(StartType) + GetString(EndType);
        }

        private static string GetString(ConnectionType type)
        {
            switch (type)
            {
                case ConnectionType.None:
                    return "";
                case ConnectionType.Property:
                    return "P";
                case ConnectionType.Cast:
                    return "C";
                default:
                    return "*";
            }
        }
    }
}