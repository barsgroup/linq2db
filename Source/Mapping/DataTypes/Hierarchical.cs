using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqToDB.Mapping.DataTypes
{
    public class Hierarchical
    {
        public virtual string Content { get; protected set; }

        protected Hierarchical()
        {
        }
        public Hierarchical(string content)
        {
            Content = content;
        }

        public virtual bool IsChildOf(Hierarchical content)
        {
            return false;
        }

        public virtual bool IsParentOf(Hierarchical content)
        {
            return false;
        }

        public virtual bool Contains(Hierarchical content)
        {
            return false;
        }

        /// <summary>Returns a string that represents the current object.</summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return Content;
        }
    }
}
