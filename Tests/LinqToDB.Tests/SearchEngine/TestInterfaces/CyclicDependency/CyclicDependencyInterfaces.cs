namespace LinqToDB.Tests.SearchEngine.TestInterfaces.CyclicDependency
{
    using LinqToDB.SqlQuery.Search;

    public interface IBase
    {
    }

    public interface IA : IBase
    {
        [SearchContainer]
        IB B { get; set; }
    }

    public interface IB : IBase
    {
        [SearchContainer]
        IC C { get; set; }
    }

    public interface IC : IBase
    {
        [SearchContainer]
        ID D { get; set; }

        [SearchContainer]
        IE E { get; set; }

        [SearchContainer]
        IF F { get; set; }

        [SearchContainer]
        IBase FBase { get; set; }
    }

    public interface ID : IBase
    {
    }

    public interface IE : IBase
    {
    }

    public interface IF : IBase
    {
        [SearchContainer]
        IA A { get; set; }
    }

    public class ClassA : IA
    {
        public IB B { get; set; }
    }

    public class F : IF
    {
        public IA A { get; set; }
    }
}