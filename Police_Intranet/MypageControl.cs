using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Police_Intranet.Models;
using Police_Intranet.Services;
using Supabase;
using WinTimer = System.Windows.Forms.Timer;

namespace Police_Intranet
{
    public partial class MypageControl : UserControl
    {
        private Label lblUserid;
        private Label lblNickname;
        private Label lblRank;
        private Button btnToggleWork;
        private Label lblWeek;
        private Label lblWorkTime;
        private Label lblHireDate;
        private Label lblWeekTotal;
        private Label lblRpTotal;

        private FlowLayoutPanel workRankPanel;
        private FlowLayoutPanel rpRankPanel;

        private bool isCheckedIn = false;
        private TimeSpan todayTotal = TimeSpan.Zero;
        private TimeSpan weekTotal = TimeSpan.Zero;

        private DateTime? runtimeWorkStart = null;
        private WinTimer workTimer;

        public User currentUser;
        private DiscordWebhook workWebhook;
        private Supabase.Client supabase;

        private Work todayWork;
        private DateTime currentKstDate;
        private bool midnightPendingReset = false;

        private readonly int baseWorkTimeY = 164;
        private readonly int baseWeekY = 204;

        private readonly string supabaseUrl = "https://eeyxcupedhyoatovzepr.supabase.co";
        private readonly string supabaseKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImVleHhjdXBlZGh5b2F0b3Z6ZXByIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NjM2NDAzNjEsImV4cCI6MjA3OTIxNjM2MX0.jQKzE_ZO1t8x8heY0mqs0pttsb7R06KIGcDVOihwg-k";

        public MypageControl(User user, Client client)
        {
            currentUser = user;
            workWebhook = new DiscordWebhook(WebhookUrls.WorkLog);

            supabase = client ?? throw new ArgumentNullException(nameof(client));

            InitializeUi();
        }

        public async Task InitializeAsync()
        {
            await LoadTodayWorkAsync();
            await LoadUserRanksAsync();
        }

        private async Task LoadTodayWorkAsync()
        {
            await supabase.InitializeAsync();

            string today = GetKstNow().ToString("yyyy-MM-dd");

            var res = await supabase.From<Work>()
                .Filter("user_id", Supabase.Postgrest.Constants.Operator.Equals, currentUser.Id)
                .Filter("date", Supabase.Postgrest.Constants.Operator.Equals, today)
                .Limit(1)
                .Get();

            todayWork = res.Models.FirstOrDefault();

            if (todayWork == null)
            {
                todayTotal = TimeSpan.Zero;
                isCheckedIn = false;

                await LoadWeekFromLatestRowAsync();
                runtimeWorkStart = null;
            }
            else
            {
                todayTotal = TimeSpan.FromSeconds(todayWork.TodayTotalSeconds);
                weekTotal = TimeSpan.FromSeconds(todayWork.WeekTotalSeconds);
                isCheckedIn = todayWork.IsWorking;

                if (isCheckedIn && todayWork.LastWorkStart.HasValue)
                    runtimeWorkStart = todayWork.LastWorkStart.Value;
                else
                    runtimeWorkStart = null;
            }

            currentKstDate = GetKstNow().Date;

            UpdateDutyButton(isCheckedIn ? "on_duty" : "off_duty");
            UpdateWorkTimeLabel();
        }

        private async Task LoadWeekFromLatestRowAsync()
        {
            DateTime today = GetKstNow().Date;
            int diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
            DateTime weekStart = today.AddDays(-diff);
            DateTime weekEnd = weekStart.AddDays(7);

            var res = await supabase.From<Work>()
                .Filter("user_id", Supabase.Postgrest.Constants.Operator.Equals, currentUser.Id)
                .Filter("date", Supabase.Postgrest.Constants.Operator.GreaterThanOrEqual, weekStart.ToString("yyyy-MM-dd"))
                .Filter("date", Supabase.Postgrest.Constants.Operator.LessThan, weekEnd.ToString("yyyy-MM-dd"))
                .Order("date", Supabase.Postgrest.Constants.Ordering.Descending)
                .Limit(1)
                .Get();

            var latest = res.Models.FirstOrDefault();
            if (latest != null)
            {
                weekTotal = TimeSpan.FromSeconds(latest.WeekTotalSeconds);
            }
        }

