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

    public class A : IA
    {
        public IB B { get; set; }
    }

    public class B : IB
    {
        public IC C { get; set; }
    }

    public class C : IC
    {
        public ID D { get; set; }

        public IE E { get; set; }

        public IF F { get; set; }

        public IBase FBase { get; set; }
    }

    public class D : ID
    {
    }

    public class E : IE
    {
    }

    public class F : IF
    {
        public IA A { get; set; }
    }
}