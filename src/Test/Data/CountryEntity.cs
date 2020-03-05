using System.ComponentModel.DataAnnotations;
using F4ST.Data;
using F4ST.Data.Dapper;

namespace Test.Data
{
    /// <summary>
    /// جدول اطلاعات کشور ها
    /// </summary>
    [Table("Country",Schema = "dbo")]
    public class CountryEntity:BaseDbEntity
    {
        /// <summary>
        /// عنوان کشور
        /// </summary>
        public string CountryName { get; set; } 
        /// <summary>
        /// آدرس عکس پرچم کشور
        /// </summary>
        public string CountryImageUrl { get; set; }
        /// <summary>
        /// سیمبل ارزی کشور
        /// </summary>
        [MaxLength(20)]
        public string CurrencySymbol { get; set; }
        /// <summary>
        /// کد ISO
        /// </summary>
        [MaxLength(5)]
        public string ISOCode { get; set; }
        /// <summary>
        /// فعال / غیرفعال
        /// </summary>
        public bool? IsActive { get; set; }
    }
}