        private void InitializeUi()
        {
            Dock = DockStyle.Fill;
            BackColor = Color.FromArgb(30, 30, 30);

            lblUserid = new Label
            {
                Text = $"고유번호: {currentUser.UserId}",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                AutoSize = true
            };

            lblNickname = new Label
            {
                Text = $"닉네임: {currentUser.Username}",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                AutoSize = true
            };

            lblRank = new Label
            {
                Text = $"직급: {currentUser.Rank}",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                AutoSize = true
            };

            btnToggleWork = new Button
            {
                Text = "확인중",
                Size = new Size(100, 40),
                BackColor = Color.FromArgb(100, 140, 240),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Enabled = true
            };

            // 🔹 여기 클릭 이벤트 추가 (즉시 UI 반영)
            btnToggleWork.Click += async (s, e) =>
            {
                btnToggleWork.Enabled = false;
                if (!isCheckedIn)
                {
                    // 출근
                    runtimeWorkStart = DateTime.UtcNow;
                    isCheckedIn = true;

                    if (todayWork == null)
                    {
                        todayWork = new Work
                        {
                            UserId = currentUser.Id,
                            Date = GetKstNow().ToString("yyyy-MM-dd"),
                            TodayTotalSeconds = 0,
                            WeekTotalSeconds = (long)weekTotal.TotalSeconds,
                            IsWorking = true,
                            LastWorkStart = runtimeWorkStart
                        };

                        await supabase.From<Work>().Insert(todayWork);
                    }
                    else
                    {
                        todayWork.IsWorking = true;
                        todayWork.LastWorkStart = runtimeWorkStart;
                        await supabase.From<Work>()
                            .Where(x => x.Id == todayWork.Id)
                            .Set(x => x.IsWorking, true)
                            .Set(x => x.LastWorkStart, runtimeWorkStart)
                            .Update();
                    }

                    UpdateDutyButton("on_duty");
                }
                else
                {
                    // 퇴근
                    await ForceCheckoutInternalAsync(DateTime.UtcNow);
                }

                btnToggleWork.Enabled = true;
            };

            lblWorkTime = new Label
            {
                ForeColor = Color.White,
                AutoSize = false,
                Width = 300,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Location = new Point((Width - 300) / 2, baseWorkTimeY)
            };

            lblWeek = new Label
            {
                ForeColor = Color.White,
                AutoSize = false,
                Width = 300,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Location = new Point((Width - 300) / 2, baseWeekY)
            };

            lblWeekTotal = new Label
            {
                Text = "주간 근무시간 순위",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                AutoSize = true
            };

            lblRpTotal = new Label
            {
                Text = "주간 RP횟수 순위",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                AutoSize = true
            };

            workRankPanel = new FlowLayoutPanel
            {
                Width = 300,
                Height = 380,
                BackColor = Color.FromArgb(50, 50, 50),
                AutoScroll = true
            };

            rpRankPanel = new FlowLayoutPanel
            {
                Width = 300,
                Height = 380,
                BackColor = Color.FromArgb(50, 50, 50),
                AutoScroll = true
            };

            Controls.AddRange(new Control[]
            {
                lblUserid, lblNickname, lblRank, lblHireDate, btnToggleWork, lblWorkTime, lblWeek,
                workRankPanel, rpRankPanel, lblWeekTotal, lblRpTotal
            });

            workTimer = new WinTimer { Interval = 1000 };
            workTimer.Tick += (s, e) => UpdateWorkTimeLabel();
            workTimer.Start();

            Resize += (s, e) => CenterUI();
            CenterUI();

            workRankPanel.TabStop = false;
            rpRankPanel.TabStop = false;

            lblWeekTotal.TextAlign = ContentAlignment.MiddleCenter;
            lblRpTotal.TextAlign = ContentAlignment.MiddleCenter;

            workRankPanel.FlowDirection = FlowDirection.TopDown;
            workRankPanel.WrapContents = false;
            rpRankPanel.FlowDirection = FlowDirection.TopDown;
            rpRankPanel.WrapContents = false;
        }

        private void CenterUI()
        {
            int cx = Width / 2;
            lblUserid.Location = new Point(cx - lblUserid.Width / 2, 60);
            lblNickname.Location = new Point(cx - lblNickname.Width / 2, 100);
            lblRank.Location = new Point(cx - lblRank.Width / 2, 140);
            btnToggleWork.Location = new Point(cx - btnToggleWork.Width / 2, 180);
            lblWorkTime.Location = new Point(cx - lblWorkTime.Width / 2, 240);
            lblWeek.Location = new Point(cx - lblWeek.Width / 2, 280);

            int gap = 20;

            workRankPanel.Location = new Point(cx - workRankPanel.Width - gap, 360);
            rpRankPanel.Location = new Point(cx + gap, 360);

            lblWeekTotal.Location = new Point(workRankPanel.Left + (workRankPanel.Width - lblWeekTotal.Width) / 2, workRankPanel.Top - lblWeekTotal.Height - 8);
            lblRpTotal.Location = new Point(rpRankPanel.Left + (rpRankPanel.Width - lblRpTotal.Width) / 2, rpRankPanel.Top - lblRpTotal.Height - 8);
        }

