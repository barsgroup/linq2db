namespace LinqToDB.SqlQuery.QueryElements.Interfaces
{
    using System;
    using System.Collections.Generic;

    public interface ICloneableElement
    {
        ICloneableElement Clone(Dictionary<ICloneableElement,ICloneableElement> objectTree, Predicate<ICloneableElement> doClone);
    }
}
