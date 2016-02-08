namespace LinqToDB.Common
{
    using LinqToDB.Properties;

    public static class Array<T>
	{
		[NotNull]
		public static readonly T[] Empty = new T[0];
	}
}