        private void UpdateDutyButton(string status)
        {
            btnToggleWork.Enabled = true;

            if (status == "on_duty")
            {
                btnToggleWork.Text = "출근중";
                btnToggleWork.ForeColor = Color.White;
                btnToggleWork.BackColor = Color.FromArgb(100, 140, 240);
            }
            else
            {
                btnToggleWork.Text = "퇴근중";
                btnToggleWork.ForeColor = Color.White;
                btnToggleWork.BackColor = Color.FromArgb(150, 50, 50);
            }
        }
        public async Task OnExternalCheckInAsync(bool checkIn, DateTime? workStart = null)
        {
            if (checkIn)
            {
                isCheckedIn = true;
                runtimeWorkStart = workStart ?? DateTime.UtcNow;

                UpdateDutyButton("on_duty");
                UpdateWorkTimeLabel(); // UI 즉시 반영

                _ = UpdateDbAsync(); // 백그라운드 DB 업데이트
            }
            else
            {
                if (!isCheckedIn) return;

                await ForceCheckoutInternalAsync(DateTime.UtcNow); // UI도 내부에서 업데이트됨
            }
        }
        private async Task ForceCheckoutInternalAsync(DateTime utcNow)
        {
            workTimer.Stop();

            if (!isCheckedIn || runtimeWorkStart == null || todayWork == null)
                return;

            TimeSpan worked = utcNow - runtimeWorkStart.Value;
            todayTotal += worked;
            weekTotal += worked;

            await supabase.From<Work>()
                .Where(x => x.Id == todayWork.Id)
                .Set(x => x.IsWorking, false)
                .Set(x => x.TodayTotalSeconds, (long)todayTotal.TotalSeconds)
                .Set(x => x.WeekTotalSeconds, (long)weekTotal.TotalSeconds)
                .Set(x => x.LastWorkStart, null)
                .Set(x => x.CheckoutTime, utcNow)
                .Update();

            await workWebhook?.SendWorkLogAsync(currentUser.UserId ?? 0, currentUser.Username, false, currentUser, runtimeWorkStart.Value, utcNow);
            await SendDutyLogToServerAsync("off_duty");

            runtimeWorkStart = null;
            isCheckedIn = false;
            UpdateDutyButton("off_duty");

            workTimer.Start();

            if (midnightPendingReset)
            {
                midnightPendingReset = false;
                _ = Task.Run(async () =>
                {
                    await Task.Delay(10000);
                    todayTotal = TimeSpan.Zero;
                });
            }

            await LoadTodayWorkAsync();
            await LoadUserRanksAsync();
        }

