using Police_Intranet.Models;
using Supabase;
using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Police_Intranet.MypageControl;
using static Supabase.Postgrest.Constants;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace Police_Intranet
{
    public partial class AdminControl : UserControl
    {
        private Supabase.Client client;

        private Panel panelSignupWaiting;
        private Panel panelUserlist;
        private Panel panelWeekTime;
        private Panel panelUsersRp;
        private Panel panelForceCheckOut;

        private ListBox lbWaiting;
        private ListBox lbUsers;
        private ListBox lbTimes;
        private ListBox lbRidingUsers;
        private ListBox lbRpReset;
        private ListBox lbCheckOut;

        private Button btnApprove;
        private Button btnReject;
        private Button btnUpdate;
        private Button btnDelete;
        private Button btnForceRelease;
        private Button btnForceSelectCheckout;
        private Button btnForceAllCheckout;

        private TextBox txtUserId;
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

        private void ApplyHorizontalCenterAlign(ListBox lb)
        {
            lb.DrawMode = DrawMode.OwnerDrawFixed;

            lb.DrawItem += (s, e) =>
            {
                if (e.Index < 0) return;

                e.DrawBackground();

                string text = lb.Items[e.Index].ToString();

                // 🔥 스크롤바 보정
                int scrollbarWidth = SystemInformation.VerticalScrollBarWidth;

                Rectangle rect = new Rectangle(
                    e.Bounds.X,
                    e.Bounds.Y,
                    e.Bounds.Width - scrollbarWidth,
                    e.Bounds.Height
                );

                using (StringFormat sf = new StringFormat())
                {
                    sf.Alignment = StringAlignment.Center;
                    sf.LineAlignment = StringAlignment.Center;

                    e.Graphics.DrawString(
                        text,
                        lb.Font,
                        new SolidBrush(lb.ForeColor),
                        rect,
                        sf
                    );
                }

                e.DrawFocusRectangle();
            };
        }

        private void InitializeUI()
        {
            this.BackColor = Color.FromArgb(30, 30, 30);
            this.Dock = DockStyle.Fill;

            TableLayoutPanel table = new TableLayoutPanel()
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 2,
                Padding = new Padding(10)
            };
            for (int i = 0; i < 3; i++)
                table.ColumnStyles.Clear();
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));

            table.RowStyles.Clear();
            table.RowStyles.Add(new RowStyle(SizeType.Percent, 60f)); // 위
            table.RowStyles.Add(new RowStyle(SizeType.Percent, 60f)); // 아래

            // ─ 회원가입 대기 ─
            panelSignupWaiting = new Panel()
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(30, 30, 30),
                Padding = new Padding(10)
            };
            panelSignupWaiting.Controls.Add(new Label()
            {
                Text = "회원가입 대기 목록",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 14,
                FontStyle.Bold),
                Location = new Point(80, 10),
                AutoSize = true
            });

            lbWaiting = new ListBox()
            {
                Location = new Point(50, 50),
                Size = new Size(250, 300),
                BackColor = Color.FromArgb(50, 50, 50),
                ForeColor = Color.White
            };
            panelSignupWaiting.Controls.Add(lbWaiting);
            ApplyHorizontalCenterAlign(lbWaiting);

            btnApprove = new Button()
            {
                Text = "가입 승인",
                Location = new Point(70, 360),
                Size = new Size(100, 35),
                BackColor = Color.FromArgb(70, 70, 70),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnApprove.Click += BtnApprove_Click;
            panelSignupWaiting.Controls.Add(btnApprove);

            btnReject = new Button()
            {
                Text = "가입 거부",
                Location = new Point(180, 360),
                Size = new Size(100, 35),
                BackColor = Color.FromArgb(150, 50, 50),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnReject.Click += async (s, e) => await BtnReject_ClickAsync();
            panelSignupWaiting.Controls.Add(btnReject);

            // ─ 탑승 중 유저 ─
            Panel panelRiding = new Panel()
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(30, 30, 30),
                Padding = new Padding(10)
            };

            Label lblRiding = new Label()
            {
                Text = "맥비 운행 관리",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Location = new Point(110, 10),
                AutoSize = true
            };
            panelRiding.Controls.Add(lblRiding);

            lbRidingUsers = new ListBox()
            {
                Location = new Point(50, lblRiding.Bottom + 10),
                Size = new Size(270, 120),
                BackColor = Color.FromArgb(50, 50, 50),
                ForeColor = Color.White
            };
            panelRiding.Controls.Add(lbRidingUsers);
            ApplyHorizontalCenterAlign(lbRidingUsers);

            btnForceRelease = new Button()
            {
                Text = "강제 해제",
                Location = new Point(110, lbRidingUsers.Bottom + 10),
                Size = new Size(100, 35),
                BackColor = Color.FromArgb(150, 50, 50),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnForceRelease.Click += async (s, e) => await BtnForceRelease_ClickAsync();
            panelRiding.Controls.Add(btnForceRelease);

            // ─ RP 횟수 초기화 ─
            Panel panelRpReset = new Panel()
            {
                Dock = DockStyle.Fill,
                Location = new Point(20, 200), // lbUsers 리스트박스 + 기존 버튼 아래 위치
                Size = new Size(350, 350),
                BackColor = Color.FromArgb(30, 30, 30)
            };

            // 라벨
            Label lblRpReset = new Label()
            {
                Text = "RP 횟수 초기화",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(50, 10),
                AutoSize = true
            };
            panelRpReset.Controls.Add(lblRpReset);

            // 리스트박스 (유저 선택)
            lbRpReset = new ListBox()
            {
                Location = new Point(10, lblRpReset.Bottom + 10),
                Size = new Size(250, 300),
                BackColor = Color.FromArgb(50, 50, 50),
                ForeColor = Color.White
            };
            panelRpReset.Controls.Add(lbRpReset);
            ApplyHorizontalCenterAlign(lbRpReset);

            // 선택 유저 초기화 버튼
            Button btnResetSelectedRp = new Button()
            {
                Text = "선택 초기화",
                Location = new Point(10, lbRpReset.Bottom + 10),
                Size = new Size(110, 35),
                BackColor = Color.FromArgb(150, 50, 50),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnResetSelectedRp.Click += async (s, e) => await ResetSelectedUserRpAsync(lbRpReset);
            panelRpReset.Controls.Add(btnResetSelectedRp);

            // 전체 초기화 버튼
            Button btnResetAllRp = new Button()
            {
                Text = "전체 초기화",
                Location = new Point(130, lbRpReset.Bottom + 10),
                Size = new Size(110, 35),
                BackColor = Color.FromArgb(150, 50, 50),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnResetAllRp.Click += async (s, e) => await ResetAllUsersRpAsync();
            panelRpReset.Controls.Add(btnResetAllRp);


            // ─ 유저 관리 ─
            panelUserlist = new Panel()
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(30, 30, 30),
                Padding = new Padding(10)
            };
            panelUserlist.Controls.Add(new Label()
            {
                Text = "전체 유저 목록",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Location = new Point(100, 10),
                AutoSize = true
            }
            );

            lbUsers = new ListBox()
            {
                Location = new Point(20, 50),
                Size = new Size(230, 300),
                BackColor = Color.FromArgb(50, 50, 50),
                ForeColor = Color.White
            };
            lbUsers.SelectedIndexChanged += LbUsers_SelectedIndexChanged;
            panelUserlist.Controls.Add(lbUsers);
            ApplyHorizontalCenterAlign(lbUsers);

            txtUserId = new TextBox() { Location = new Point(260, 50), Size = new Size(110, 25)};
            txtName = new TextBox() { Location = new Point(260, 90), Size = new Size(110, 25) };
            txtRank = new TextBox() { Location = new Point(260, 130), Size = new Size(110, 25) };
            panelUserlist.Controls.Add(txtUserId);
            panelUserlist.Controls.Add(txtName);
            panelUserlist.Controls.Add(txtRank);

            btnUpdate = new Button() { Text = "저장", Location = new Point(260, 170), Size = new Size(110, 35), BackColor = Color.FromArgb(70, 70, 70), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnUpdate.Click += async (s, e) => await BtnUpdate_ClickAsync();
            panelUserlist.Controls.Add(btnUpdate);

            btnDelete = new Button() { Text = "삭제", Location = new Point(260, 220), Size = new Size(110, 35), BackColor = Color.FromArgb(150, 50, 50), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnDelete.Click += async (s, e) => await BtnDelete_ClickAsync();
            panelUserlist.Controls.Add(btnDelete);

            // ─ 주간 근무시간 조회 ─
            panelWeekTime = new Panel() { Dock = DockStyle.Fill, BackColor = Color.FromArgb(30, 30, 30), Padding = new Padding(10) };
            panelWeekTime.Controls.Add(new Label() { Text = "주간 출근시간 조회", ForeColor = Color.White, Font = new Font("Segoe UI", 14, FontStyle.Bold), Location = new Point(90, 10), AutoSize = true });

            lbTimes = new ListBox() { Location = new Point(50, 50), Size = new Size(250, 300), BackColor = Color.FromArgb(50, 50, 50), ForeColor = Color.White };
            panelWeekTime.Controls.Add(lbTimes);
            ApplyHorizontalCenterAlign(lbTimes);

            // 선택 유저 주간 초기화 버튼
            Button btnResetSelectedWeek = new Button()
            {
                Text = "선택 초기화",
                Location = new Point(80, 360),
                Size = new Size(90, 35),
                BackColor = Color.FromArgb(150, 50, 50),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnResetSelectedWeek.Click += async (s, e) =>
            {
                if (lbTimes.SelectedItem == null)
                {
                    MessageBox.Show("초기화할 유저를 선택하세요.");
                    return;
                }

                var parts = lbTimes.SelectedItem.ToString().Split('|');

                int userNumber = int.Parse(parts[0].Trim());   // ✅ UserId(고유번호)
                string username = parts[1].Trim();             // ✅ Username

                var users = await client
                    .From<User>()
                    .Where(u => u.UserId == userNumber)         // ✅ UserId 기준
                    .Get();

                var user = users.Models.FirstOrDefault();
                if (user == null)
                {
                    MessageBox.Show("유저를 찾을 수 없습니다.");
                    return;
                }

                await ResetSelectedUserWeekTimeAsync(user.Id, user.Username); // ✅ PK 전달
            };
            panelWeekTime.Controls.Add(btnResetSelectedWeek);

            Button btnResetWeek = new Button() 
            { 
                Text = "전체 초기화", Location = new Point(180, 360), 
                Size = new Size(90, 35), 
                BackColor = Color.FromArgb(150, 50, 50), 
                ForeColor = Color.White, 
                FlatStyle = FlatStyle.Flat 
            };
            btnResetWeek.Click += async (s, e) =>
            {
                if (MessageBox.Show("모든 유저의 주간 출근시간을 초기화하시겠습니까?", "확인", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    await ResetWeekTimeAsync();
                }
            };
            panelWeekTime.Controls.Add(btnResetWeek);

            panelForceCheckOut = new Panel()
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(30, 30, 30),
                Padding = new Padding(10)
            };

            panelForceCheckOut.Controls.Add(new Label()
            {
                Text = "출근중인 유저 조회",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 14,
                FontStyle.Bold),
                Location = new Point(80, 10),
                AutoSize = true
            });

            lbCheckOut = new ListBox()
            {
                Location = new Point(50, 50),
                Size = new Size(250, 300),
                BackColor = Color.FromArgb(50, 50, 50),
                ForeColor = Color.White
            };
            panelForceCheckOut.Controls.Add(lbCheckOut);
            ApplyHorizontalCenterAlign(lbCheckOut);

            btnForceSelectCheckout = new Button()
            {
                Text = "선택 퇴근",
                Location = new Point(70, 360),
                Size = new Size(100, 35),
                BackColor = Color.FromArgb(150, 50, 50),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnForceSelectCheckout.Click += BtnForceSelectCheckout_Click;
            panelForceCheckOut.Controls.Add(btnForceSelectCheckout);

            btnForceAllCheckout = new Button()
            {
                Text = "전체 퇴근",
                Location = new Point(180, 360),
                Size = new Size(100, 35),
                BackColor = Color.FromArgb(150, 50, 50),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnForceAllCheckout.Click += BtnForceAllCheckout_Click;
            panelForceCheckOut.Controls.Add(btnForceAllCheckout);

            table.Controls.Add(panelSignupWaiting, 0, 0);
            table.Controls.Add(panelUserlist, 1, 0);
            table.Controls.Add(panelWeekTime, 2, 0);
            table.Controls.Add(panelRiding, 0, 1);
            table.Controls.Add(panelRpReset, 1, 1);
            table.Controls.Add(panelForceCheckOut, 2, 1);

            this.Controls.Add(table);
        }

        

        private async Task LoadAllDataAsync()
        {
            await LoadWaitingUsersAsync();
            await LoadAllUsersAsync();
            await LoadWeekTimesAsync();
            await LoadRidingUsersAsync();
            await LoadRpUsersAsync();
            await LoadWorkingUsersAsync();
        }

        private async Task LoadWaitingUsersAsync()
        {
            lbWaiting.Items.Clear();
            var resp = await client.From<User>().Get();
            foreach (var u in resp.Models.Where(u => u.IsApproved == false))
                lbWaiting.Items.Add($"{u.UserId} | {u.Username} | {u.Rank}");
        }

        private async void BtnApprove_Click(object sender, EventArgs e)
        {
            if (lbWaiting.SelectedItem == null)
            {
                MessageBox.Show("승인할 유저를 선택하세요.");
                return;
            }

            var parts = lbWaiting.SelectedItem.ToString().Split('|');
            string selectedUsername = parts[1].Trim(); // ✅ Username

            if (MessageBox.Show(
                $"[ {selectedUsername} ] 님의 가입을 승인하시겠습니까?",
                "가입 승인 확인",
                MessageBoxButtons.YesNo) != DialogResult.Yes)
                return;

            try
            {
                var users = await client.From<User>()
                    .Where(u => u.Username == selectedUsername)
                    .Get();

                var existingUser = users.Models.FirstOrDefault();
                if (existingUser == null)
                {
                    MessageBox.Show("선택된 유저를 찾을 수 없습니다.");
                    return;
                }

                existingUser.IsApproved = true;

                await client.From<User>()
                    .Where(u => u.Id == existingUser.Id) // ✅ PK 기준
                    .Update(existingUser);

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
            if (lbWaiting.SelectedItem == null)
            {
                MessageBox.Show("거부할 유저를 선택하세요.");
                return;
            }

            var parts = lbWaiting.SelectedItem.ToString().Split('|');
            string selectedUsername = parts[1].Trim(); // ✅ Username

            if (MessageBox.Show(
                $"[ {selectedUsername} ] 님의 가입을 거부하시겠습니까?",
                "가입 거부 확인",
                MessageBoxButtons.YesNo) != DialogResult.Yes)
                return;

            try
            {
                var users = await client.From<User>()
                    .Where(u => u.Username == selectedUsername)
                    .Get();

                var existingUser = users.Models.FirstOrDefault();
                if (existingUser == null)
                {
                    MessageBox.Show("선택된 사용자를 찾을 수 없습니다.");
                    return;
                }

                await client.From<User>()
                    .Where(u => u.Id == existingUser.Id) // ✅ PK 기준
                    .Delete();

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
                lbUsers.Items.Add($"{u.UserId} | {u.Username} | {u.Rank}");
        }

        private async void LbUsers_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lbUsers.SelectedItem == null) return;

            var parts = lbUsers.SelectedItem.ToString().Split('|');
            if (parts.Length < 3) return;

            txtUserId.Text = parts[0].Trim(); // 고유번호
            txtName.Text = parts[1].Trim(); // 이름
            txtRank.Text = parts[2].Trim(); // 직급

            await SetSelectedPkAsync(txtName.Text);
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

                existingUser.UserId = int.Parse(txtUserId.Text.Trim());
                existingUser.Username = txtName.Text.Trim();
                existingUser.Rank = txtRank.Text.Trim();
                await client.From<User>().Where(u => u.Id == selectedPk).Update(existingUser);

                await LoadAllUsersAsync();
                await LoadWeekTimesAsync();
                await LoadRidingUsersAsync();
                await LoadRpUsersAsync();
                await main.Mypage.LoadUserRanksAsync();

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

            string selectedUsername = lbUsers.SelectedItem.ToString().Split('|')[0].Trim();

            if (selectedPk <= 0) return;
            if (MessageBox.Show($"선택된 유저 [ {selectedUsername} ] 을(를) 삭제하시겠습니까?", "삭제 확인", MessageBoxButtons.YesNo) != DialogResult.Yes) return;

            try
            {
                await client.From<User>().Where(u => u.Id == selectedPk).Delete();
                selectedPk = -1;
                txtUserId.Clear();
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
                var usersResp = await client
                    .From<User>()
                    .Where(u => u.IsApproved == true)
                    .Get();

                var userDict = usersResp.Models
                    .ToDictionary(u => u.Id, u => u);

                var workResp = await client
                    .From<Work>()
                    .Get();

                var latestWorks = workResp.Models
                    .GroupBy(w => w.UserId)
                    .Select(g => g
                        .OrderByDescending(w => w.Date) // 또는 CreatedAt
                        .First())
                    .ToList();

                foreach (var work in latestWorks
                             .Where(w => userDict.ContainsKey(w.UserId))
                             .OrderByDescending(w => w.WeekTotalSeconds))
                {
                    var user = userDict[work.UserId];
                    TimeSpan t = TimeSpan.FromSeconds(work.WeekTotalSeconds);

                    lbTimes.Items.Add(
                        $"{user.UserId} | {user.Username} | {(int)t.TotalHours}시간 {t.Minutes}분 {t.Seconds}초"
                    );
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"주간 근무 시간 로드 실패: {ex.Message}");
            }
        }

        private async Task ResetWeekTimeAsync()
        {
            var result = MessageBox.Show(
                "모든 유저의 주간 근무시간을 초기화하시겠습니까?\n\n이 작업은 되돌릴 수 없습니다.",
                "초기화 확인",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (result != DialogResult.Yes)
                return;

            try
            {
                var resp = await client.From<Work>().Get();

                foreach (var work in resp.Models)
                {
                    work.WeekTotalSeconds = 0;

                    await client
                        .From<Work>()
                        .Where(w => w.UserId == work.UserId)
                        .Update(work);
                }

                await LoadWeekTimesAsync();
                await main.Mypage.LoadUserRanksAsync();

                if (main?.Mypage != null)
                    await main.Mypage.ForceReloadFromDbAsync();

                MessageBox.Show("모든 유저의 주간 근무 시간이 초기화되었습니다.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("주간 초기화 실패: " + ex.Message);
            }
        }

        // ─ 수정된 선택 유저 주간 초기화 ─
        private async Task ResetSelectedUserWeekTimeAsync(int userPkId, string username)
        {
            var result = MessageBox.Show(
                $"[ {username} ] 유저의 주간 근무시간을 초기화하시겠습니까?\n\n이 작업은 되돌릴 수 없습니다.",
                "초기화 확인",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (result != DialogResult.Yes)
                return;

            try
            {
                // 1️⃣ 해당 유저의 work 전부 조회
                var resp = await client
                    .From<Work>()
                    .Where(w => w.UserId == userPkId)   // ✅ FK = users.id
                    .Get();

                if (!resp.Models.Any())
                {
                    MessageBox.Show("근무 데이터가 존재하지 않습니다.");
                    return;
                }

                // 2️⃣ PK(id) 기준으로 하나씩 Update (이게 제일 안전)
                foreach (var work in resp.Models)
                {
                    work.WeekTotalSeconds = 0;

                    await client
                        .From<Work>()
                        .Where(w => w.Id == work.Id)   // ✅ work PK
                        .Update(work);
                }

                await LoadWeekTimesAsync();

                if (main?.Mypage != null)
                {
                    await main.Mypage.LoadUserRanksAsync();
                    await main.Mypage.ForceReloadFromDbAsync();
                }

                MessageBox.Show(
                    $"[ {username} ] 유저의 주간 근무 시간이 초기화되었습니다.",
                    "초기화 완료",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "주간 근무시간 초기화 실패:\n" + ex.Message,
                    "오류",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }


        private async Task LoadRidingUsersAsync()
        {
            lbRidingUsers.Items.Clear();
            try
            {
                var resp = await client.From<User>().Where(u => u.IsRiding == true).Get();
                foreach (var u in resp.Models)
                {
                    lbRidingUsers.Items.Add($"{u.UserId} | {u.Username} | {u.Level} | {u.RP}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("탑승 중 유저 목록 로드 실패: " + ex.Message);
            }
        }

        private async Task BtnForceRelease_ClickAsync()
        {
            if (lbRidingUsers.SelectedItem == null)
            {
                MessageBox.Show("강제 해제할 유저를 선택하세요.");
                return;
            }

            var parts = lbRidingUsers.SelectedItem.ToString().Split('|');

            int userId = int.Parse(parts[0].Trim());     // ✅ UserId
            string username = parts[1].Trim();           // 메시지용

            if (MessageBox.Show(
                $"[ {username} ] 님의 탑승 상태를 강제 해제하시겠습니까?",
                "확인",
                MessageBoxButtons.YesNo) != DialogResult.Yes)
                return;

            try
            {
                var users = await client
                    .From<User>()
                    .Where(u => u.UserId == userId)       // ✅ UserId 기준 조회
                    .Get();

                var user = users.Models.FirstOrDefault();
                if (user == null)
                {
                    MessageBox.Show("선택된 유저를 찾을 수 없습니다.");
                    return;
                }

                user.IsRiding = false;

                await client
                    .From<User>()
                    .Where(u => u.Id == user.Id)           // ✅ PK(id) 기준 Update
                    .Update(user);

                await LoadRidingUsersAsync();
                await LoadAllUsersAsync();

                MessageBox.Show("해제가 완료되었습니다.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("강제 해제 중 오류: " + ex.Message);
            }
        }

        private async Task LoadRpUsersAsync()
        {
            lbRpReset.Items.Clear();

            var resp = await client.From<User>().Get();

            var sortedUsers = resp.Models
                .Where(u => u.IsApproved == true)
                .OrderByDescending(u => u.RpCount); // 🔥 이게 진짜 RP

            foreach (var u in sortedUsers)
            {
                lbRpReset.Items.Add($"{u.UserId} | {u.Username} | RP {u.RpCount}회");
            }
        }

        private async Task ResetSelectedUserRpAsync(ListBox lb)
        {
            if (lb.SelectedItem == null)
            {
                MessageBox.Show("초기화할 유저를 선택하세요.");
                return;
            }

            var parts = lb.SelectedItem.ToString().Split('|');

            int userId = int.Parse(parts[0].Trim());   // ✅ UserId
            string username = parts[1].Trim();         // 메시지용

            if (MessageBox.Show(
                $"[ {username} ] 유저의 RP 횟수를 초기화하시겠습니까?",
                "확인",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning) != DialogResult.Yes)
                return;

            try
            {
                var usersResp = await client
                    .From<User>()
                    .Where(u => u.UserId == userId)     // ✅ UserId 기준 조회
                    .Get();

                var user = usersResp.Models.FirstOrDefault();
                if (user == null)
                {
                    MessageBox.Show("선택된 유저를 찾을 수 없습니다.");
                    return;
                }

                user.RpCount = 0;

                await client
                    .From<User>()
                    .Where(u => u.Id == user.Id)        // ✅ PK(id) 기준 Update
                    .Update(user);

                await LoadRpUsersAsync();
                await LoadAllUsersAsync();
                await LoadRidingUsersAsync();
                await main.Mypage.LoadUserRanksAsync();

                MessageBox.Show($"[ {username} ] 유저의 RP 횟수가 초기화되었습니다.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("RP 초기화 중 오류: " + ex.Message);
            }
        }

        private async Task ResetAllUsersRpAsync()
        {
            if (MessageBox.Show("모든 유저의 RP 횟수를 초기화하시겠습니까?", "확인", MessageBoxButtons.YesNo, MessageBoxIcon.None) != DialogResult.Yes)
                return;

            var resp = await client.From<User>().Get();

            foreach (var user in resp.Models)
            {
                user.RpCount = 0;
                await client.From<User>().Where(u => u.Id == user.Id).Update(user);
            }

            await LoadRpUsersAsync();
            await LoadAllUsersAsync();
            await LoadRidingUsersAsync();
            await main.Mypage.LoadUserRanksAsync();

            MessageBox.Show("모든 유저의 RP 횟수가 초기화되었습니다.");
        }

        public class ListBoxItem
        {
            public string Text { get; set; }
            public long WorkId { get; set; }
            public int UserId { get; set; }

            public override string ToString()
            {
                return Text; // ListBox에는 이것만 보임
            }
        }

        private async Task LoadWorkingUsersAsync()
        {
            lbCheckOut.Items.Clear();

            var works = await client
                .From<Work>()
                .Where(w => w.IsWorking == true)
                .Get();

            var users = await client
                .From<User>()
                .Get();

            var userDict = users.Models.ToDictionary(u => u.Id);

            foreach (var work in works.Models)
            {
                if (!userDict.TryGetValue(work.UserId, out var user))
                    continue;

                // 표시용 텍스트
                var item = new ListBoxItem
                {
                    Text = $"{user.UserId} | {user.Username}",
                    WorkId = work.Id, // 🔥 숨겨진 진짜 키
                    UserId = user.Id
                };

                lbCheckOut.Items.Add(item);
            }
        }

        private async void BtnForceSelectCheckout_Click(object? sender, EventArgs e)
        {
            if (lbCheckOut.SelectedItem is not ListBoxItem selected)
            {
                MessageBox.Show("강제퇴근할 유저를 선택하세요.");
                return;
            }

            long workId = selected.WorkId;
            int userId = selected.UserId;

            DateTime now = GetKstNow(); // 🔥 KST 통일

            // 1️⃣ work 종료
            await client
                .From<Work>()
                .Where(w => w.Id == workId && w.IsWorking == true)
                .Set(w => w.IsWorking, false)
                .Set(w => w.CheckoutTime, now)
                .Set(w => w.LastWorkStart, null)
                .Update();

            // 2️⃣ users 근무 상태 종료
            await client
                .From<User>()
                .Where(u => u.Id == userId)
                .Set(u => u.IsWorking, false)
                .Update();

            // 3⃣ 마이페이지 UI 갱신
            ForceCheckoutEventBus.Raise(userId);

            MessageBox.Show("강제퇴근 처리 완료");

            await LoadWorkingUsersAsync();
        }

        private async void BtnForceAllCheckout_Click(object? sender, EventArgs e)
        {
            MessageBox.Show("추후 업데이트 예정입니다.", "안내", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }


    }
}
