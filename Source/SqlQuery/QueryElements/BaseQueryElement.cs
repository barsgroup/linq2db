namespace LinqToDB.SqlQuery.QueryElements
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Text;

    using LinqToDB.SqlQuery.QueryElements.Enums;
    using LinqToDB.SqlQuery.QueryElements.Interfaces;

    [DebuggerDisplay("SQL = {SqlText}")]
    public abstract class BaseQueryElement : IQueryElement
    {
        protected void FillList<TElement>(IEnumerable<TElement> items, LinkedList<IQueryElement> list) where TElement: IQueryElement
        {
            foreach (var item in items)
            {
                list.AddLast(item);
            }
        }

        public IEnumerable<TElementType> DeepFindParentLast<TElementType>() where TElementType : class, IQueryElement
        {
            var list = new LinkedList<IQueryElement>();

            list.AddFirst(this);

            while (list.First != null)
            {
                var current = list.First;
                var value = current.Value as TElementType;

                value?.GetChildren(list);

                if (value != null)
                {
                    yield return value;
                }

                list.RemoveFirst();

            }
        }

        public IEnumerable<TElementType> DeepFindParentFirst<TElementType>() where TElementType: class, IQueryElement
        {
            var list = new LinkedList<IQueryElement>();

            list.AddFirst(this);

            while (list.First != null)
            {
                var value = list.Last.Value as TElementType;
                if (value != null)
                {
                    yield return value;
                }

                list.RemoveLast();

                value?.GetChildren(list);
            }
        }

        public IEnumerable<TElementType> DeepFindDownTo<TElementType>() where TElementType : class, IQueryElement
        {
            var list = new LinkedList<IQueryElement>();

            list.AddFirst(this);

            while (list.First != null)
            {
                var current = list.Last;

                var value = current.Value as TElementType;
                if (value != null)
                {
                    yield return value;
                }

                list.RemoveLast();

                if (value == null)
                {
                    current.Value.GetChildren(list);
                }
            }
        }

        public abstract void GetChildren(LinkedList<IQueryElement> list);

        public abstract EQueryElementType ElementType { get; }

        public abstract StringBuilder ToString(StringBuilder sb, Dictionary<IQueryElement, IQueryElement> dic);

        public string SqlText => ToString(new StringBuilder(), new Dictionary<IQueryElement, IQueryElement>()).ToString();
    }
}