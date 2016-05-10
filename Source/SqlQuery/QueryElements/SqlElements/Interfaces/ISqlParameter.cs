namespace LinqToDB.SqlQuery.QueryElements.SqlElements.Interfaces
{
    using System;

    using LinqToDB.SqlQuery.QueryElements.Interfaces;

    public interface ISqlParameter : IQueryExpression,
                                     IValueContainer
    {
        string Name { get; set; }

        bool IsQueryParameter { get; set; }

        DataType DataType { get; set; }

        int DbSize { get;  }

        string LikeStart { get; set; }

        bool ReplaceLike { get; set; }

        void SetTakeConverter(int take);

        Func<object, object> ValueConverter { set; }
    }
}