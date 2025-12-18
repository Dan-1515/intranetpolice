using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using Police_Intranet.Models; // User 모델 참조

namespace Police_Intranet.Services
{
    public class DiscordWebhook
    {
        private readonly string webhookUrl;
        private static readonly HttpClient httpClient = new HttpClient();

        public DiscordWebhook(string url)
        {
            webhookUrl = url ?? throw new ArgumentNullException(nameof(url));
        }

        // ===================== Embed 전송 =====================
        public async Task SendEmbedAsync(string title, string description, int colorHex, string footerText, string imageUrl = null)
        {
            // Dictionary 사용 → null 필드는 아예 JSON에서 제외됨 (500 방지)
            var embed = new Dictionary<string, object>
            {
                { "title", title },
                { "description", description },
                { "color", colorHex },
                { "footer", new { text = footerText } }
            };

            // 이미지 존재 시에만 thumbnail 필드 추가
            if (!string.IsNullOrEmpty(imageUrl))
            {
                embed.Add("thumbnail", new { url = imageUrl });
            }

            var payload = new
            {
                username = "경찰청 로그",
                embeds = new[] { embed }
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync(webhookUrl, content);
            if (!response.IsSuccessStatusCode)
            {
                var errorText = await response.Content.ReadAsStringAsync();
                throw new Exception($"웹훅 전송 실패: {response.StatusCode}\n{errorText}");
            }
        }

        // ===================== 출퇴근 로그 전송 =====================
        public async Task SendWorkLogAsync(string userName, bool isCheckedIn, User user, DateTime checkInTime, DateTime? checkOutTime)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            string action = isCheckedIn ? "출근" : "퇴근";
            string description;

            if (isCheckedIn)
            {
                description =
                    $"**닉네임 : ** {userName}\n\n" +
                    $"**상태 : ** {action}";
            }
            else
            {
                TimeSpan workedTime = checkOutTime.HasValue ? checkOutTime.Value - checkInTime : TimeSpan.Zero;
                string workedTimeText = FormatTimeSpan(workedTime);

                description =
                    $"**닉네임 : ** {userName}\n\n" +
                    $"**상태 : ** {action}\n\n" +
                    $"**총 출근시간 : ** {workedTimeText}";
            }

            string footer = $"Made By dadev  |  {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
            int color = isCheckedIn ? 0x00FF00 : 0xFF0000;

            // ❤️ 디스코드 로고 URL
            string logoUrl = "https://cdn.discordapp.com/attachments/1427149493123420290/1441946603467571380/ver.png";

            await SendEmbedAsync("경찰청 출퇴근 로그", description, color, footer, logoUrl);
        }

        // ===================== TimeSpan → 문자열 변환 =====================
        private string FormatTimeSpan(TimeSpan t)
        {
            return $"{t.Days}일 {t.Hours}시간 {t.Minutes}분 {t.Seconds}초";
        }

    }
}
