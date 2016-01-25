namespace LinqToDB.SqlQuery.QueryElements.Interfaces
{
    using System.Collections.Generic;

    public interface IBaseQueryElement
    {
        IEnumerable<IQueryElement> GetChildItems();
    }
}