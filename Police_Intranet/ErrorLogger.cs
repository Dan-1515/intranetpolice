using System;
using System.Threading.Tasks;
using Police_Intranet.Services;

namespace Police_Intranet.Services
{
    public static class ErrorLogger
    {
        private static DiscordWebhook _webhook;

        public static void Initialize(string webhookUrl)
        {
            _webhook = new DiscordWebhook(webhookUrl);
        }

        public static async Task LogAsync(string location, Exception ex)
        {
            try
            {
                if (_webhook == null) return;

                string message =
                    "🚨 **오류 발생**\n" +
                    $"📍 위치: {location}\n" +
                    $"🕒 시간: {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n" +
                    $"📄 메시지: {ex.Message}\n\n" +
                    $"```\n{ex.StackTrace}\n```";

                await _webhook.SendMessageAsync(message);
            }
            catch
            {
                // 🔥 웹훅 실패해도 앱 죽지 않게
            }
        }
    }
}