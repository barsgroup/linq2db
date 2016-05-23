using System.Linq;
using System.Runtime.InteropServices;
using Bars2Db.Data;
using Bars2Db.DataProvider.PostgreSQL;

namespace Bars2Db.IntegrationTests.Data
{
    public class Bars2dbRepository
    {
        private static DataConnection db = new DataConnection(new PostgreSQLDataProvider(), "Server=172.21.21.75;Port=5434;Database=bars2dbTests;User ID=postgres;Password=123qwe;CommandTimeout=1024");
        public static IQueryable<TEntity> GetAll<TEntity>() where TEntity : class
        {
            return db.GetTable<TEntity>();
        }
    }
}