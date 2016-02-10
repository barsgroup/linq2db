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
        public IEnumerable<IQueryElement> GetSelfWithChildren()
        {
            var list = new List<IQueryElement>();

            list.Add(this);

            while (list.Count != 0)
            {
                var current = list[list.Count - 1];
                if (current != null)
                {
                    yield return current;
                }

                list.RemoveAt(list.Count - 1);

                ((BaseQueryElement)current)?.GetChildrenInternal(list);
            }
        }

        protected abstract void GetChildrenInternal(List<IQueryElement> list);

        public abstract EQueryElementType ElementType { get; }

        public abstract StringBuilder ToString(StringBuilder sb, Dictionary<IQueryElement, IQueryElement> dic);

        public string SqlText => ToString(new StringBuilder(), new Dictionary<IQueryElement, IQueryElement>()).ToString();
    }
}