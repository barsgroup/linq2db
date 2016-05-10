using System;
using System.Collections.Generic;

namespace Bars2Db.SqlQuery.QueryElements.Interfaces
{
    public interface ICloneableElement
    {
        ICloneableElement Clone(Dictionary<ICloneableElement, ICloneableElement> objectTree,
            Predicate<ICloneableElement> doClone);
    }
}