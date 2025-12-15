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
        private Button btnToggleWork;
        private Label lblWeek;
        private Label lblWorkTime;

        private bool isCheckedIn = false;
        private TimeSpan todayTotal = TimeSpan.Zero;
        private TimeSpan weekTotal = TimeSpan.Zero;

        // 🔥 화면 계산 전용 (DB 절대 참조 금지)
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

                // 🔥 재실행 시 절대 누적 방지
                isCheckedIn = todayWork.IsWorking;
                runtimeWorkStart = null;
            }

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

            btnToggleWork = new Button
            {
                Text = "출근",
                Size = new Size(100, 40),
                BackColor = Color.FromArgb(50, 50, 50),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnToggleWork.Click += async (s, e) => await ToggleWorkAsync();

            // 🔹 위치/크기 고정 레이블
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

            Controls.AddRange(new Control[] { lblNickname, btnToggleWork, lblWorkTime, lblWeek });

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
            btnToggleWork.Location = new Point(cx - btnToggleWork.Width / 2, 110);
            lblWorkTime.Location = new Point(cx - lblWorkTime.Width / 2, 160);
            lblWeek.Location = new Point(cx - lblWeek.Width / 2, 200);
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
            if (runtimeWorkStart == null || todayWork == null) return;

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
                utcNow - worked,
                utcNow
            );

            runtimeWorkStart = null;
            isCheckedIn = false;
            btnToggleWork.Text = "출근";
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

            lblWorkTime.Text = $"금일 근무시간: {(int)displayToday.TotalHours}시간 {displayToday.Minutes}분 {displayToday.Seconds}초";
            lblWorkTime.Font = new Font("Segoe UI", 12, FontStyle.Bold);

            lblWeek.Text = $"금주 근무시간: {(int)displayWeek.TotalHours}시간 {displayWeek.Minutes}분 {displayWeek.Seconds}초";
            lblWeek.Font = new Font("Segoe UI", 12, FontStyle.Bold);
        }

        // 🔥 앱 종료 대응
        public async Task ForceCheckoutAsync()
        {
            if (!isCheckedIn || runtimeWorkStart == null) return;
            await ForceCheckoutInternalAsync(DateTime.UtcNow);
        }

        // Main.cs 호환
        public async Task ForceCheckoutIfNeededAsync()
        {
            await ForceCheckoutAsync();
        }

        // AdminControl 호환
        public void RefreshWorkStatus()
        {
            UpdateWorkTimeLabel();
        }

        public void UpdateUser(User user)
        {
            currentUser = user;
            lblNickname.Text = $"닉네임: {currentUser.Username}";
            CenterUI();
        }
    }
}
