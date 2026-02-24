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
     "https://discord.com/api/webhooks/1433432108264722545/4wp-I4rpcJR0FhFPnvVsUMkPAps9qi7KZa1O-n6lT3S7YzQhl5NojmznuJEUwNLfP7WY";

        public const string ReportLog =
            "https://discord.com/api/webhooks/1458789125128720547/STl0TZq1Mru9kt2BviYHXgE6T6sFQapwMLig7KPMS8N17juFA1K24xPkMQNsahAwi9Ip";
    }
}
