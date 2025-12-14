using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq; // LINQ 사용을 위해 추가
using System.Windows.Forms;
using Police_Intranet.Models;   // User, Work 모델
using Police_Intranet.Services; // DiscordWebhook
using Supabase; // Supabase 클라이언트 네임스페이스
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

        public MypageControl(User user, Client client, DiscordWebhook webhook)
        {
            currentUser = user ?? throw new ArgumentNullException(nameof(user));
            workWebhook = webhook;

            // Supabase 클라이언트 초기화
            var options = new Supabase.SupabaseOptions
            {
                AutoRefreshToken = true,
                AutoConnectRealtime = false // 401 오류 해결을 위해 false로 변경 (실시간 기능 끄기)
            };
            supabase = new Supabase.Client(supabaseUrl, supabaseKey, options);

            InitializeUi();

            // 비동기 초기화 실행 (생성자에서는 await 불가하므로 Task.Run 혹은 _ = 패턴 사용)
            _ = InitializeSupabaseAndStatusAsync();

            UpdateWorkTimeLabel();
        }

        public async Task InitializeAsync()
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

                string todayStr = DateTime.Now.ToString("yyyy-MM-dd");

                // 🔹 user id 보정
                if (currentUser.Id == 0 && !string.IsNullOrEmpty(currentUser.Username))
                {
                    var userRes = await supabase.From<User>()
                        .Filter("username", Supabase.Postgrest.Constants.Operator.Equals, currentUser.Username)
                        .Limit(1)
                        .Get();

                    var dbUser = userRes.Models.FirstOrDefault();
                    if (dbUser != null)
                        currentUser.Id = dbUser.Id;
                }

                // 🔹 오늘 날짜 기준 조회 (핵심)
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
                    else
                    {
                        workStartTime = null;
                        currentWorkId = null;
                    }
                }
                else
                {
                    // 🔹 오늘 데이터 없으면 → 가장 최근 데이터로 주간 누적만 복구
                    var lastRes = await supabase.From<Work>()
                        .Filter("user_id", Supabase.Postgrest.Constants.Operator.Equals, currentUser.Id)
                        .Order("date", Supabase.Postgrest.Constants.Ordering.Descending)
                        .Order("id", Supabase.Postgrest.Constants.Ordering.Descending)
                        .Limit(1)
                        .Get();

                    var lastWork = lastRes.Models.FirstOrDefault();

                    if (lastWork != null && IsSameWeekString(lastWork.Date, todayStr))
                        weekTotal = TimeSpan.FromSeconds(lastWork.WeekTotalSeconds);
                    else
                        weekTotal = TimeSpan.Zero;

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
            UpdateLabels();
            await CheckAndResetDailyTimeAsync();
        }

        private bool IsSameWeekString(string d1, string d2)
        {
            DateTime dt1 = DateTime.Parse(d1);
            DateTime dt2 = DateTime.Parse(d2);
            return IsSameWeek(dt1, dt2);
        }

        private async Task CheckAndResetDailyTimeAsync()
        {
            DateTime today = DateTime.Today;

            // 오늘 날짜와 현재 저장된 todayDate 비교
            if (todayDate != today)
            {
                todayDate = today;

                if (!isCheckedIn)
                {
                    // 출근 중이 아니면 바로 초기화
                    todayTotal = TimeSpan.Zero;
                    workStartTime = null;
                    UpdateWorkTimeLabel();

                    // Work 테이블에 오늘 데이터 없으면 생성
                    string todayStr = today.ToString("yyyy-MM-dd");
                    var todayRes = await supabase.From<Work>()
                        .Filter("user_id", Supabase.Postgrest.Constants.Operator.Equals, currentUser.Id)
                        .Filter("date", Supabase.Postgrest.Constants.Operator.Equals, todayStr)
                        .Get();

                    if (!todayRes.Models.Any())
                    {
                        var newWork = new Work
                        {
                            UserId = currentUser.Id,
                            Date = todayStr,
                            TodayTotalSeconds = 0,
                            WeekTotalSeconds = (long)weekTotal.TotalSeconds,
                            IsWorking = false
                        };
                        await supabase.From<Work>().Insert(newWork);
                    }
                }
                else
                {
                    // 출근 중이면, 퇴근 후 30초 뒤에 초기화
                    System.Windows.Forms.Timer resetTimer = new System.Windows.Forms.Timer
                    {
                        Interval = 30000 // 30초
                    };

                    resetTimer.Tick += async (s, e) =>
                    {
                        resetTimer.Stop();
                        resetTimer.Dispose();

                        todayTotal = TimeSpan.Zero;
                        workStartTime = null;
                        isCheckedIn = false;

                        UpdateWorkTimeLabel();

                        // Work 테이블에 오늘 데이터 없으면 생성
                        string todayStr = today.ToString("yyyy-MM-dd");
                        var todayRes = await supabase.From<Work>()
                            .Filter("user_id", Supabase.Postgrest.Constants.Operator.Equals, currentUser.Id)
                            .Filter("date", Supabase.Postgrest.Constants.Operator.Equals, todayStr)
                            .Get();

                        if (!todayRes.Models.Any())
                        {
                            var newWork = new Work
                            {
                                UserId = currentUser.Id,
                                Date = todayStr,
                                TodayTotalSeconds = 0,
                                WeekTotalSeconds = (long)weekTotal.TotalSeconds,
                                IsWorking = false
                            };
                            await supabase.From<Work>().Insert(newWork);
                        }
                    };

                    resetTimer.Start();
                }
            }
        }

        public async Task RefreshWorkStatus()
        {
            await InitializeSupabaseAndStatusAsync();
        }


        private void StartWorkTimer()
        {
            workTimer?.Stop();
            workTimer?.Dispose();

            workTimer = new WinTimer();  // ⭐ 별칭 사용
            workTimer.Interval = 1000;
            workTimer.Tick += WorkTimer_Tick;
            workTimer.Start();
        }

        private void WorkTimer_Tick(object sender, EventArgs e)
        {
            UpdateWorkTimeLabel();
        }

        private bool IsSameWeek(DateTime date1, DateTime date2)
        {
            var cal = System.Globalization.CultureInfo.CurrentCulture.Calendar;
            var d1 = date1.Date.AddDays(-1 * (int)cal.GetDayOfWeek(date1));
            var d2 = date2.Date.AddDays(-1 * (int)cal.GetDayOfWeek(date2));
            return d1 == d2;
        }

        private async Task ToggleWorkStatusAsync()
        {
            DateTime now = DateTime.Now;
            string todayStr = now.ToString("yyyy-MM-dd");

            try
            {
                if (!isCheckedIn)
                {
                    // ─── 출근 처리 ───
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

                    // ─── 출근 웹훅 전송 ───
                    if (workWebhook != null)
                    {
                        try
                        {
                            await workWebhook.SendWorkLogAsync(currentUser.Username, true, currentUser, now, null);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"웹훅 전송 실패(출근): {ex.Message}");
                        }
                    }
                }
                else
                {
                    // ─── 퇴근 처리 ───
                    if (currentWorkId == null)
                        throw new Exception("근무 ID 없음");

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

                    // ─── 퇴근 웹훅 전송 ───
                    if (workWebhook != null)
                    {
                        try
                        {
                            await workWebhook.SendWorkLogAsync(currentUser.Username, false, currentUser, workStartTime.Value, now);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"웹훅 전송 실패(퇴근): {ex.Message}");
                        }
                    }

                    workStartTime = null;
                    currentWorkId = null;
                }

                UpdateLabels();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
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

            lblWeek.Text = $"금주 근무시간: {displayWeek.Hours}시간 {displayWeek.Minutes}분 {displayWeek.Seconds}초";
            lblWeek.Location = new Point(this.Width / 2 - lblWeek.PreferredWidth / 2, lblWeek.Location.Y);
        }

        private void UpdateLabels()
        {
            UpdateWorkTimeLabel();
        }
    }
}
