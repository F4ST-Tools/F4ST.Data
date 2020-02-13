using F4ST.Data;
using F4ST.Data.RavenDB;

namespace Test.Data
{
    [Table("TestT")]
    public class TestEntity : BaseDbEntity
    {
        public string Name { get; set; }
        public string Family { get; set; }
    }
}