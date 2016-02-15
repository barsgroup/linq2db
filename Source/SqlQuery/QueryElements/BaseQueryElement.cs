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

                current.Value?.GetChildren(list);

                if (value != null)
                {
                    yield return value;
                }

                list.RemoveFirst();

            }
        }

        public IEnumerable<TElementType> DeepFindParentFirst<TElementType>() where TElementType: class, IQueryElement
        {
            var processed = new LinkedList<IQueryElement>();

            processed.AddFirst(this);

            while (processed.First != null)
            {
                var current = processed.Last;
                var value = current.Value as TElementType;
                if (value != null)
                {
                    yield return value;
                }

                processed.RemoveLast();

                current.Value?.GetChildren(processed);
            }
        }

        public IEnumerable<TElementType> DeepFindDownTo<TElementType>() where TElementType : class, IQueryElement
        {
            var list = new LinkedList<IQueryElement>();

            list.AddFirst(this);

            while (list.First != null)
            {
                var current = list.First;

                if (current.Value != null)
                {
                    var value = current.Value as TElementType;

                    if (value != this && value != null)
                    {
                        yield return value;
                    }
                    else
                    {
                        current.Value.GetChildren(list);
                    }
                }

                list.RemoveFirst();
            }
        }

        public abstract void GetChildren(LinkedList<IQueryElement> list);

        public abstract EQueryElementType ElementType { get; }

        public abstract StringBuilder ToString(StringBuilder sb, Dictionary<IQueryElement, IQueryElement> dic);

        public string SqlText => ToString(new StringBuilder(), new Dictionary<IQueryElement, IQueryElement>()).ToString();
    }
}