using System.Linq;
using Bars2Db.IntegrationTests.Data;
using Bars2Db.IntegrationTests.Data.Entities;
using Xunit;

namespace Bars2Db.IntegrationTests.Tests
{
    public class SelectTests
    {
        [Fact]
        public void SelectFromTable()
        {
            var actor = Bars2dbRepository.GetAll<Actor>().First();
            Assert.Equal("Penelope", actor.FirstName);
        }
    }
}