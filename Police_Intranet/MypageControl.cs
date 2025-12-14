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
        // UI 컨트롤
        private Label lblNickname;
        private Button btnToggleWork;
        private Label lblWeek;
        private Label lblWorkTime;

        // 상태 변수
        private bool isCheckedIn = false;
        private DateTime? workStartTime;
        private TimeSpan todayTotal = TimeSpan.Zero;
        private TimeSpan weekTotal = TimeSpan.Zero;
        private DateTime todayDate = DateTime.Today;
        private WinTimer workTimer;

        // 모델 및 서비스
        private User currentUser;
        private DiscordWebhook workWebhook;
        private System.Windows.Forms.Timer timer;

        // Supabase 설정
        private readonly string supabaseUrl = "https://eeyxcupedhyoatovzepr.supabase.co";
        private readonly string supabaseKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImVleXhjdXBlZGh5b2F0b3Z6ZXByIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NjM2NDAzNjEsImV4cCI6MjA3OTIxNjM2MX0.jQKzE_ZO1t8x8heY0mqs0pttsb7R06KIGcDVOihwg-k";

        private Supabase.Client supabase;
        private long? currentWorkId;

        // 🔹 외부에서 접근 가능하도록 프로퍼티 추가
        public User CurrentUser => currentUser;

        public MypageControl(User user, Client client, DiscordWebhook webhook)
        {
            currentUser = user ?? throw new ArgumentNullException(nameof(user));
            workWebhook = webhook;

            var options = new Supabase.SupabaseOptions
            {
                AutoRefreshToken = true,
                AutoConnectRealtime = false
            };
            supabase = new Supabase.Client(supabaseUrl, supabaseKey, options);

            InitializeUi();
        }

        // ⭐ Main.cs에서 호출 가능하도록 public async 메서드 추가
        public async Task InitializeAsync()
        {
            await InitializeSupabaseAndStatusAsync();
        }

        // ⭐ 외부에서 근무 상태 새로고침용
        public async Task RefreshWorkStatus()
        {
            await InitializeSupabaseAndStatusAsync();
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
                Text = "금주 근무시간: 0시간 0분 0초",
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

        private async Task InitializeSupabaseAndStatusAsync()
        {
            try
            {
                await supabase.InitializeAsync();
                string todayStr = DateTime.Today.ToString("yyyy-MM-dd");

                // 유저 ID 보정
                if (currentUser.Id == 0 && !string.IsNullOrEmpty(currentUser.Username))
                {
                    var userRes = await supabase.From<User>()
                        .Filter("username", Supabase.Postgrest.Constants.Operator.Equals, currentUser.Username)
                        .Limit(1)
                        .Get();

                    currentUser.Id = userRes.Models.FirstOrDefault()?.Id ?? 0;
                }

                // 오늘 데이터 조회
                var todayRes = await supabase.From<Work>()
                    .Filter("user_id", Supabase.Postgrest.Constants.Operator.Equals, currentUser.Id)
                    .Filter("date", Supabase.Postgrest.Constants.Operator.Equals, todayStr)
                    .Order("id", Supabase.Postgrest.Constants.Ordering.Descending)
                    .Limit(1)
                    .Get();

                var todayWork = todayRes.Models.FirstOrDefault();

                if (todayWork != null)
                {
                    isCheckedIn = todayWork.IsWorking;
                    todayTotal = TimeSpan.FromSeconds(todayWork.TodayTotalSeconds);
                    weekTotal = TimeSpan.FromSeconds(todayWork.WeekTotalSeconds);

                    if (isCheckedIn)
                    {
                        workStartTime = todayWork.CheckinTime ?? DateTime.Now;
                        currentWorkId = todayWork.Id;
                        StartWorkTimer();
                    }
                }
                else
                {
                    todayTotal = TimeSpan.Zero;
                    isCheckedIn = false;
                    workStartTime = null;
                    currentWorkId = null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"데이터 로드 중 오류 발생: {ex.Message}");
            }

            btnToggleWork.Text = isCheckedIn ? "퇴근" : "출근";
            UpdateWorkTimeLabel();
        }

        private void StartWorkTimer()
        {
            workTimer?.Stop();
            workTimer?.Dispose();

            workTimer = new WinTimer { Interval = 1000 };
            workTimer.Tick += (s, e) => UpdateWorkTimeLabel();
            workTimer.Start();
        }

        private async Task ToggleWorkStatusAsync()
        {
            DateTime now = DateTime.Now;
            string todayStr = now.ToString("yyyy-MM-dd");

            try
            {
                if (!isCheckedIn)
                {
                    isCheckedIn = true;
                    btnToggleWork.Text = "퇴근";
                    workStartTime = now;

                    var newWork = new Work
                    {
                        UserId = currentUser.Id,
                        Date = todayStr,
                        CheckinTime = now,
                        IsWorking = true,
                        TodayTotalSeconds = (long)todayTotal.TotalSeconds,
                        WeekTotalSeconds = (long)weekTotal.TotalSeconds
                    };

                    var res = await supabase.From<Work>().Insert(newWork);
                    currentWorkId = res.Models.First().Id;

                    if (workWebhook != null)
                        await workWebhook.SendWorkLogAsync(currentUser.Username, true, currentUser, now, null);
                }
                else
                {
                    if (currentWorkId == null) return;

                    isCheckedIn = false;
                    btnToggleWork.Text = "출근";

                    TimeSpan worked = now - workStartTime.Value;
                    todayTotal += worked;
                    weekTotal += worked;

                    await supabase.From<Work>()
                        .Where(x => x.Id == currentWorkId.Value)
                        .Set(x => x.CheckoutTime, now)
                        .Set(x => x.IsWorking, false)
                        .Set(x => x.TodayTotalSeconds, (long)todayTotal.TotalSeconds)
                        .Set(x => x.WeekTotalSeconds, (long)weekTotal.TotalSeconds)
                        .Update();

                    if (workWebhook != null)
                        await workWebhook.SendWorkLogAsync(currentUser.Username, false, currentUser, workStartTime.Value, now);

                    workStartTime = null;
                    currentWorkId = null;
                }

                UpdateWorkTimeLabel();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"근무 상태 토글 오류: {ex.Message}");
            }
        }

        private void UpdateWorkTimeLabel()
        {
            DateTime now = DateTime.Now;

            TimeSpan displayToday = todayTotal;
            TimeSpan displayWeek = weekTotal;

            if (isCheckedIn && workStartTime.HasValue)
            {
                TimeSpan current = now - workStartTime.Value;
                displayToday += current;
                displayWeek += current;
            }

            lblWorkTime.Text = $"금일 근무시간: {displayToday.Hours}시간 {displayToday.Minutes}분 {displayToday.Seconds}초";
            lblWeek.Text = $"금주 근무시간: {displayWeek.Hours}시간 {displayWeek.Minutes}분 {displayWeek.Seconds}초";

            lblWorkTime.Location = new Point(this.Width / 2 - lblWorkTime.PreferredWidth / 2, lblWorkTime.Location.Y);
            lblWeek.Location = new Point(this.Width / 2 - lblWeek.PreferredWidth / 2, lblWeek.Location.Y);
        }

        public void RefreshUserInfo()
        {
            lblNickname.Text = $"닉네임: {currentUser.Username}";
            lblNickname.Location = new Point(this.Width / 2 - lblNickname.PreferredWidth / 2, lblNickname.Location.Y);
        }
    }
}
