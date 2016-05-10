namespace LinqToDB.Tests.SearchEngine.DelegateConstructors
{
    using LinqToDB.Tests.SearchEngine.DelegateConstructors.Base;
    using LinqToDB.Tests.SearchEngine.TestInterfaces.CastFromBase;

    using Xunit;

    public class CastFromBaseDelegateConstructorTest : BaseDelegateConstructorTest<IBase>
    {
        [Fact]
        public void Test()
        {
            var testObj = SetupTestObject();

            Assert.True(CompareWithReflectionSearcher<IA>(testObj));
            Assert.True(CompareWithReflectionSearcher<IB>(testObj));
            Assert.True(CompareWithReflectionSearcher<IC>(testObj));
            Assert.True(CompareWithReflectionSearcher<ID>(testObj));
        }

        protected IBase SetupTestObject()
        {
            var obj = new A
                          {
                              Base = new A
                                      {
                                          Base = new C
                                                  {
                                                      Base = new B
                                                              {
                                                                  Base = new C
                                                                          {
                                                                              Base = new A
                                                                                      {
                                                                                          Base = new D()
                                                                                      }
                                                                          }
                                                              }
                                                  }
                                      }
                          };

            return obj;
        }
    }
}