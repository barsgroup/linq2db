using System;
using Bars2Db.SqlQuery.QueryElements.Interfaces;

namespace Bars2Db.SqlQuery.QueryElements.SqlElements.Interfaces
{
    public interface ISqlParameter : IQueryExpression,
        IValueContainer
    {
        string Name { get; set; }

        bool IsQueryParameter { get; set; }

        DataType DataType { get; set; }

        int DbSize { get; }

        string LikeStart { get; set; }

        bool ReplaceLike { get; set; }

        Func<object, object> ValueConverter { set; }

        void SetTakeConverter(int take);
    }
}