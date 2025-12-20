using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Supabase;
using Police_Intranet.Models;

namespace Police_Intranet
{
    public partial class AdminControl : UserControl
    {
        private Supabase.Client client;

        private Panel panelSignupWaiting;
        private Panel panelUserlist;
        private Panel panelWeekTime;

        private ListBox lbWaiting;
        private ListBox lbUsers;
        private ListBox lbTimes;
        private ListBox lbRidingUsers;

        private Button btnApprove;
        private Button btnReject;
        private Button btnUpdate;
        private Button btnDelete;
        private Button btnForceRelease;

        private TextBox txtName;
        private TextBox txtRank;

        private Main main;
        private MypageControl mypageControl;

        private int selectedPk = -1;

        public AdminControl(Supabase.Client supabaseClient, Main main, MypageControl mypageControl)
        {
            this.client = supabaseClient ?? throw new ArgumentNullException(nameof(supabaseClient));

            _ = client.InitializeAsync();
            InitializeComponent();
            InitializeUI();

            _ = LoadAllDataAsync();
            this.main = main;
            this.mypageControl = mypageControl;
        }

        public async Task InitializeAsync()
        {
            await client.InitializeAsync();
            await LoadAllDataAsync();
        }

        private void InitializeUI()
        {
            this.BackColor = Color.FromArgb(30, 30, 30);
            this.Dock = DockStyle.Fill;

            TableLayoutPanel table = new TableLayoutPanel()
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1,
                Padding = new Padding(10)
            };
            for (int i = 0; i < 3; i++)
                table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
            table.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            // ─ 회원가입 대기 ─
            panelSignupWaiting = new Panel() { Dock = DockStyle.Fill, BackColor = Color.FromArgb(30, 30, 30), Padding = new Padding(10) };
            panelSignupWaiting.Controls.Add(new Label() { Text = "회원가입 대기 목록", ForeColor = Color.White, Font = new Font("Segoe UI", 14, FontStyle.Bold), Location = new Point(80, 10), AutoSize = true });

            lbWaiting = new ListBox() { Location = new Point(50, 50), Size = new Size(250, 300), BackColor = Color.FromArgb(50, 50, 50), ForeColor = Color.White };
            panelSignupWaiting.Controls.Add(lbWaiting);

