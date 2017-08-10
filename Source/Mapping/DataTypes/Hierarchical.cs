namespace Bars2Db.Mapping.DataTypes
{
    public class Hierarchical
    {
        protected Hierarchical()
        {
        }

        public Hierarchical(string content)
        {
            Content = content;
        }

        public virtual string Content { get; protected set; }

        public virtual bool IsChildOf(Hierarchical hierarchical)
        {
            return Content.StartsWith(hierarchical.Content) && Content != hierarchical.Content;
        }

        public virtual bool IsParentOf(Hierarchical hierarchical)
        {
            return hierarchical.IsChildOf(this);
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        protected bool Equals(Hierarchical other)
        {
            return string.Equals(Content, other.Content);
        }

        public override int GetHashCode()
        {
            return Content?.GetHashCode() ?? 0;
        }

        public virtual bool Contains(Hierarchical hierarchical)
        {
            return Content.Contains(hierarchical.Content);
        }

        /// <summary>Returns a string that represents the current object.</summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return Content;
        }
    }
}