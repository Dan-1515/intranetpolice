using Supabase;
using Postgrest;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;
using System.Text.Json.Serialization;

namespace Police_Intranet.Models
{
    [Table("users")]
    public class User : BaseModel
    {
        [PrimaryKey("id", false)]
        public int Id { get; set; }

        [Column("username")]
        [JsonPropertyName("username")]
        public string Username { get; set; }

        // [Column("userid")]
        // [JsonPropertyName("userid")]
        // public string UserId { get; set; }

        [Column("password_hash")]
        [JsonPropertyName("password_hash")]
        public string PasswordHash { get; set; }

        [Column("rank")]
        [JsonPropertyName("rank")]
        public string Rank { get; set; }

        [Column("created_at")]
        [JsonPropertyName("created_at")]
        public DateTime? CreatedAt { get; set; }

        [Column("checkin_time")]
        [JsonPropertyName("checkin_time")]
        public DateTime? CheckInTime { get; set; }

        [Column("checkout_time")]
        [JsonPropertyName("checkout_time")]
        public DateTime? CheckOutTime { get; set; }

        [Column("isworking")]
        [JsonPropertyName("isworking")]
        public bool? IsWorking { get; set; }

        [Column("today_total_seconds")]
        [JsonPropertyName("today_total_seconds")]
        public long? TodayTotalSeconds { get; set; }

        [Column("week_total_seconds")]
        [JsonPropertyName("week_total_seconds")]
        public long? WeekTotalSeconds { get; set; }

        [Column("workdate")]
        public DateTime? WorkDate { get; set; }

        [Column("is_admin")]
        [JsonPropertyName("is_admin")]
        public bool? IsAdmin { get; set; }

        [Column("IsApproved")]
        [JsonPropertyName("isapproved")]
        public bool? IsApproved { get; set; }

        [Column("isRiding")]
        [JsonPropertyName("isriding")]
        public bool? IsRiding { get; set; }

        [Column("level")]
        [JsonPropertyName("level")]
        public string Level { get; set; }

        [Column("rp")]
        [JsonPropertyName("rp")]
        public string RP { get; set; }

        public User GetWeekResetCopy()
        {
            return new User
            {
                Id = this.Id,
                WeekTotalSeconds = 0
            };
        }
    }
}