            btnApprove = new Button() { Text = "가입 승인", Location = new Point(70, 360), Size = new Size(100, 35), BackColor = Color.FromArgb(70, 70, 70), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnApprove.Click += BtnApprove_Click;
            panelSignupWaiting.Controls.Add(btnApprove);

            btnReject = new Button() { Text = "가입 거부", Location = new Point(180, 360), Size = new Size(100, 35), BackColor = Color.FromArgb(150, 50, 50), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnReject.Click += async (s, e) => await BtnReject_ClickAsync();
            panelSignupWaiting.Controls.Add(btnReject);

            // ─ 탑승 중 유저 ─
            Label lblRiding = new Label()
            {
                Text = "마쯔다 운행 관리",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Location = new Point(90, 410), // 기존 라벨과 비슷하게 좌측 맞춤
                AutoSize = true
            };
            panelSignupWaiting.Controls.Add(lblRiding);

            lbRidingUsers = new ListBox()
            {
                Location = new Point(50, lblRiding.Bottom + 10), // 라벨 바로 아래로
                Size = new Size(250, 150),
                BackColor = Color.FromArgb(50, 50, 50),
                ForeColor = Color.White
            };
            panelSignupWaiting.Controls.Add(lbRidingUsers);

            btnForceRelease = new Button()
            {
                Text = "강제 해제",
                Location = new Point(110, lbRidingUsers.Bottom + 10), // 리스트박스 아래로 간격 유지
                Size = new Size(100, 35),
                BackColor = Color.FromArgb(150, 50, 50),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnForceRelease.Click += async (s, e) => await BtnForceRelease_ClickAsync();
            panelSignupWaiting.Controls.Add(btnForceRelease);


            // ─ 유저 관리 ─
            panelUserlist = new Panel() { Dock = DockStyle.Fill, BackColor = Color.FromArgb(30, 30, 30), Padding = new Padding(10) };
            panelUserlist.Controls.Add(new Label() { Text = "전체 유저 목록", ForeColor = Color.White, Font = new Font("Segoe UI", 14, FontStyle.Bold), Location = new Point(100, 10), AutoSize = true });

            lbUsers = new ListBox() { Location = new Point(20, 50), Size = new Size(230, 300), BackColor = Color.FromArgb(50, 50, 50), ForeColor = Color.White };
            lbUsers.SelectedIndexChanged += LbUsers_SelectedIndexChanged;
            panelUserlist.Controls.Add(lbUsers);

            txtName = new TextBox() { Location = new Point(260, 90), Size = new Size(120, 25) };
            txtRank = new TextBox() { Location = new Point(260, 130), Size = new Size(120, 25) };
            panelUserlist.Controls.Add(txtName);
            panelUserlist.Controls.Add(txtRank);

            btnUpdate = new Button() { Text = "저장", Location = new Point(260, 170), Size = new Size(120, 35), BackColor = Color.FromArgb(70, 70, 70), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnUpdate.Click += async (s, e) => await BtnUpdate_ClickAsync();
            panelUserlist.Controls.Add(btnUpdate);

            btnDelete = new Button() { Text = "삭제", Location = new Point(260, 220), Size = new Size(120, 35), BackColor = Color.FromArgb(150, 50, 50), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnDelete.Click += async (s, e) => await BtnDelete_ClickAsync();
            panelUserlist.Controls.Add(btnDelete);

            // ─ 주간 근무시간 조회 ─
            panelWeekTime = new Panel() { Dock = DockStyle.Fill, BackColor = Color.FromArgb(30, 30, 30), Padding = new Padding(10) };
            panelWeekTime.Controls.Add(new Label() { Text = "주간 출근시간 조회", ForeColor = Color.White, Font = new Font("Segoe UI", 14, FontStyle.Bold), Location = new Point(90, 10), AutoSize = true });

            lbTimes = new ListBox() { Location = new Point(50, 50), Size = new Size(250, 300), BackColor = Color.FromArgb(50, 50, 50), ForeColor = Color.White };
            panelWeekTime.Controls.Add(lbTimes);

            Button btnResetWeek = new Button() { Text = "초기화", Location = new Point(120, 360), Size = new Size(100, 35), BackColor = Color.FromArgb(150, 50, 50), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnResetWeek.Click += async (s, e) =>
            {
                if (MessageBox.Show("모든 유저의 주간 출근시간을 초기화하시겠습니까?", "확인", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    await ResetWeekTimeAsync();
                }
            };
            panelWeekTime.Controls.Add(btnResetWeek);

            table.Controls.Add(panelSignupWaiting, 0, 0);
            table.Controls.Add(panelUserlist, 1, 0);
            table.Controls.Add(panelWeekTime, 2, 0);

            this.Controls.Add(table);
        }

        private async Task LoadAllDataAsync()
        {
            await LoadWaitingUsersAsync();
            await LoadAllUsersAsync();
            await LoadWeekTimesAsync();
            await LoadRidingUsersAsync();
        }

        private async Task LoadWaitingUsersAsync()
        {
            lbWaiting.Items.Clear();
            var resp = await client.From<User>().Get();
            foreach (var u in resp.Models.Where(u => u.IsApproved == false))
                lbWaiting.Items.Add($"{u.Username} | {u.Rank}");
        }

        private async void BtnApprove_Click(object sender, EventArgs e)
        {
            if (lbWaiting.SelectedItem == null) { MessageBox.Show("승인할 유저를 선택하세요."); return; }

            string selectedUsername = lbWaiting.SelectedItem.ToString().Split('|')[0].Trim();
            if (MessageBox.Show($"{selectedUsername}님의 가입을 승인하시겠습니까?", "가입 승인 확인", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

            try
            {
                var users = await client.From<User>().Where(u => u.Username == selectedUsername).Get();
                var existingUser = users.Models.FirstOrDefault();
                if (existingUser == null) { MessageBox.Show("선택된 사용자를 찾을 수 없습니다."); return; }

                existingUser.IsApproved = true;
                await client.From<User>().Where(u => u.Username == selectedUsername).Update(existingUser);

                MessageBox.Show("승인이 완료되었습니다.");
                await LoadAllDataAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show("승인 처리 실패: " + ex.Message);
            }
        }

        private async Task BtnReject_ClickAsync()
        {
            if (lbWaiting.SelectedItem == null) { MessageBox.Show("거부할 유저를 선택하세요."); return; }

            string selectedUsername = lbWaiting.SelectedItem.ToString().Split('|')[0].Trim();
            if (MessageBox.Show($"{selectedUsername}님의 가입을 거부하시겠습니까?", "가입 거부 확인", MessageBoxButtons.YesNo) != DialogResult.Yes) return;

            try
            {
                var users = await client.From<User>().Where(u => u.Username == selectedUsername).Get();
                var existingUser = users.Models.FirstOrDefault();
                if (existingUser == null) { MessageBox.Show("선택된 사용자를 찾을 수 없습니다."); return; }

                await client.From<User>().Where(u => u.Id == existingUser.Id).Delete();
                MessageBox.Show("가입이 거부되었습니다.");
                await LoadAllDataAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show("가입 거부 처리 실패: " + ex.Message);
            }
        }

        private async Task LoadAllUsersAsync()
        {
            lbUsers.Items.Clear();
            var resp = await client.From<User>().Get();
            foreach (var u in resp.Models.Where(u => u.IsApproved == true))
                lbUsers.Items.Add($"{u.Username} | {u.Rank}");
        }

        private async void LbUsers_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lbUsers.SelectedItem == null) return;
            var p = lbUsers.SelectedItem.ToString().Split('|');
            string username = p[0].Trim();

            txtName.Text = username;
            txtRank.Text = p.Length > 1 ? p[1].Trim() : "";

            await SetSelectedPkAsync(username);
        }

        private async Task SetSelectedPkAsync(string username)
        {
            try
            {
                var response = await client.From<User>().Where(u => u.Username == username).Get();
                var user = response.Models.FirstOrDefault();
                selectedPk = user != null ? user.Id : -1;
            }
            catch { selectedPk = -1; }
        }

        private async Task BtnUpdate_ClickAsync()
        {
            if (selectedPk <= 0) { MessageBox.Show("유저를 선택해주세요."); return; }

            try
            {
                var response = await client.From<User>().Where(u => u.Id == selectedPk).Get();
                var existingUser = response.Models.FirstOrDefault();
                if (existingUser == null) { MessageBox.Show("선택된 사용자를 찾을 수 없습니다."); return; }

                existingUser.Username = txtName.Text.Trim();
                existingUser.Rank = txtRank.Text.Trim();
                await client.From<User>().Where(u => u.Id == selectedPk).Update(existingUser);

                await LoadAllUsersAsync();
                await LoadWeekTimesAsync();
                await LoadRidingUsersAsync();

                if (mypageControl != null && mypageControl.currentUser.Id == existingUser.Id)
                    mypageControl.UpdateUserAsync(existingUser);

                MessageBox.Show("유저 정보가 업데이트되었습니다.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("유저 정보 업데이트 중 오류: " + ex.Message);
            }
        }

        private async Task BtnDelete_ClickAsync()
        {
            if (selectedPk <= 0) return;
            if (MessageBox.Show("선택된 유저를 삭제하시겠습니까?", "삭제 확인", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;

            try
            {
                await client.From<User>().Where(u => u.Id == selectedPk).Delete();
                selectedPk = -1;
                txtName.Clear();
                txtRank.Clear();
                await LoadAllDataAsync();
                MessageBox.Show("유저가 삭제되었습니다.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("유저 삭제 중 오류: " + ex.Message);
            }
        }

        private async Task LoadWeekTimesAsync()
        {
            lbTimes.Items.Clear();
            try
            {
                DateTime today = DateTime.Today;
                int diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
                DateTime startOfWeek = today.AddDays(-diff);
                DateTime endOfWeek = startOfWeek.AddDays(7);

                var workResp = await client.From<Work>()
                    .Filter("date", Supabase.Postgrest.Constants.Operator.GreaterThanOrEqual, startOfWeek.ToString("yyyy-MM-dd"))
                    .Filter("date", Supabase.Postgrest.Constants.Operator.LessThan, endOfWeek.ToString("yyyy-MM-dd"))
                    .Get();

                var latestPerUser = workResp.Models
                    .Where(w => DateTime.TryParse(w.Date, out _))
                    .GroupBy(w => w.UserId)
                    .Select(g => g.OrderByDescending(x => x.Date).First())
                    .OrderByDescending(x => x.WeekTotalSeconds);

                foreach (var work in latestPerUser)
                {
                    var userResp = await client.From<User>()
                        .Where(u => u.Id == work.UserId)
                        .Limit(1)
                        .Get();

                    var user = userResp.Models.FirstOrDefault();
                    if (user != null)
                    {
                        TimeSpan t = TimeSpan.FromSeconds(work.WeekTotalSeconds);
                        lbTimes.Items.Add($"{user.Username} | {(int)t.TotalHours}시간 {t.Minutes}분 {t.Seconds}초");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"주간 근무 시간 로드 실패: {ex.Message}");
            }
        }

        private async Task ResetWeekTimeAsync()
        {
            try
            {
                DateTime today = DateTime.Today;
                int diff = (int)today.DayOfWeek - (int)DayOfWeek.Monday;
                if (diff < 0) diff += 7;
                DateTime weekStart = today.AddDays(-diff);
                DateTime weekEnd = weekStart.AddDays(7).AddSeconds(-1);

                var resp = await client.From<Work>().Get();
                foreach (var work in resp.Models)
                {
                    if (DateTime.TryParse(work.Date, out DateTime workDate))
                    {
                        if (workDate >= weekStart && workDate <= weekEnd)
                        {
                            work.WeekTotalSeconds = 0;
                            await client.From<Work>().Where(w => w.Id == work.Id).Update(work);
                        }
                    }
                }

                await LoadWeekTimesAsync();
                if (main != null && main.Mypage != null)
                    main.Mypage.RefreshWorkStatus();

                MessageBox.Show("이번 주 주간 출근 시간이 초기화되었습니다.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("주간 출근 시간 초기화 실패: " + ex.Message);
            }
        }

        // ─ 탑승 중 유저 조회 ─
        private async Task LoadRidingUsersAsync()
        {
            lbRidingUsers.Items.Clear();
            try
            {
                var resp = await client.From<User>().Where(u => u.IsRiding == true).Get();
                foreach (var u in resp.Models)
                {
                    lbRidingUsers.Items.Add($"{u.Username} | {u.Level} | {u.RP}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("탑승 중 유저 목록 로드 실패: " + ex.Message);
            }
        }

        // ─ 강제 해제 버튼 ─
        private async Task BtnForceRelease_ClickAsync()
        {
            if (lbRidingUsers.SelectedItem == null)
            {
                MessageBox.Show("강제 해제할 유저를 선택하세요.");
                return;
            }

            string selectedUsername = lbRidingUsers.SelectedItem.ToString().Split('|')[0].Trim();

            if (MessageBox.Show($"{selectedUsername}님의 탑승 상태를 강제 해제하시겠습니까?", "확인", MessageBoxButtons.YesNo) != DialogResult.Yes) return;

            try
            {
                var users = await client.From<User>().Where(u => u.Username == selectedUsername).Get();
                var user = users.Models.FirstOrDefault();
                if (user != null)
                {
                    user.IsRiding = false;
                    await client.From<User>().Where(u => u.Id == user.Id).Update(user);

                    await LoadRidingUsersAsync();
                    await LoadAllUsersAsync(); // 보고서 탭/전체 유저 갱신

                    MessageBox.Show("강제 해제가 완료되었습니다.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("강제 해제 중 오류: " + ex.Message);
            }
        }
    }
}
