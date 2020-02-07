using F4ST.Data;

namespace Test.Data
{
    [Table("TestT")]
    public class TestEntity : DbEntity
    {
        public string Name { get; set; }
        public string Family { get; set; }
    }
}