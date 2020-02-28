
namespace F4ST.Data.Dapper
{
    public class BaseDbEntity : BaseEntity
    {
        /// <summary>
        /// Id
        /// </summary>
        [Key]
        public int Id { get; set; }
    }
}