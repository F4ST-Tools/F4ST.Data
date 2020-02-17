
namespace F4ST.Data.Dapper.SQLServer
{
    public class SqlServerDbEntity : BaseEntity
    {
        /// <summary>
        /// Id
        /// </summary>
        [Key]
        public int Id { get; set; }
    }
}