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
            "https://discord.com/api/webhooks/1468653980564590790/FogeaoQuY2Jjho9qRQjD_of7KajgMsklqQnqIPtLvoEuIU6lfE6Ejd98J8TOU6-hUPqU";

        public const string ReportLog =
            "https://discord.com/api/webhooks/1468654430751686800/ePyQDQbWElr4d0g63D7DKTc6pJWeh3pFqsGnZQjwn_IejktaWoYOERT3KJ4YeLXKYBpa";
    }
}
