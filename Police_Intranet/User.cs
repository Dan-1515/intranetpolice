using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using Newtonsoft.Json; // [중요] 이 네임스페이스가 꼭 필요합니다.
using System;

namespace Police_Intranet.Models
{
    [Table("users")]
    public class User : BaseModel
    {
        public List<Work> work { get; set; }

        [PrimaryKey("id", false)]
        public int Id { get; set; }

        [Column("user_id")]
        [JsonProperty("user_id")]
        public int? UserId { get; set; }

        [Column("username")]
        [JsonProperty("username")] // JsonPropertyName 대신 JsonProperty 사용
        public string Username { get; set; }

        public override string ToString() => $"{UserId} {Username}";

        [Column("password_hash")]
        [JsonProperty("password_hash")]
        public string PasswordHash { get; set; }

        [Column("rank")]
        [JsonProperty("rank")]
        public string Rank { get; set; }

        [Column("created_at")]
        [JsonProperty("created_at")]
        public DateTime? CreatedAt { get; set; }

        [Column("checkin_time")]
        [JsonProperty("checkin_time")]
        public DateTime? CheckInTime { get; set; }

        [Column("checkout_time")]
        [JsonProperty("checkout_time")]
        public DateTime? CheckOutTime { get; set; }

        [Column("isworking")]
        [JsonProperty("isworking")]
        public bool IsWorking { get; set; }

        [Column("today_total_seconds")]
        [JsonProperty("today_total_seconds")]
        public long? TodayTotalSeconds { get; set; }

        [Column("week_total_seconds")]
        [JsonProperty("week_total_seconds")]
        public long? WeekTotalSeconds { get; set; }

        [Column("workdate")]
        [JsonProperty("workdate")]
        public DateTime? WorkDate { get; set; }

        // ---------------------------------------------------------

        [Column("is_admin")]
        [JsonProperty("is_admin")]
        public bool? IsAdmin { get; set; }

        // Postgres는 보통 소문자 컬럼명을 씁니다. DB가 "isapproved"라면 아래처럼 수정
        [Column("IsApproved")]
        [JsonProperty("isapproved")]
        public bool? IsApproved { get; set; }

        // DB가 "isriding"이라면 아래처럼 수정
        [Column("isriding")]
        [JsonProperty("isriding")]
        public bool? IsRiding { get; set; }

        [Column("level")]
        [JsonProperty("level")]
        public string Level { get; set; }

        [Column("rp")]
        [JsonProperty("rp")]
        public string RP { get; set; }

        [Column("rp_count")]
        [JsonProperty("rp_count")]
        public int RpCount { get; set; }

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
