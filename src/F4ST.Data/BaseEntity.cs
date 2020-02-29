using System;

namespace F4ST.Data
{
    public class BaseEntity
    {
        /// <summary>
        /// زمان ایجاد
        /// </summary>
        public DateTime CreateOn { get; set; } = DateTime.Now;

        /// <summary>
        /// زمان بروزرسانی
        /// </summary>
        public DateTime? ModifiedOn { get; set; }

        /// <summary>
        /// آیا رکورد حذف شده است
        /// </summary>
        public bool IsDeleted { get; set; } = false;
    }
}