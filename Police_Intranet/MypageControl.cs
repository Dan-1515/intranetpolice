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
        private readonly string supabaseKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImVleXhjdXBlZGh5b2F0b3Z6ZXByIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NjM2NDAzNjEsImV4cCI6MjA3OTIxNjM2MX0.jQKzE_ZO1t8x8heY0mqs0pttsb7R06KIGcDVOihwg-k";

        public MypageControl(User user, Client client)
        {
            currentUser = user;
            workWebhook = new DiscordWebhook(WebhookUrls.WorkLog);

            supabase = new Supabase.Client(
                supabaseUrl,
                supabaseKey,
                new SupabaseOptions { AutoConnectRealtime = false }
            );

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

                // 오늘 기록 없으면 → 주간 최신값 로드
                await LoadWeekFromLatestRowAsync();

                runtimeWorkStart = null;
            }
            else
            {
                todayTotal = TimeSpan.FromSeconds(todayWork.TodayTotalSeconds);
                weekTotal = TimeSpan.FromSeconds(todayWork.WeekTotalSeconds);
                isCheckedIn = todayWork.IsWorking;

                // 🔥 여기 중요
                if (isCheckedIn && todayWork.LastWorkStart.HasValue)
                    runtimeWorkStart = todayWork.LastWorkStart.Value;
                else
                    runtimeWorkStart = null;
            }

            currentKstDate = GetKstNow().Date;

            btnToggleWork.Text = isCheckedIn ? "퇴근" : "출근";
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

            lblHireDate = new Label
            {
                Text = $"입사일: {FormatHireDate(currentUser.CreatedAt)}",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                AutoSize = true
            };

            btnToggleWork = new Button
            {
                Text = "출근",
                Size = new Size(100, 40),
                BackColor = Color.FromArgb(100, 140, 240),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnToggleWork.Click += async (s, e) => await ToggleWorkAsync();

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

            // 🔹 랭킹 Panel 추가
            workRankPanel = new FlowLayoutPanel
            {
                Width = 250,
                Height = 250,
                BackColor = Color.FromArgb(50, 50, 50),
                AutoScroll = true
            };

            rpRankPanel = new FlowLayoutPanel
            {
                Width = 250,
                Height = 250,
                BackColor = Color.FromArgb(50, 50, 50),
                AutoScroll = true
            };

            Controls.AddRange(new Control[]
            {
                lblNickname, lblRank, lblHireDate, btnToggleWork, lblWorkTime, lblWeek,
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

        }

        private void CenterUI()
        {
            int cx = Width / 2;
            lblNickname.Location = new Point(cx - lblNickname.Width / 2, 60);
            lblRank.Location = new Point(cx - lblRank.Width / 2, 100);
            lblHireDate.Location = new Point(cx - lblHireDate.Width / 2, 140);
            btnToggleWork.Location = new Point(cx - btnToggleWork.Width / 2, 180);
            lblWorkTime.Location = new Point(cx - lblWorkTime.Width / 2, 240);
            lblWeek.Location = new Point(cx - lblWeek.Width / 2, 280);
            
            int gap = 20; // 두 랭킹 사이 간격

            workRankPanel.Location = new Point(cx - workRankPanel.Width - gap, 360);
            rpRankPanel.Location = new Point(cx + gap, 360);

            lblWeekTotal.Location = new Point(workRankPanel.Left + (workRankPanel.Width - lblWeekTotal.Width) / 2, workRankPanel.Top - lblWeekTotal.Height - 8);  // ← 위 여백 (숫자만 조절)
            lblRpTotal.Location = new Point(rpRankPanel.Left + (rpRankPanel.Width - lblRpTotal.Width) / 2, rpRankPanel.Top - lblRpTotal.Height - 8);

        }

        private async Task ToggleWorkAsync()
        {
            DateTime now = DateTime.UtcNow;
            string today = GetKstNow().ToString("yyyy-MM-dd");

            if (!isCheckedIn)
            {
                isCheckedIn = true;
                runtimeWorkStart = now;
                btnToggleWork.Text = "퇴근";
                btnToggleWork.BackColor = Color.FromArgb(150, 50, 50);

                if (todayWork == null)
                {
                    var inserted = await supabase.From<Work>().Insert(new Work
                    {
                        UserId = currentUser.Id,
                        Date = today,
                        IsWorking = true,
                        LastWorkStart = now,
                        TodayTotalSeconds = 0,
                        WeekTotalSeconds = (long)weekTotal.TotalSeconds,
                        CheckinTime = now
                    });

                    todayWork = inserted.Models.First();
                }
                else
                {
                    await supabase.From<Work>()
                        .Where(x => x.Id == todayWork.Id)
                        .Set(x => x.IsWorking, true)
                        .Set(x => x.LastWorkStart, now)
                        .Set(x => x.CheckinTime, now)
                        .Update();
                }

                await workWebhook?.SendWorkLogAsync(currentUser.Username, true, currentUser, now, null);
            }
            else
            {
                await ForceCheckoutInternalAsync(now);
            }

            UpdateWorkTimeLabel();
            await LoadUserRanksAsync();
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

            await workWebhook?.SendWorkLogAsync(currentUser.Username, false, currentUser, runtimeWorkStart.Value, utcNow);

            runtimeWorkStart = null;
            isCheckedIn = false;
            btnToggleWork.Text = "출근";
            btnToggleWork.BackColor = Color.FromArgb(100, 140, 240);

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

            await LoadUserRanksAsync();
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

        private string FormatHireDate(DateTime? createdAt)
        {
            if (!createdAt.HasValue)
                return "알 수 없음";

            DateTime kst = createdAt.Value.ToLocalTime();
            return kst.ToString("yyyy-MM-dd");
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
            lblHireDate.Text = $"입사일: {FormatHireDate(currentUser.CreatedAt)}";
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
                // 🔹 유저 정보
                var userRes = await supabase.From<User>()
                    .Select("id, username, rp_count")
                    .Get();

                var users = userRes.Models ?? new List<User>();

                // 🔹 근무 정보
                var workRes = await supabase.From<Work>()
                    .Select("user_id, week_total_seconds")
                    .Get();

                var works = workRes.Models ?? new List<Work>();

                // =========================
                // 주간 출근 랭킹
                // =========================
                var workRanks = works
                    .Where(w => (w.WeekTotalSeconds) > 0)
                    .GroupBy(w => w.UserId)
                    .Select(g => new
                    {
                        UserId = g.Key,
                        WeekSeconds = g.Max(x => x.WeekTotalSeconds)
                    })
                    .OrderByDescending(x => x.WeekSeconds)
                    .Take(10)
                    .ToList();

                workRankPanel.Controls.Clear();

                if (workRanks.Count == 0)
                {
                    workRankPanel.Controls.Add(CreateEmptyLabel());
                }
                else
                {
                    for (int i = 0; i < workRanks.Count; i++)
                    {
                        var rank = workRanks[i];
                        var user = users.FirstOrDefault(u => u.Id == rank.UserId);
                        if (user == null) continue;

                        TimeSpan ts = TimeSpan.FromSeconds(rank.WeekSeconds);

                        string text =
                            $"{user.Username} {(int)ts.TotalHours}시간 {ts.Minutes:D2}분";

                        workRankPanel.Controls.Add(
                            CreateRankItem(i + 1, text)
                        );
                    }
                }

                // =========================
                // 주간 RP 랭킹
                // =========================
                var rpRanks = users
                    .Where(u => u.RpCount > 0)
                    .OrderByDescending(u => u.RpCount)
                    .Take(10)
                    .ToList();

                rpRankPanel.Controls.Clear();

                if (rpRanks.Count == 0)
                {
                    rpRankPanel.Controls.Add(CreateEmptyLabel());
                }
                else
                {
                    for (int i = 0; i < rpRanks.Count; i++)
                    {
                        var u = rpRanks[i];
                        string text = $"{u.Username} {u.RpCount}회";

                        rpRankPanel.Controls.Add(
                            CreateRankItem(i + 1, text)
                        );
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

        private Control CreateRankItem(int rank, string text)
        {
            Color textColor = rank switch
            {
                1 => Color.Gold,
                2 => Color.Silver,
                3 => Color.Peru,
                _ => Color.White
            };

            var panel = new Panel
            {
                Width = workRankPanel.ClientSize.Width - 10,
                Height = 30,
                Margin = new Padding(0, 0, 0, 6)
            };

            // 1️⃣ 순위 Label (왼쪽 고정)
            var lblRank = new Label
            {
                Text = $"[{rank}위]",
                ForeColor = textColor,
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Left,
                Width = 50,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold)
            };

            // 2️⃣ 이름+시간 Label (순위 오른쪽, 왼쪽으로 10px 이동)
            var lblContent = new Label
            {
                Text = text,
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleLeft, // 중앙에서 왼쪽으로 변경
                Dock = DockStyle.Fill,
                Padding = new Padding(10, 0, 0, 0), // 왼쪽으로 10px 이동
                Font = new Font("Segoe UI", 9f, FontStyle.Bold)
            };

            // 상위 3위 폰트 조절
            if (rank == 1) lblContent.Font = new Font("Segoe UI", 10.5f, FontStyle.Bold);
            if (rank == 2) lblContent.Font = new Font("Segoe UI", 10f, FontStyle.Bold);
            if (rank == 3) lblContent.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);

            panel.Controls.Add(lblContent); // 먼저 content
            panel.Controls.Add(lblRank);     // 순위는 Dock.Left로 왼쪽 고정

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
