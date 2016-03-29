namespace LinqToDB.Tests.SearchEngine.TestInterfaces.CastFromBase
{
    using LinqToDB.SqlQuery.Search;

    public interface IBase
    {
    }

    public interface IA : IBase
    {
        [SearchContainer]
        IBase Base { get; set; }
    }

    public interface IB : IBase
    {
        [SearchContainer]
        IBase Base { get; set; }
    }

    public interface IC : IBase
    {
        [SearchContainer]
        IBase Base { get; set; }
    }

    public interface ID : IBase
    {
    }

    public class A : IA
    {
        public IBase Base { get; set; }
    }

    public class B : IB
    {
        public IBase Base { get; set; }
    }

    public class C : IC
    {
        public IBase Base { get; set; }
    }

    public class D : ID
    {
    }
}