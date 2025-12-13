using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq; // LINQ 사용을 위해 추가
using System.Threading.Tasks;
using System.Windows.Forms;
using Police_Intranet.Models;   // User, Work 모델
using Police_Intranet.Services; // DiscordWebhook
using Supabase; // Supabase 클라이언트 네임스페이스

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

        // 모델 및 서비스
        private User currentUser;
        private DiscordWebhook workWebhook;
        private System.Windows.Forms.Timer timer;

        // Supabase 설정
        private readonly string supabaseUrl = "https://eeyxcupedhyoatovzepr.supabase.co";
        private readonly string supabaseKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImVleXhjdXBlZGh5b2F0b3Z6ZXByIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NjM2NDAzNjEsImV4cCI6MjA3OTIxNjM2MX0.jQKzE_ZO1t8x8heY0mqs0pttsb7R06KIGcDVOihwg-k";

        private Supabase.Client supabase;

        public MypageControl(User user, DiscordWebhook webhook)
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

        private async Task InitializeSupabaseAndStatusAsync()
        {
            try
            {
                // 1. Supabase 클라이언트 연결 초기화
                await supabase.InitializeAsync();

                // [추가] User ID가 0이거나 확실하지 않을 경우를 대비해 Username으로 ID를 다시 조회
                if (currentUser.Id == 0 && !string.IsNullOrEmpty(currentUser.Username))
                {
                    var userRes = await supabase.From<User>()
                        .Filter("username", Supabase.Postgrest.Constants.Operator.Equals, currentUser.Username)
                        .Limit(1)
                        .Get();
                    var dbUser = userRes.Models.FirstOrDefault();
                    if (dbUser != null) currentUser.Id = dbUser.Id;
                }

                // 2. DB에서 '오늘' 상태 가져오기
                string todayStr = DateTime.Today.ToString("yyyy-MM-dd");

                var response = await supabase.From<Work>()
                    .Filter("user_id", Supabase.Postgrest.Constants.Operator.Equals, currentUser.Id)
                    .Filter("date", Supabase.Postgrest.Constants.Operator.Equals, todayStr)
                    .Order("id", Supabase.Postgrest.Constants.Ordering.Descending)
                    .Limit(1)
                    .Get();

                var latestWork = response.Models.FirstOrDefault();

                if (latestWork != null)
                {
                    // === CASE A: 오늘 기록이 있는 경우 (앱 재실행, 근무 중 등) ===

                    // 이상 상태 체크: 근무중인데 checkin_time이 없으면 자동 복구
                    if (latestWork.IsWorking && !latestWork.CheckinTime.HasValue)
                    {
                        await supabase.From<Work>()
                             .Where(x => x.Id == latestWork.Id)
                             .Set(x => x.IsWorking, false)
                             .Update();

                        latestWork.IsWorking = false;
                    }

                    isCheckedIn = latestWork.IsWorking;
                    todayTotal = TimeSpan.FromSeconds(latestWork.TodayTotalSeconds);
                    weekTotal = TimeSpan.FromSeconds(latestWork.WeekTotalSeconds);
                    workStartTime = latestWork.CheckinTime;
                }
                else
                {
                    // === CASE B: 오늘 기록이 없는 경우 (앱 재실행 시 오늘 첫 출근 전) ===
                    // [추가] 어제까지의 기록을 조회해서 '이번 주 누적 시간'을 불러와야 함

                    var lastResponse = await supabase.From<Work>()
                        .Filter("user_id", Supabase.Postgrest.Constants.Operator.Equals, currentUser.Id)
                        .Order("id", Supabase.Postgrest.Constants.Ordering.Descending)
                        .Limit(1)
                        .Get();

                    var lastWork = lastResponse.Models.FirstOrDefault();

                    if (lastWork != null && IsSameWeek(lastWork.Date, DateTime.Today))
                    {
                        // 지난 기록이 이번 주라면 누적 시간 이어받기
                        weekTotal = TimeSpan.FromSeconds(lastWork.WeekTotalSeconds);
                    }
                    else
                    {
                        // 지난주 기록이거나 기록이 아예 없으면 0
                        weekTotal = TimeSpan.Zero;
                    }

                    // 오늘은 아직 근무 전
                    isCheckedIn = false;
                    todayTotal = TimeSpan.Zero;
                    workStartTime = null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"데이터 로드 중 오류 발생: {ex.Message}");
            }

            // UI 업데이트 (UI 스레드에서 실행되도록 Invoke 확인)
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() =>
                {
                    btnToggleWork.Text = isCheckedIn ? "퇴근" : "출근";
                    UpdateLabels();
                }));
            }
            else
            {
                btnToggleWork.Text = isCheckedIn ? "퇴근" : "출근";
                UpdateLabels();
            }
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
            string todayStr = todayDate.ToString("yyyy-MM-dd");

            try
            {
                if (!isCheckedIn)
                {
                    // === 출근 처리 ===
                    isCheckedIn = true;
                    btnToggleWork.Text = "퇴근";
                    workStartTime = now;

                    var newWork = new Work
                    {
                        UserId = currentUser.Id,
                        Date = todayDate, // DateTime 타입 그대로 사용 가능
                        CheckinTime = now,
                        CheckoutTime = null,
                        IsWorking = true,
                        TodayTotalSeconds = (long)todayTotal.TotalSeconds,
                        WeekTotalSeconds = (long)weekTotal.TotalSeconds
                    };

                    await supabase.From<Work>().Insert(newWork);

                    if (workWebhook != null)
                        await workWebhook.SendWorkLogAsync(currentUser.Username, true, currentUser, now, null);
                }
                else
                {
                    // === 퇴근 처리 ===
                    if (!workStartTime.HasValue) workStartTime = now;

                    isCheckedIn = false;
                    btnToggleWork.Text = "출근";

                    TimeSpan worked = now - workStartTime.Value;
                    todayTotal += worked;
                    weekTotal += worked;

                    // 현재 '근무 중'인 항목을 찾아 업데이트
                    // Match를 사용하여 WHERE 조건 설정 (user_id, date, is_working=true)
                    await supabase.From<Work>()
                        .Set(x => x.CheckoutTime, now)
                        .Set(x => x.IsWorking, false)
                        .Set(x => x.TodayTotalSeconds, (long)todayTotal.TotalSeconds)
                        .Set(x => x.WeekTotalSeconds, (long)weekTotal.TotalSeconds)
                        .Match(new Dictionary<string, string>
                        {
                            { "user_id", currentUser.Id.ToString() },
                            { "date", todayStr },
                            { "is_working", "true" }
                        })
                        .Update();

                    if (workWebhook != null)
                        await workWebhook.SendWorkLogAsync(currentUser.Username, false, currentUser, workStartTime.Value, now);

                    workStartTime = null;
                }

                UpdateLabels();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"상태 변경 중 오류 발생: {ex.Message}");
                // 에러 발생 시 상태 롤백 로직이 필요할 수 있음
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

            lblWeek.Text = $"이번주 근무시간: {displayWeek.Hours}시간 {displayWeek.Minutes}분 {displayWeek.Seconds}초";
            lblWeek.Location = new Point(this.Width / 2 - lblWeek.PreferredWidth / 2, lblWeek.Location.Y);
        }

        private void UpdateLabels()
        {
            UpdateWorkTimeLabel();
        }
    }
}
