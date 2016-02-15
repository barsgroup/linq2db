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
                action(current);

                current = current.Next;
            }
        }

        public static void ReverseEach<TElement>(this LinkedListNode<TElement> node, Action<LinkedListNode<TElement>> action)
        {
            var current = node.Previous;
            while (current != null)
            {
                action(current);

                current = current.Previous;
            }
        }
    }
}