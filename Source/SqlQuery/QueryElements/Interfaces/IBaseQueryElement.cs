namespace LinqToDB.SqlQuery.QueryElements.Interfaces
{
    using System.Collections.Generic;

    public interface IBaseQueryElement
    {
        IEnumerable<TElementType> DeepFindParentFirst<TElementType>() where TElementType : class, IQueryElement;

        IEnumerable<TElementType> DeepFindParentLast<TElementType>() where TElementType : class, IQueryElement;

        IEnumerable<TElementType> DeepFindDownTo<TElementType>() where TElementType : class, IQueryElement;

        void GetChildren(LinkedList<IQueryElement> list);
    }
}