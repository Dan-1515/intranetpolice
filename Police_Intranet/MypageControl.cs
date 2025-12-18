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

            string today = DateTime.Today.ToString("yyyy-MM-dd");

            var res = await supabase.From<Work>()
                .Filter("user_id", Supabase.Postgrest.Constants.Operator.Equals, currentUser.Id)
                .Filter("date", Supabase.Postgrest.Constants.Operator.Equals, today)
                .Limit(1)
                .Get();

            todayWork = res.Models.FirstOrDefault();

            if (todayWork == null)
            {
                todayTotal = TimeSpan.Zero;
                weekTotal = TimeSpan.Zero;
                isCheckedIn = false;
            }
            else
            {
                todayTotal = TimeSpan.FromSeconds(todayWork.TodayTotalSeconds);
                weekTotal = TimeSpan.FromSeconds(todayWork.WeekTotalSeconds);
                isCheckedIn = todayWork.IsWorking;
            }

            runtimeWorkStart = isCheckedIn ? todayWork.LastWorkStart : null;

            btnToggleWork.Text = isCheckedIn ? "퇴근" : "출근";
            UpdateWorkTimeLabel();
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

            btnToggleWork = new Button
            {
                Text = "출근",
                Size = new Size(100, 40),
                BackColor = Color.FromArgb(50, 50, 50),
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

            Controls.AddRange(new Control[] { lblNickname, lblRank, btnToggleWork, lblWorkTime, lblWeek });

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
            btnToggleWork.Location = new Point(cx - btnToggleWork.Width / 2, 150);
            lblWorkTime.Location = new Point(cx - lblWorkTime.Width / 2, 200);
            lblWeek.Location = new Point(cx - lblWeek.Width / 2, 240);
        }

        private async Task ToggleWorkAsync()
        {
            DateTime now = DateTime.UtcNow;
            string today = DateTime.Today.ToString("yyyy-MM-dd");

            if (!isCheckedIn)
            {
                // ✅ 출근
                isCheckedIn = true;
                runtimeWorkStart = now;
                btnToggleWork.Text = "퇴근";

                if (todayWork == null)
                {
                    var inserted = await supabase.From<Work>().Insert(new Work
                    {
                        UserId = currentUser.Id,
                        Date = today,
                        IsWorking = true,
                        LastWorkStart = now,
                        TodayTotalSeconds = 0,
                        WeekTotalSeconds = 0,
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

            workTimer.Start();
        }

        private void UpdateWorkTimeLabel()
        {
            TimeSpan displayToday = todayTotal;
            TimeSpan displayWeek = weekTotal;

            if (isCheckedIn && runtimeWorkStart.HasValue)
            {
                TimeSpan current = DateTime.UtcNow - runtimeWorkStart.Value;
                displayToday += current;
                displayWeek += current;
            }

            lblWorkTime.Text =
                $"금일 근무시간: {(int)displayToday.TotalHours}시간 {displayToday.Minutes}분 {displayToday.Seconds}초";

            lblWeek.Text =
                $"금주 근무시간: {(int)displayWeek.TotalHours}시간 {displayWeek.Minutes}분 {displayWeek.Seconds}초";
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
    }
}
