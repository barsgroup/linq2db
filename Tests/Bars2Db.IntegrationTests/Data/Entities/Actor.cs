using Bars2Db.Mapping;

namespace Bars2Db.IntegrationTests.Data.Entities
{
    [Table("actor")]
    public class Actor
    {
        [Column("first_name")]
        public string FirstName { get; set; }

        [Column("last_name")]
        public string LastName { get; set; }
    }
}