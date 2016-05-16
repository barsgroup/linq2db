namespace Bars2Db.Common
{
    internal class Option<T>
    {
        public static Option<T> None = new Option<T>(default(T));
        public readonly T Value;

        public Option(T value)
        {
            Value = value;
        }

        public bool IsNone => this == None;

        public bool IsSome => this != None;

        public static Option<T> Some(T value)
        {
            return new Option<T>(value);
        }
    }
}