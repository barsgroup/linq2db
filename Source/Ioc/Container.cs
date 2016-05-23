using Bars2Db.Linq.Joiner;
using Bars2Db.Linq.Joiner.Interfaces;
using Bars2Db.Linq.Joiner.Visitors;
using DryIoc;

namespace Bars2Db.Ioc
{
    public static class Container
    {
        private static DryIoc.Container Current { get; } = new DryIoc.Container();

        static Container()
        {
            RegisterJoiner();
        }

        private static void RegisterJoiner()
        {
            Current.Register<IFullPathVisitor, FullPathVisitor>(Reuse.Singleton);
            Current.Register<IJoinService, JoinService>(Reuse.Singleton);
        }
    }
}