        private async Task UpdateDbAsync()
        {
            if (todayWork == null)
            {
                todayWork = new Work
                {
                    UserId = currentUser.Id,
                    Date = GetKstNow().ToString("yyyy-MM-dd"),
                    TodayTotalSeconds = 0,
                    WeekTotalSeconds = (long)weekTotal.TotalSeconds,
                    IsWorking = isCheckedIn,
                    LastWorkStart = runtimeWorkStart
                };
                await supabase.From<Work>().Insert(todayWork);
            }
            else
            {
                todayWork.IsWorking = isCheckedIn;
                todayWork.LastWorkStart = runtimeWorkStart;

                await supabase.From<Work>()
                    .Where(x => x.Id == todayWork.Id)
                    .Set(x => x.IsWorking, isCheckedIn)
                    .Set(x => x.LastWorkStart, runtimeWorkStart)
                    .Update();
            }
        }
        private async Task SendDutyLogToServerAsync(string status)
        {
            try
            {
                using var client = new HttpClient();

                client.DefaultRequestHeaders.Add("x-api-key", "duty_webhook_2026_secret_x9a2");

                var body = new
                {
                    userid = currentUser.Id,
                    name = currentUser.Username,
                    status = status
                };

                var content = new StringContent(
                    Newtonsoft.Json.JsonConvert.SerializeObject(body),
                    System.Text.Encoding.UTF8,
                    "application/json"
                );

                var res = await client.PostAsync(
                    "https://eeyxcupedhyoatovzepr.supabase.co/functions/v1/duty-webhook",
                    content
                );

                if (!res.IsSuccessStatusCode)
                {
                    var msg = await res.Content.ReadAsStringAsync();
                    Console.WriteLine("Duty log 실패: " + msg);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Duty log 예외: " + ex);
            }
        }

        private void UpdateWorkTimeLabel()
        {
            DateTime kstNow = GetKstNow();

            if (kstNow.Date != currentKstDate)
            {
                currentKstDate = kstNow.Date;

                if (isCheckedIn)
                    midnightPendingReset = true;
                else
                {
                    todayTotal = TimeSpan.Zero;
                    midnightPendingReset = false;
                }
            }

            TimeSpan displayToday = todayTotal;
            TimeSpan displayWeek = weekTotal;

            if (isCheckedIn && runtimeWorkStart.HasValue)
            {
                TimeSpan current = DateTime.UtcNow - runtimeWorkStart.Value;
                displayToday += current;
                displayWeek += current;
            }

            lblWorkTime.Text = $"일간 근무시간: {(int)displayToday.TotalHours}시간 {displayToday.Minutes}분 {displayToday.Seconds}초";
            lblWeek.Text = $"주간 근무시간: {(int)displayWeek.TotalHours}시간 {displayWeek.Minutes}분 {displayWeek.Seconds}초";
        }

        public async Task ForceCheckoutIfNeededAsync()
        {
            if (isCheckedIn)
                await ForceCheckoutInternalAsync(DateTime.UtcNow);
        }

        public async Task UpdateUserAsync(User user)
        {
            isCheckedIn = false;
            todayTotal = TimeSpan.Zero;
            weekTotal = TimeSpan.Zero;
            runtimeWorkStart = null;
            todayWork = null;

            workTimer.Stop();

            currentUser = user;
            lblNickname.Text = $"닉네임: {currentUser.Username}";
            lblRank.Text = $"직급: {currentUser.Rank}";
            CenterUI();

            await LoadTodayWorkAsync();
            await LoadUserRanksAsync();

            workTimer.Start();
        }

        public void RefreshWorkStatus()
        {
            UpdateWorkTimeLabel();
        }

        private static DateTime GetKstNow()
        {
            return TimeZoneInfo.ConvertTimeFromUtc(
                DateTime.UtcNow,
                TimeZoneInfo.FindSystemTimeZoneById("Korea Standard Time")
            );
        }

        public async Task ForceReloadFromDbAsync()
        {
            workTimer.Stop();

            isCheckedIn = false;
            runtimeWorkStart = null;
            todayTotal = TimeSpan.Zero;
            weekTotal = TimeSpan.Zero;
            todayWork = null;

            await LoadTodayWorkAsync();
            await LoadUserRanksAsync();

            UpdateWorkTimeLabel();
            workTimer.Start();
        }

        // 🔹 10위까지 Label에 표시
        public async Task LoadUserRanksAsync()
        {
            try
            {
                var userRes = await supabase.From<User>()
                    .Select("id, username, rp_count, IsApproved")
                    .Get();

                var users = userRes.Models
                    .Where(u => u.IsApproved == true)
                    .ToList();

                var workRes = await supabase.From<Work>()
                    .Select("user_id, week_total_seconds")
                    .Get();

                var works = workRes.Models ?? new List<Work>();

                var allWorkRanks = works
                    .GroupBy(w => w.UserId)
                    .Select(g => g.OrderByDescending(x => x.WeekTotalSeconds).First())
                    .Where(w => w.WeekTotalSeconds > 0)
                    .OrderByDescending(w => w.WeekTotalSeconds)
                    .Select(w => new { UserId = w.UserId, WeekSeconds = w.WeekTotalSeconds })
                    .ToList();

                workRankPanel.Controls.Clear();

                if (allWorkRanks.Count == 0)
                    workRankPanel.Controls.Add(CreateEmptyLabel());
                else
                {
                    int displayCount = Math.Min(10, allWorkRanks.Count);

                    for (int i = 0; i < displayCount; i++)
                    {
                        var rank = allWorkRanks[i];
                        var user = users.FirstOrDefault(u => u.Id == rank.UserId);
                        if (user == null) continue;

                        TimeSpan ts = TimeSpan.FromSeconds(rank.WeekSeconds);
                        string text = $"{user.Username} {(int)ts.TotalHours}시간 {ts.Minutes:D2}분";

                        bool isMe = user.Id == currentUser.Id;
                        workRankPanel.Controls.Add(CreateRankItem(i + 1, text, isMe));
                    }

                    var me = users.FirstOrDefault(u => u.Id == currentUser.Id);
                    if (me != null)
                    {
                        int myIndex = allWorkRanks.FindIndex(w => w.UserId == me.Id);
                        if (myIndex >= 10)
                        {
                            TimeSpan ts = TimeSpan.FromSeconds(allWorkRanks[myIndex].WeekSeconds);
                            string text = $"{me.Username} {(int)ts.TotalHours}시간 {ts.Minutes:D2}분";

                            workRankPanel.Controls.Add(CreateRankItem(-(myIndex + 1), text, true, 15));
                        }
                    }
                }

                var allRpRanks = users
                    .Where(u => u.RpCount > 0)
                    .OrderByDescending(u => u.RpCount)
                    .ToList();

                rpRankPanel.Controls.Clear();

                if (allRpRanks.Count == 0)
                    rpRankPanel.Controls.Add(CreateEmptyLabel());
                else
                {
                    int displayCount = Math.Min(10, allRpRanks.Count);

                    for (int i = 0; i < displayCount; i++)
                    {
                        var u = allRpRanks[i];
                        string text = $"{u.Username} {u.RpCount}회";

                        bool isMe = u.Id == currentUser.Id;
                        rpRankPanel.Controls.Add(CreateRankItem(i + 1, text, isMe));
                    }

                    int myIndex = allRpRanks.FindIndex(u => u.Id == currentUser.Id);
                    if (myIndex >= 10)
                    {
                        var me = allRpRanks[myIndex];
                        string text = $"{me.Username} {me.RpCount}회";

                        rpRankPanel.Controls.Add(CreateRankItem(-(myIndex + 1), text, true, 15));
                    }
                }
            }
            catch (Exception ex)
            {
                workRankPanel.Controls.Clear();
                rpRankPanel.Controls.Clear();

                workRankPanel.Controls.Add(CreateErrorLabel());
                rpRankPanel.Controls.Add(CreateErrorLabel());

                Console.WriteLine("LoadUserRanksAsync 오류: " + ex);
            }
        }

        private Control CreateRankItem(int rank, string text, bool isMe = false, int topMargin = 0)
        {
            Color textColor = rank switch
            {
                1 => Color.Gold,
                2 => Color.Silver,
                3 => Color.Peru,
                _ => Color.White
            };

            Color backColor = isMe ? Color.FromArgb(60, 60, 90) : Color.Transparent;

            var panel = new Panel
            {
                Width = workRankPanel.ClientSize.Width,
                Height = 30,
                Margin = new Padding(0, topMargin, 0, 2),
                BackColor = backColor
            };

            string rankText = rank > 0 ? $"{rank}위" : $"{Math.Abs(rank)}위";
            var lblRank = new Label
            {
                Text = $"[{rankText}]",
                ForeColor = textColor,
                TextAlign = ContentAlignment.MiddleLeft,
                Width = 50,
                Height = panel.Height,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                Location = new Point(0, 0),
                AutoSize = false
            };

            var lblContent = new Label
            {
                Text = text,
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = lblRank.Font,
                AutoSize = false,
                Location = new Point(lblRank.Right + 5, 0),
                Width = panel.Width - lblRank.Width - 5,
                Height = panel.Height
            };

            if (rank == 1) lblContent.Font = new Font("Segoe UI", 10.5f, FontStyle.Bold);
            if (rank == 2) lblContent.Font = new Font("Segoe UI", 10f, FontStyle.Bold);
            if (rank == 3) lblContent.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);

            panel.Controls.Add(lblRank);
            panel.Controls.Add(lblContent);

            return panel;
        }

        private Label CreateEmptyLabel()
        {
            return new Label
            {
                Text = "데이터가 없습니다.",
                ForeColor = Color.Gray,
                AutoSize = false,
                Width = 220,
                Height = 28,
                TextAlign = ContentAlignment.MiddleCenter
            };
        }

        private Label CreateErrorLabel()
        {
            return new Label
            {
                Text = "데이터를 불러올 수 없습니다.",
                ForeColor = Color.IndianRed,
                AutoSize = false,
                Width = 220,
                Height = 28,
                TextAlign = ContentAlignment.MiddleCenter
            };
        }
    }
}
