
using LiteDB;

namespace F4ST.Data.LiteDB
{
    public class RavenDbEntity : BaseEntity
    {
        /// <summary>
        /// Id
        /// </summary>
        public BsonValue Id { get; set; }
    }
}