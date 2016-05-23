using QuickGraph;

namespace Bars2Db.Linq.Joiner.Graph
{
    public class ReferenceGraph : AdjacencyGraph<ReferenceGraphVertex, ReferenceEdge>
    {
        public ReferenceGraph() : base(true)
        {
        }
    }
}