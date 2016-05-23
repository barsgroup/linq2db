using Bars2Db.Linq.Joiner.Visitors.Entities;

namespace Bars2Db.Linq.Joiner.Graph
{
    public class ReferenceGraphVertex
    {
        private readonly FullPathInfo _fullPathInfo;

        public FullPathInfo FullPathInfo
        {
            get { return _fullPathInfo; }
        }

        public ReferenceGraphVertex(FullPathInfo fullPathInfo)
        {
            _fullPathInfo = fullPathInfo;
        }

        /// <summary>Determines whether the specified <see cref="T:System.Object" /> is equal to the current
        ///     <see cref="T:System.Object" />.</summary>
        /// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
        /// <param name="obj">The object to compare with the current object. </param>
        public override bool Equals(object obj)
        {
            var comparedObject = (ReferenceGraphVertex)obj;
            return _fullPathInfo.Equals(comparedObject.FullPathInfo);
        }

        /// <summary>Serves as a hash function for a particular type.</summary>
        /// <returns>A hash code for the current <see cref="T:System.Object" />.</returns>
        public override int GetHashCode()
        {
            return FullPathInfo.GetHashCode();
        }

        /// <summary>Returns a string that represents the current object.</summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return _fullPathInfo.ToString();
        }
    }
}