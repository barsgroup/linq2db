using LinqToDB.Tests.SearchEngine.DelegateConstructors.Base;
using LinqToDB.Tests.SearchEngine.TestInterfaces.CastFromBase;
using Xunit;

namespace LinqToDB.Tests.SearchEngine.DelegateConstructors
{
    public class CastFromBaseDelegateConstructorTest : BaseDelegateConstructorTest<IBase>
    {
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

        [Fact]
        public void Test()
        {
            var testObj = SetupTestObject();

            Assert.True(CompareWithReflectionSearcher<IA>(testObj));
            Assert.True(CompareWithReflectionSearcher<IB>(testObj));
            Assert.True(CompareWithReflectionSearcher<IC>(testObj));
            Assert.True(CompareWithReflectionSearcher<ID>(testObj));
        }
    }
}