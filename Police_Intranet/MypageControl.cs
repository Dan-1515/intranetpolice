using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using Police_Intranet.Models;   // User 모델
using Police_Intranet.Services; // DiscordWebhook

namespace Police_Intranet
{
    public partial class MypageControl : UserControl
    {
        private Label lblNickname;
        private Button btnToggleWork;
        private Label lblWeek;
        private Label lblWorkTime;

        private bool isCheckedIn = false;
        private DateTime? workStartTime;
        private TimeSpan todayTotal = TimeSpan.Zero;
        private TimeSpan weekTotal = TimeSpan.Zero;
        private DateTime todayDate = DateTime.Today;

        private User currentUser;
        private DiscordWebhook workWebhook;

        private System.Windows.Forms.Timer timer;
        private readonly HttpClient httpClient;

        private readonly string table = "work";
        private readonly string postgrestUrl = "https://eeyxcupedhyoatovzepr.supabase.co/rest/v1";
        private readonly string postgrestApiKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImVleHhjdXBlZGh5b2F0b3Z6ZXByIiwicm9sZSI6InNlcnZpY2Vfcm9sZSIsImlhdCI6MTc2MzY0MDM2MSwiZXhwIjoyMDc5MjE2MzYxfQ.P0I8kUVDoyRgByiFo2rjVGcMqd_0eDdLfio8VwMBfcU";

        public MypageControl(User user, DiscordWebhook webhook)
        {
            currentUser = user ?? throw new ArgumentNullException(nameof(user));
            workWebhook = webhook;

            httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("apikey", postgrestApiKey);
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {postgrestApiKey}");

            InitializeUi();
            _ = InitializeCheckInStatusAsync();
            UpdateWorkTimeLabel();
        }

        private void InitializeUi()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.FromArgb(30, 30, 30);

            int startY = 60;
            int gap = 50;

            lblNickname = new Label
            {
                Text = $"닉네임: {currentUser.Username}",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                AutoSize = true
            };
            this.Controls.Add(lblNickname);

            btnToggleWork = new Button
            {
                Text = "출근",
                Size = new Size(100, 40),
                BackColor = Color.FromArgb(50, 50, 50),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12, FontStyle.Bold)
            };
            btnToggleWork.FlatAppearance.BorderSize = 0;
            btnToggleWork.Click += async (s, e) => await ToggleWorkStatusAsync();
            this.Controls.Add(btnToggleWork);

