namespace LinqToDB.SqlQuery.QueryElements.Interfaces
{
    using System.Collections.Generic;
    using System.Text;

    public interface IQueryElement : IBaseQueryElement//: ICloneableElement
    {
        QueryElementType ElementType { get; }
        StringBuilder    ToString (StringBuilder sb, Dictionary<IQueryElement,IQueryElement> dic);
    }
}
