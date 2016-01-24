using System;
using System.Collections.Generic;
using System.Text;

namespace LinqToDB.SqlQuery
{
	public interface IBaseQueryElement
	{
		IEnumerable<IQueryElement> GetChildItems();
	}

	public interface IQueryElement : IBaseQueryElement//: ICloneableElement
	{
		QueryElementType ElementType { get; }
		StringBuilder    ToString (StringBuilder sb, Dictionary<IQueryElement,IQueryElement> dic);
	}
}
