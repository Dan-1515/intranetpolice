using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;

namespace Police_Intranet.Models
{
    [Table("work")]
    public class Work : BaseModel
    {
        [PrimaryKey("id", false)]
        public int Id { get; set; }

        [Column("user_id")]
        public int UserId { get; set; }

        [Column("date")]
        public DateTime Date { get; set; }

        [Column("today_total_seconds")]
        public long TodayTotalSeconds { get; set; }

        [Column("week_total_seconds")]
        public long WeekTotalSeconds { get; set; }

        [Column("last_work_start")]
        public DateTime? LastWorkStart { get; set; }

        [Column("is_working")]
        public bool IsWorking { get; set; }

        [Column("checkin_time")]
        public DateTime? CheckinTime { get; set; }

        [Column("checkout_time")]
        public DateTime? CheckoutTime { get; set; }
    }
}
