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

        private bool isCheckedIn = false;
        private TimeSpan todayTotal = TimeSpan.Zero;
        private TimeSpan weekTotal = TimeSpan.Zero;

        // 🔥 화면 실시간 계산 전용
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

        public MypageControl(User user, Client client, DiscordWebhook webhook)
        {
            currentUser = user;
            workWebhook = webhook;

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
            }
            else
            {
                todayTotal = TimeSpan.FromSeconds(todayWork.TodayTotalSeconds);
                weekTotal = TimeSpan.FromSeconds(todayWork.WeekTotalSeconds);
                isCheckedIn = todayWork.IsWorking;
            }

            if (todayWork == null)
            {
                await LoadWeekFromLatestRowAsync();
            }


            runtimeWorkStart = isCheckedIn ? todayWork.LastWorkStart : null;
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

            Controls.AddRange(new Control[] { lblNickname, lblRank, lblHireDate, btnToggleWork, lblWorkTime, lblWeek });

            workTimer = new WinTimer { Interval = 1000 };
            workTimer.Tick += (s, e) => UpdateWorkTimeLabel();
            workTimer.Start();

            Resize += (s, e) => CenterUI();
            CenterUI();
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
        }

        private async Task ToggleWorkAsync()
        {
            DateTime now = DateTime.UtcNow;
            string today = GetKstNow().ToString("yyyy-MM-dd");

            if (!isCheckedIn)
            {
                // ✅ 출근
                isCheckedIn = true;
                runtimeWorkStart = now;
                btnToggleWork.Text = "퇴근";
                btnToggleWork.BackColor = Color.FromArgb(150, 50, 50); // 연한 빨강

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

                await workWebhook?.SendWorkLogAsync(
                    currentUser.Username, true, currentUser, now, null
                );
            }
            else
            {
                await ForceCheckoutInternalAsync(now);
            }

            UpdateWorkTimeLabel();
        }

        private async Task ForceCheckoutInternalAsync(DateTime utcNow)
        {
            workTimer.Stop();

            if (!isCheckedIn || runtimeWorkStart == null || todayWork == null)
                return;

            // 🔥 출근 이후 실제 근무 시간 1회 누적
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

            await workWebhook?.SendWorkLogAsync(
                currentUser.Username,
                false,
                currentUser,
                runtimeWorkStart.Value,
                utcNow
            );

            runtimeWorkStart = null;
            isCheckedIn = false;
            btnToggleWork.Text = "출근";
            btnToggleWork.BackColor = Color.FromArgb(100, 140, 240); // 연한 파랑

            workTimer.Start();

            if (midnightPendingReset)
            {
                midnightPendingReset = false;
                _ = Task.Run(async () =>
                {
                    await Task.Delay(10000); //10초 대기
                    todayTotal = TimeSpan.Zero;
                });
            }
        }

        private async void UpdateWorkTimeLabel()
        {
            DateTime kstNow = GetKstNow();

            // 🔥 자정 감지 (KST 기준 날짜 변경)
            if (kstNow.Date != currentKstDate)
            {
                currentKstDate = kstNow.Date;

                if (isCheckedIn)
                {
                    // 출근 중이면 → 퇴근 후 처리 예약
                    midnightPendingReset = true;
                }
                else
                {
                    // 퇴근 상태면 즉시 초기화
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

            lblWorkTime.Text =
                $"일간 근무시간: {(int)displayToday.TotalHours}시간 {displayToday.Minutes}분 {displayToday.Seconds}초";

            lblWeek.Text =
                $"주간 근무시간: {(int)displayWeek.TotalHours}시간 {displayWeek.Minutes}분 {displayWeek.Seconds}초";
        }

        private string FormatHireDate(DateTime? createdAt)
        {
            if (!createdAt.HasValue)
                return "알 수 없음";

            // Supabase timestamptz → 이미 UTC
            DateTime kst = createdAt.Value.ToLocalTime();

            return kst.ToString("yyyy-MM-dd");
        }

        // 앱 종료 대비
        public async Task ForceCheckoutIfNeededAsync()
        {
            if (isCheckedIn)
                await ForceCheckoutInternalAsync(DateTime.UtcNow);
        }

        public async Task UpdateUserAsync(User user)
        {
            // 🔥 이전 유저 상태 완전 초기화
            isCheckedIn = false;
            todayTotal = TimeSpan.Zero;
            weekTotal = TimeSpan.Zero;
            runtimeWorkStart = null;
            todayWork = null;

            workTimer.Stop();

            // 유저 교체
            currentUser = user;
            lblNickname.Text = $"닉네임: {currentUser.Username}";
            lblRank.Text = $"직급: {currentUser.Rank}";
            lblHireDate.Text = $"입사일: {FormatHireDate(currentUser.CreatedAt)}";
            CenterUI();

            // 🔥 새 유저 기준으로 다시 로드
            await LoadTodayWorkAsync();

            workTimer.Start();
        }

        // AdminControl 호환용
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
            // 🔥 타이머 중지
            workTimer.Stop();

            // 🔥 런타임 상태 완전 초기화
            isCheckedIn = false;
            runtimeWorkStart = null;
            todayTotal = TimeSpan.Zero;
            weekTotal = TimeSpan.Zero;
            todayWork = null;

            // 🔥 DB 기준으로 다시 로드
            await LoadTodayWorkAsync();

            // 🔥 UI 갱신
            UpdateWorkTimeLabel();

            workTimer.Start();
        }

    }
}
