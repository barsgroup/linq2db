namespace LinqToDB.Common
{
	class Option<T>
	{
		public readonly T Value;

		public Option(T value)
		{
			Value = value;
		}

		public bool IsNone => this == None;

	    public bool IsSome => this != None;

	    static public Option<T> Some(T value)
		{
			return new Option<T>(value);
		}

		static public Option<T> None = new Option<T>(default(T));
	}
}
