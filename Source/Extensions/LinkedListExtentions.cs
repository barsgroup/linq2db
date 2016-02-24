namespace LinqToDB.Extensions
{
    using System;
    using System.Collections.Generic;

    public static class LinkedListExtentions
    {
        public static void ForEach<TElement>(this LinkedList<TElement> linkedList, Action<LinkedListNode<TElement>> action)
        {
            var current = linkedList.First;
            while (current != null)
            {
                var next = current.Next;

                action(current);

                current = next;
            }
        }

        public static void AddRange<TElement>(this LinkedList<TElement> sourceList, LinkedList<TElement> targetList)
        {
            targetList.ForEach(node => sourceList.AddLast(node.Value));
        }

        public static void AddRange<TElement>(this LinkedList<TElement> sourceList, List<TElement> targetList)
        {
            for (int i = 0; i < targetList.Count; i++)
            {
                sourceList.AddLast(targetList[i]);
            }
        }

        public static TResult FindOnce<TElement, TResult>(this LinkedList<TElement> linkedList, Func<LinkedListNode<TElement>, TResult> action)
        {
            var current = linkedList.First;
            while (current != null)
            {
                var next = current.Next;
                var result = action(current);
                if (!Equals(result, default(TResult)))
                {
                    return result;
                }

                current = next;
            }

            return default(TResult);
        }

        public static void ReverseEach<TElement>(this LinkedListNode<TElement> node, Action<LinkedListNode<TElement>> action)
        {
            var current = node;
            while (current != null)
            {
                var previous = current.Previous;

                action(current);

                current = previous;
            }
        }
    }
}