            lblWorkTime = new Label
            {
                Text = "금일 근무시간: 0시간 0분 0초",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11, FontStyle.Regular),
                AutoSize = true
            };
            this.Controls.Add(lblWorkTime);

            lblWeek = new Label
            {
                Text = "이번주 근무시간: 0시간 0분 0초",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11, FontStyle.Regular),
                AutoSize = true
            };
            this.Controls.Add(lblWeek);

            CenterControls(startY, gap);

            timer = new System.Windows.Forms.Timer { Interval = 1000 };
            timer.Tick += (s, e) => UpdateWorkTimeLabel();
            timer.Start();

            this.Resize += (s, e) => CenterControls(startY, gap);
        }

        private void CenterControls(int startY, int gap)
        {
            int center = this.Width / 2;
            lblNickname.Location = new Point(center - lblNickname.PreferredWidth / 2, startY);
            btnToggleWork.Location = new Point(center - btnToggleWork.Width / 2, startY + gap);
            lblWorkTime.Location = new Point(center - lblWorkTime.PreferredWidth / 2, startY + gap * 2);
            lblWeek.Location = new Point(center - lblWeek.PreferredWidth / 2, startY + gap * 3);
        }

        private async Task InitializeCheckInStatusAsync()
        {
            string todayStr = todayDate.ToString("yyyy-MM-dd");
            string url = $"{postgrestUrl}/{table}?user_id=eq.{currentUser.Id}&date=eq.{todayStr}&order=id.desc&limit=1";

            try
            {
                var res = await httpClient.GetAsync(url);
                var json = await res.Content.ReadAsStringAsync();
                var works = JsonSerializer.Deserialize<List<WorkDto>>(json);

                if (works != null && works.Count > 0)
                {
                    var row = works[0];

                    // 이상 상태 체크: is_working=true인데 checkin_time이 없으면 자동 복구
                    if (row.is_working && !row.checkin_time.HasValue)
                    {
                        var patchObj = new { is_working = false };
                        string patchUrl = $"{postgrestUrl}/{table}?id=eq.{row.id}";
                        var content = new StringContent(JsonSerializer.Serialize(patchObj), Encoding.UTF8, "application/json");
                        await httpClient.PatchAsync(patchUrl, content);

                        row.is_working = false; // 앱 상태도 false로
                    }

                    isCheckedIn = row.is_working;
                    todayTotal = TimeSpan.FromSeconds(row.today_total_seconds);
                    weekTotal = TimeSpan.FromSeconds(row.week_total_seconds);
                    workStartTime = row.checkin_time;
                }
            }
            catch { }

            btnToggleWork.Text = isCheckedIn ? "퇴근" : "출근";
            UpdateLabels();
        }

        private async Task ToggleWorkStatusAsync()
        {
            DateTime now = DateTime.Now;
            string todayStr = todayDate.ToString("yyyy-MM-dd");

            if (!isCheckedIn)
            {
                // 출근
                isCheckedIn = true;
                btnToggleWork.Text = "퇴근";
                workStartTime = now;

                var insertObj = new
                {
                    user_id = currentUser.Id,
                    date = todayStr,
                    checkin_time = now,
                    checkout_time = (DateTime?)null,
                    is_working = true,
                    today_total_seconds = (long)todayTotal.TotalSeconds,
                    week_total_seconds = (long)weekTotal.TotalSeconds
                };

                var content = new StringContent(JsonSerializer.Serialize(insertObj), Encoding.UTF8, "application/json");
                await httpClient.PostAsync($"{postgrestUrl}/{table}", content);

                if (workWebhook != null)
                    await workWebhook.SendWorkLogAsync(currentUser.Username, true, currentUser, now, null);
            }
            else
            {
                // 퇴근
                if (!workStartTime.HasValue)
                {
                    // 예외 방지: null이면 그냥 체크아웃만 처리
                    workStartTime = now;
                }

                isCheckedIn = false;
                btnToggleWork.Text = "출근";

                TimeSpan worked = now - workStartTime.Value;
                todayTotal += worked;
                weekTotal += worked;

                var patchObj = new
                {
                    checkout_time = now,
                    is_working = false,
                    today_total_seconds = (long)todayTotal.TotalSeconds,
                    week_total_seconds = (long)weekTotal.TotalSeconds
                };

                string patchUrl = $"{postgrestUrl}/{table}?user_id=eq.{currentUser.Id}&date=eq.{todayStr}&is_working=eq.true";

                var content = new StringContent(JsonSerializer.Serialize(patchObj), Encoding.UTF8, "application/json");
                await httpClient.PatchAsync(patchUrl, content);

                if (workWebhook != null)
                    await workWebhook.SendWorkLogAsync(currentUser.Username, false, currentUser, workStartTime.Value, now);

                workStartTime = null;
            }

            UpdateLabels();
        }

        private void UpdateWorkTimeLabel()
        {
            DateTime now = DateTime.Now;

            TimeSpan displayToday = todayTotal;
            if (isCheckedIn && workStartTime.HasValue)
                displayToday += now - workStartTime.Value;

            lblWorkTime.Text = $"금일 근무시간: {displayToday.Hours}시간 {displayToday.Minutes}분 {displayToday.Seconds}초";
            lblWorkTime.Location = new Point(this.Width / 2 - lblWorkTime.PreferredWidth / 2, lblWorkTime.Location.Y);

            TimeSpan displayWeek = weekTotal;
            if (isCheckedIn && workStartTime.HasValue)
                displayWeek += now - workStartTime.Value;

            lblWeek.Text = $"이번주 근무시간: {displayWeek.Hours}시간 {displayWeek.Minutes}분 {displayWeek.Seconds}초";
            lblWeek.Location = new Point(this.Width / 2 - lblWeek.PreferredWidth / 2, lblWeek.Location.Y);
        }

        private void UpdateLabels()
        {
            UpdateWorkTimeLabel();
        }

        private class WorkDto
        {
            public int id { get; set; }
            public int user_id { get; set; }
            public DateTime date { get; set; }
            public DateTime? checkin_time { get; set; }
            public DateTime? checkout_time { get; set; }
            public bool is_working { get; set; }
            public long today_total_seconds { get; set; }
            public long week_total_seconds { get; set; }
        }
    }
}
