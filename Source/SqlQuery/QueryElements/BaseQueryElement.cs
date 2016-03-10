namespace LinqToDB.SqlQuery.QueryElements
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Text;

    using LinqToDB.Extensions;
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

        protected void FillList<TElement>(List<TElement> items, LinkedList<IQueryElement> list) where TElement : IQueryElement
        {
            for (int i = 0; i < items.Count; i++)
            {
                list.AddLast(items[i]);
            }
        }

        public LinkedList<TElementType> DeepFindParentLastOnce<TElementType>() where TElementType : class, IQueryElement
        {
            //var returnList = new LinkedList<TElementType>();
            //var list = new LinkedList<IQueryElement>();

            //var visitedItems = new HashSet<IQueryElement>();

            //list.AddFirst(this);

            //while (list.First != null)
            //{
            //    var current = list.First;
            //    var value = current.Value as TElementType;

            //    if (!visitedItems.Contains(value))
            //    {
            //        current.Value?.GetChildren(list);

            //        if (value != null)
            //        {
            //            visitedItems.Add(value);

            //            returnList.AddFirst(value);
            //        }
            //    }

            //    list.RemoveFirst();
            //}

            //return returnList;

            return null;
        }

        public LinkedList<TElementType> DeepFindParentFirst<TElementType>() where TElementType: class, IQueryElement
        {
            //var returnList = new LinkedList<TElementType>();
            //var processed = new LinkedList<IQueryElement>();

            //processed.AddFirst(this);

            //while (processed.First != null)
            //{
            //    var current = processed.Last;
            //    var value = current.Value as TElementType;
            //    if (value != null)
            //    {
            //        returnList.AddLast(value);
            //    }

            //    processed.RemoveLast();

            //    current.Value?.GetChildren(processed);
            //}

            //return returnList;
            return null;
        }

        public LinkedList<TElementType> DeepFindDownTo<TElementType>() where TElementType : class, IQueryElement
        {
            //var returnList = new LinkedList<TElementType>();
            //var list = new LinkedList<IQueryElement>();

            //var visitedItems = new HashSet<IQueryElement>();

            //list.AddFirst(this);
            //while (list.First != null)
            //{
            //    var current = list.First;

            //    if (current.Value != null)
            //    {
            //        var value = current.Value as TElementType;

            //        if (list.Contains(current.Value))
            //        {
            //            visitedItems.Add(current.Value);

            //            if (value != null && value != this)
            //            {
            //                returnList.AddLast(value);
            //            }
            //            else
            //            {
            //                current.Value.GetChildren(list);
            //            }
            //        }
            //    }

            //    list.RemoveFirst();
            //}

            //return returnList;


            return null;
        }

        //public abstract void GetChildren(LinkedList<IQueryElement> list);

        public abstract EQueryElementType ElementType { get; }

        public abstract StringBuilder ToString(StringBuilder sb, Dictionary<IQueryElement, IQueryElement> dic);

        public string SqlText => ToString(new StringBuilder(), new Dictionary<IQueryElement, IQueryElement>()).ToString();
    }
}