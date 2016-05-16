using System.Collections.Generic;
using System.Text;
using Bars2Db.SqlQuery.QueryElements.Enums;

namespace Bars2Db.SqlQuery.QueryElements.Interfaces
{
    public interface IQueryElement //: ICloneableElement
    {
        EQueryElementType ElementType { get; }

        string SqlText { get; }

        StringBuilder ToString(StringBuilder sb, Dictionary<IQueryElement, IQueryElement> dic);
    }
}