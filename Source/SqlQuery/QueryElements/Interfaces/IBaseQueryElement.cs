namespace LinqToDB.SqlQuery.QueryElements.Interfaces
{
    using System.Collections.Generic;

    public interface IBaseQueryElement
    {
        LinkedList<TElementType> DeepFindParentFirst<TElementType>() where TElementType : class, IQueryElement;

        LinkedList<TElementType> DeepFindParentLast<TElementType>() where TElementType : class, IQueryElement;

        LinkedList<TElementType> DeepFindDownTo<TElementType>() where TElementType : class, IQueryElement;

        void GetChildren(LinkedList<IQueryElement> list);
    }
}