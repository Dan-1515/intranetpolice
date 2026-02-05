using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using Police_Intranet.Models; // User 모델 참조

namespace Police_Intranet.Services
{
    public static class WebhookUrls
    {
        public const string WorkLog =
            "https://discord.com/api/webhooks/1433432108264722545/4wp-I4rpcJR0FhFPnvVsUMkPAps9qi7KZa1O-n6lT3S7YzQhl5NojmznuJEUwNLfP7WY"; // 개인 디코 웹훅이므로 변경필요

        public const string ReportLog =
            "https://discord.com/api/webhooks/1468654430751686800/ePyQDQbWElr4d0g63D7DKTc6pJWeh3pFqsGnZQjwn_IejktaWoYOERT3KJ4YeLXKYBpa"; // 개인 디코 X 변경 불필요
    }
}
