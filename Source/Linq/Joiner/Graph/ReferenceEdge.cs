using System.Reflection;
using QuickGraph;

namespace Bars2Db.Linq.Joiner.Graph
{
    public class ReferenceEdge : TaggedEdge<ReferenceGraphVertex, PropertyInfo>
    {
        /// <ensures>Object.Equals(this.Tag,tag)</ensures>
        public ReferenceEdge(ReferenceGraphVertex referenceGraphVertex, ReferenceGraphVertex target, PropertyInfo tag) : base(referenceGraphVertex, target, tag)
        {
        }

        /// <summary>Determines whether the specified <see cref="T:System.Object" /> is equal to the current
        ///     <see cref="T:System.Object" />.</summary>
        /// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
        /// <param name="obj">The object to compare with the current object. </param>
        public override bool Equals(object obj)
        {
            // Проверить правильность реализации
            var comparedObject = (ReferenceEdge)obj;

            return comparedObject.Source.Equals(Source) && comparedObject.Target.Equals(Target) && comparedObject.Tag.Equals(Tag);
        }

        /// <summary>
        /// Serves as a hash function for a particular type.
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"/>.
        /// </returns>
        public override int GetHashCode()
        {
            return Source.GetHashCode() * Target.GetHashCode() * Tag.GetHashCode();
        }
    }
}