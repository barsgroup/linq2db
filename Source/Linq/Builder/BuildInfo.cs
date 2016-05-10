using System.Linq.Expressions;
using Bars2Db.SqlQuery.QueryElements.Interfaces;

namespace Bars2Db.Linq.Builder
{
    public class BuildInfo
    {
        private bool _isAssociationBuilt;

        public BuildInfo(IBuildContext parent, Expression expression, ISelectQuery selectQuery)
        {
            Parent = parent;
            Expression = expression;
            SelectQuery = selectQuery;
        }

        public BuildInfo(BuildInfo buildInfo, Expression expression)
            : this(buildInfo.Parent, expression, buildInfo.SelectQuery)
        {
            SequenceInfo = buildInfo;
            CreateSubQuery = buildInfo.CreateSubQuery;
        }

        public BuildInfo(BuildInfo buildInfo, Expression expression, ISelectQuery selectQuery)
            : this(buildInfo.Parent, expression, selectQuery)
        {
            SequenceInfo = buildInfo;
            CreateSubQuery = buildInfo.CreateSubQuery;
        }

        public BuildInfo SequenceInfo { get; set; }
        public IBuildContext Parent { get; set; }
        public Expression Expression { get; set; }
        public ISelectQuery SelectQuery { get; set; }
        public bool CopyTable { get; set; }
        public bool CreateSubQuery { get; set; }

        public bool IsSubQuery => Parent != null;

        public bool IsAssociationBuilt
        {
            get { return _isAssociationBuilt; }
            set
            {
                _isAssociationBuilt = value;

                if (SequenceInfo != null)
                    SequenceInfo.IsAssociationBuilt = value;
            }
        }
    }
}