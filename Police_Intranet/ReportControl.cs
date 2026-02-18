using Police_Intranet;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Police_Intranet.Models;
using Police_Intranet.Services;
using System.Diagnostics.Eventing.Reader;

namespace Police_Intranet
{
    public partial class ReportControl : UserControl
    {
        private Panel mainPanel;
        private Panel rightPanel;

        private Button submit;
        private Button btnClear;
        private Button btnEndRide;
        private Button btnStartRide;

        private TextBox txtPerson;
        private TextBox txtFine;
        private TextBox txtDetention;
        private TextBox txtBailFine;
        private TextBox txtBailDetention;
        private TextBox txtPeak;

        private CheckBox cbPeak;

        private ListBox lbUser;
        private ListBox lbUsers;

        private string selectedRp = "";
        private List<string> selectedCrimes = new List<string>();
        public List<string> RidingUsers { get; }

        private Button[] robberyButtons;
        private Button[] storyButtons;

        private Panel panelLeft;
        private Panel panelRight;

        private ComboBox cbLevel;
        private ComboBox cbRP;

        private Main mainForm;
        private MypageControl mypageControl; // 추가

        private User loggedInUser;
        private bool isRiding = false;

        private readonly DiscordWebhook _reportWebhook;
        private DiscordWebhook reportWebhook;

        private System.Windows.Forms.Timer rideTimer;
        // private System.Windows.Forms.Timer workinguserTimer;

        private string selectedMutder = "";


        public ReportControl(Main main, User currentUser, MypageControl mypage, DiscordWebhook Webhook)
        {
            InitializeComponent();
            InitializeRpUi();

            mainForm = main;
            loggedInUser = currentUser; // 여기서 초기화
            this.RidingUsers = main.RidingUsers;
            mypageControl = mypage;

            rideTimer = new System.Windows.Forms.Timer();
            rideTimer.Interval = 3000; // 3초마다 갱신
            rideTimer.Tick += async (s, e) => await LoadRidingUsersAsync();
            rideTimer.Start();
            _ = LoadRidingUsersAsync();
            _ = LoadUsersAsync();

            RefreshWorkingUsers();
            RefreshLbUser(); // 탭 전환 후에도 유지 가능하도록 수정
            this.reportWebhook = Webhook;
        }

        private void InitializeRpUi()
        {
            Panel topSpacer = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50,
                BackColor = Color.FromArgb(30, 30, 30)
            };

            this.Controls.Add(topSpacer);

            this.Dock = DockStyle.Fill;
            this.BackColor = Color.FromArgb(30, 30, 30);

            mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = false,
                BackColor = Color.FromArgb(30, 30, 30),
            };
            this.Controls.Add(mainPanel);
            mainPanel.BringToFront();

            // 강도RP 버튼
            Label lblRobbery = new Label
            {
                Text = "강도RP",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                Location = new Point(10, 35),
                AutoSize = true
            };
            mainPanel.Controls.Add(lblRobbery);

            // FlowLayoutPanel 설정
            FlowLayoutPanel flpRobbery = new FlowLayoutPanel
            {
                Location = new Point(10, 65),
                Size = new Size(390, 120),   // 줄바꿈 때문에 Height 증가
                AutoSize = false,
                BackColor = Color.Transparent,

                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true, // 4개 들어가면 자동 줄바꿈
                Padding = new Padding(0),
                Margin = new Padding(0)
            };

            // 강도RP 리스트
            var robberyData = new Dictionary<string, List<string>>
                {
                    { "ATM", new List<string> { "ATM"} },
                    { "편의점", new List<string> { "편의점" } },
                    { "남부빈집", new List<string> { "남부빈집" } },
                    { "보석상", new List<string> { "보석상" } },
                    { "남부은행", new List<string> { "남부은행" } },
                    { "경털1차", new List<string> { "경털1차" } },
                };

            string[] robberyNames = robberyData.Keys.ToArray();

            // 버튼 생성
            robberyButtons = robberyNames.Select(name =>
            {
                var b = CreateSelectableButton(name, robberyData[name]);
                b.Margin = new Padding(3);   // 핵심
                return b;
            }).ToArray();

            // 버튼 추가
            foreach (var btn in robberyButtons)
                flpRobbery.Controls.Add(btn);

            mainPanel.Controls.Add(flpRobbery);

            // 스토리RP
            Label lblStory = new Label
            {
                Text = "스토리RP",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),

                // 🔥 위치 내려줌 (기존 155 → 235)
                Location = new Point(10, 185),
                AutoSize = true
            };
            mainPanel.Controls.Add(lblStory);

            FlowLayoutPanel flpStory = new FlowLayoutPanel
            {
                Location = new Point(10, 205),

                Size = new Size(650, 65),
                AutoSize = false,
                BackColor = Color.Transparent,

                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true
            };

            // 스토리RP 리스트
            var storyData = new Dictionary<string, List<string>>
                {
                    { "즉흥", new List<string> { "즉흥" } },
                    { "영장", new List<string> { "영장" } },
                    { "수배 (4인미만)", new List<string> { "수배 (4인미만)" } },
                    { "수배 (4인이상)", new List<string> { "수배 (4인이상)" } },
                };

            string[] storyNames = storyData.Keys.ToArray();

            storyButtons = storyNames.Select(name =>
            {
                var b = CreateSelectableButton(name, storyData[name]);
                b.Margin = new Padding(3);   // spacing 통일
                return b;
            }).ToArray();

            // 버튼 추가
            foreach (var btn in storyButtons)
                flpStory.Controls.Add(btn);

            mainPanel.Controls.Add(flpStory);

            // ===== 묻더 계산 버튼 =====
            Label lblMutder = new Label
            {
                Text = "묻더 계산",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                Location = new Point(10, flpStory.Bottom + 15),
                AutoSize = true
            };
            // mainPanel.Controls.Add(lblMutder);

            FlowLayoutPanel flpMutder = new FlowLayoutPanel
            {
                Location = new Point(10, lblMutder.Bottom + 5),
                Size = new Size(650, 120),
                BackColor = Color.Transparent,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true
            };

            string[] mutderButtons =
                {
                    "묻더 승", "묻더 패",
                    "묻묻더 승", "묻묻더 패",
                    "묻묻묻더 승", "묻묻묻더 패"
                };

            foreach (var name in mutderButtons)
            {
                Button btn = new Button
                {
                    Text = name,
                    Size = new Size(120, 40),
                    BackColor = Color.FromArgb(60, 60, 60),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                    Margin = new Padding(4),
                    Cursor = Cursors.Hand
                };

                btn.FlatAppearance.BorderColor = Color.FromArgb(100, 100, 100);
                btn.FlatAppearance.BorderSize = 1;

                btn.Click += (s, e) =>
                {
                    // 🔥 이미 선택된 버튼을 다시 누른 경우 → 해제
                    if (selectedMutder == btn.Text)
                    {
                        btn.BackColor = Color.FromArgb(60, 60, 60);
                        selectedMutder = "";
                    }
                    else
                    {
                        // 🔥 묻더 버튼 전체 초기화
                        foreach (Button b in flpMutder.Controls)
                            b.BackColor = Color.FromArgb(60, 60, 60);

                        // 🔥 현재 버튼 선택
                        btn.BackColor = Color.FromArgb(100, 140, 240);
                        selectedMutder = btn.Text;
                    }

                    UpdateFineAndDetention();
                };

                flpMutder.Controls.Add(btn);
            }

            // mainPanel.Controls.Add(flpMutder);

            // 맥비 운행표 & 보고서
            int panelsTop = flpStory.Bottom + 20;
            int panelsHeight = 300;
            int panelsWidth = (flpStory.Width - 25) / 2;

            panelLeft = new Panel
            {
                Location = new Point(flpRobbery.Left, panelsTop),
                Size = new Size(panelsWidth, panelsHeight),
                BackColor = Color.FromArgb(30, 30, 30)
            };
            mainPanel.Controls.Add(panelLeft);

            Label lblPanelLeftTitle = new Label
            {
                Text = "🚔 맥비 운행표",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                Location = new Point(90, 5),
                AutoSize = true
            };
            panelLeft.Controls.Add(lblPanelLeftTitle);

            lbUser = new ListBox
            {
                Location = new Point(10, 40),
                Size = new Size(panelLeft.Width - 20, 200),
                BackColor = Color.FromArgb(30, 30, 30),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10F, FontStyle.Regular),
                BorderStyle = BorderStyle.FixedSingle,
                SelectionMode = SelectionMode.MultiExtended
            };
            panelLeft.Controls.Add(lbUser);
            ApplyHorizontalCenterAlign(lbUser);

            panelRight = new Panel
            {
                Location = new Point(panelLeft.Right + 10, panelsTop),
                Size = new Size(panelsWidth, panelsHeight),
                BackColor = Color.FromArgb(30, 30, 30)
            };
            mainPanel.Controls.Add(panelRight);

            Label lblPanelRightTitle = new Label
            {
                Text = "🚔 맥비 운행 보고서",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                Location = new Point(65, 5),
                AutoSize = true
            };
            panelRight.Controls.Add(lblPanelRightTitle);

            Label lblLevel = new Label
            {
                Text = "탑승자 직급 선택",
                Location = new Point(90, 45),
                Size = new Size(200, 20),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            panelRight.Controls.Add(lblLevel);

            cbLevel = new ComboBox
            {
                Location = new Point(50, 75),
                Size = new Size(200, 25),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(30, 30, 30),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10F)
            };
            cbLevel.Items.AddRange(new string[] { "고위/간부직", "일반직", "특공대(SOU)", "타격대(SCP)", "항공팀(ASD)" });
            panelRight.Controls.Add(cbLevel);

            Label lblRP = new Label
            {
                Text = "탑승자 참여 RP 선택",
                Location = new Point(80, 115),
                Size = new Size(200, 20),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            panelRight.Controls.Add(lblRP);

            cbRP = new ComboBox
            {
                Location = new Point(50, 145),
                Size = new Size(200, 25),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(30, 30, 30),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10F)
            };
            cbRP.Items.AddRange(new string[] { "출석/수배", "즉흥", "도주", "순찰" });
            panelRight.Controls.Add(cbRP);

            btnStartRide = new Button
            {
                Text = "탑승 시작",
                Size = new Size(80, 30),
                Location = new Point(110, 195),
                BackColor = Color.FromArgb(70, 70, 70),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnStartRide.FlatAppearance.BorderSize = 0;
            btnStartRide.Click += BtnStartRide_Click;
            panelRight.Controls.Add(btnStartRide);

            btnEndRide = new Button
            {
                Text = "탑승 종료",
                Size = new Size(80, 30),
                Location = new Point(110, 195),
                BackColor = Color.FromArgb(150, 50, 50),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Visible = false
            };
            btnEndRide.Click += BtnEndRide_Click;
            panelRight.Controls.Add(btnEndRide);

            rightPanel = new Panel
            {
                Location = new Point(mainPanel.Width - 250, 10),
                BackColor = Color.FromArgb(30, 30, 30),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Size = new Size(300, 600),
            };
            this.Controls.Add(rightPanel);
            rightPanel.BringToFront();

            Label lblSelectedTitle = new Label
            {
                Text = "참여자 선택",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(10, 10),
                AutoSize = true
            };
            rightPanel.Controls.Add(lblSelectedTitle);

            submit = new Button
            {
                Text = "보고서 작성",
                Size = new Size(95, 30),
                BackColor = Color.FromArgb(70, 70, 70),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Location = new Point(rightPanel.Width - 110, 5)
            };
            submit.FlatAppearance.BorderSize = 0;
            submit.MouseEnter += (s, e) => submit.BackColor = Color.FromArgb(100, 100, 100);
            submit.MouseLeave += (s, e) => submit.BackColor = Color.FromArgb(70, 70, 70);
            submit.Click += btnSubmit_Click;
            rightPanel.Controls.Add(submit);

            // Panel 생성
            Panel panel = new Panel
            {
                Location = new Point(10, submit.Bottom + 10),
                Size = new Size(rightPanel.Width - 20, 180),
                BackColor = Color.FromArgb(30, 30, 30),
            };
            rightPanel.Controls.Add(panel);

            // ListBox 생성
            lbUsers = new ListBox
            {
                Location = new Point(5, 5),
                Size = new Size(panel.Width - 10, panel.Height - 10),
                BackColor = Color.FromArgb(30, 30, 30),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.None,
                Font = new Font("Segoe UI", 10F),
                SelectionMode = SelectionMode.MultiExtended,
                DrawMode = DrawMode.OwnerDrawFixed,
                IntegralHeight = false,
                ItemHeight = 20
            };
            panel.Controls.Add(lbUsers);
            lbUsers.SelectionMode = SelectionMode.MultiExtended;
            lbUsers.DrawMode = DrawMode.OwnerDrawFixed;

            // Panel Paint에서 흰 테두리만 그리기
            panel.Paint += (s, e) =>
            {
                using (Pen pen = new Pen(Color.White, 2))
                {
                    // 펜 두께 고려해서 사각형 조정
                    Rectangle rect = new Rectangle(1, 1, panel.Width - 3, panel.Height - 3);
                    e.Graphics.DrawRectangle(pen, rect);
                }
            };

            // 아이템 커스터마이징
            lbUsers.DrawItem += (s, e) =>
            {
                if (e.Index < 0) return;

                bool selected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
                Color backColor = selected ? Color.DodgerBlue : lbUsers.BackColor;
                Color textColor = selected ? Color.White : Color.White;

                using (Brush backBrush = new SolidBrush(backColor))
                    e.Graphics.FillRectangle(backBrush, e.Bounds);

                string text = lbUsers.Items[e.Index].ToString();
                using (Brush textBrush = new SolidBrush(textColor))
                    e.Graphics.DrawString(text, e.Font, textBrush, e.Bounds.X + 5, e.Bounds.Y);
            };

            CreateFineDetentionControls();

            btnClear = new Button
            {
                Text = "초기화",
                Size = new Size(280, 30),
                Location = new Point(10, 475),
                BackColor = Color.FromArgb(70, 70, 70),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnClear.FlatAppearance.BorderColor = Color.FromArgb(100, 100, 100);
            btnClear.Click += (s, e) =>
            {
                selectedRp = "";
                selectedCrimes.Clear();
                foreach (var b in robberyButtons) b.BackColor = Color.FromArgb(60, 60, 60);
                foreach (var b in storyButtons) b.BackColor = Color.FromArgb(60, 60, 60);
                lbUsers.ClearSelected();
                txtFine.Clear();
                txtDetention.Clear();
                txtBailFine.Clear();
                txtBailDetention.Clear();
                txtPerson.Clear();
            };
            // rightPanel.Controls.Add(btnClear);

            UpdateRightPanelLocation();
            this.Resize += (s, e) => UpdateRightPanelLocation();
        }

        private async Task LoadRidingUsersAsync()
        {
            try
            {
                if (SupabaseClient.Instance == null) return;

                // DB 조회: isriding = true 인 사람만 가져옴
                var response = await SupabaseClient.Instance
                    .From<User>()
                    .Filter("isriding", Supabase.Postgrest.Constants.Operator.Equals, "true")
                    .Get();

                var ridingUsers = response.Models;

                // UI 스레드에서 리스트박스 갱신
                if (lbUser.InvokeRequired)
                {
                    lbUser.Invoke(new Action(() => UpdateRideUI(ridingUsers)));
                }
                else
                {
                    UpdateRideUI(ridingUsers);
                }
            }
            catch
            {
                // 타이머 에러는 무시 (로그만 남김)
            }
        }
        // [수정] 탑승 시작 버튼 클릭
        private async void BtnStartRide_Click(object sender, EventArgs e)
        {
            if (loggedInUser == null)
            {
                MessageBox.Show("로그인 정보가 없습니다. 다시 로그인해주세요.");
                return;
            }

            if (cbLevel.SelectedItem == null || cbRP.SelectedItem == null)
            {
                MessageBox.Show("직급과 RP를 모두 선택해주세요.");
                return;
            }

            string level = cbLevel.SelectedItem.ToString();
            string rp = cbRP.SelectedItem.ToString();

            try
            {
                // 1. 내 최신 정보를 DB에서 가져옴 (가장 안전한 방법)
                var userRes = await SupabaseClient.Instance
                    .From<User>()
                    .Filter("username", Supabase.Postgrest.Constants.Operator.Equals, loggedInUser.Username)
                    .Single();

                var myUser = userRes;

                if (myUser != null)
                {
                    // 2. 값 변경
                    myUser.IsRiding = true;
                    myUser.Level = level;
                    myUser.RP = rp;

                    // 3. 통째로 업데이트
                    await SupabaseClient.Instance
                        .From<User>()
                        .Update(myUser);

                    // 4. 즉시 목록 갱신 (타이머가 돌지만 즉각 반응을 위해 호출)
                    await LoadRidingUsersAsync();

                    // 버튼 상태 변경
                    btnStartRide.Visible = false;
                    btnEndRide.Visible = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("탑승 정보 업데이트 중 오류: " + ex.Message);
            }
        }

        // [수정] 탑승 종료 버튼 클릭
        private async void BtnEndRide_Click(object sender, EventArgs e)
        {
            if (loggedInUser == null)
            {
                MessageBox.Show("로그인 정보가 없습니다. 다시 로그인해주세요.");
                return;
            }

            try
            {
                // 1. 내 최신 정보 가져오기
                var userRes = await SupabaseClient.Instance
                    .From<User>()
                    .Filter("username", Supabase.Postgrest.Constants.Operator.Equals, loggedInUser.Username)
                    .Single();

                var myUser = userRes;

                if (myUser != null)
                {
                    // 2. 값 초기화
                    myUser.IsRiding = false;
                    myUser.Level = null; // 또는 ""
                    myUser.RP = null;    // 또는 ""

                    // 3. 통째로 업데이트
                    await SupabaseClient.Instance
                        .From<User>()
                        .Update(myUser);

                    // 4. 즉시 목록 갱신
                    await LoadRidingUsersAsync();

                    // 버튼 상태 변경 
                    btnStartRide.Visible = true;
                    btnEndRide.Visible = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("탑승 종료 업데이트 중 오류: " + ex.Message);
            }
        }

        // [수정] 외부 호출용 메서드 (이제 타이머가 자동 갱신하므로 기능 축소)
        public void RefreshLbUser()
        {
            // 수동으로 갱신하고 싶을 때 호출되도록 연결
            // _ = LoadRidingUsersAsync();
        }

        public void UpdateRideUI(List<User> ridingUsers)
        {
            lbUser.Items.Clear();
            foreach (var user in ridingUsers)
            {
                string displayText = $"{user.UserId} | {user.Username} | {user.Level} | {user.RP}";
                lbUser.Items.Add(displayText);
            }
        }

        private void CreateFineDetentionControls()
        {
            int y = lbUsers.Bottom + 50;
            int gap = 5;

            // ===== 참여자 수 =====
            Label lblPerson = CreateLabel("상대측 참여자 수", 10, y);
            rightPanel.Controls.Add(lblPerson);
            y += lblPerson.Height + 5;

            txtPerson = new TextBox
            {
                Location = new Point(10, y),
                Size = new Size(280, 25),
                BackColor = Color.FromArgb(30, 30, 30),
                ForeColor = Color.White,
                Text = "1"
            };

            txtPerson.KeyPress += (s, e) =>
            {
                if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
                    e.Handled = true;
            };
            txtPerson.TextChanged += (s, e) =>
            {
                UpdateFineAndDetention();  // 참여자 수 바뀌면 자동 계산
            };

            rightPanel.Controls.Add(txtPerson);
            y += txtPerson.Height + gap;

            // ===== 벌금 =====
            Label lblFine = CreateLabel("벌금", 10, y);
            rightPanel.Controls.Add(lblFine);
            y += lblFine.Height + 5;

            txtFine = CreateReadonlyTextBox(10, y);
            rightPanel.Controls.Add(txtFine);
            y += txtFine.Height + gap;

            // ===== 구금 =====
            Label lblDetention = CreateLabel("구금", 10, y);
            rightPanel.Controls.Add(lblDetention);
            y += lblDetention.Height + 5;

            txtDetention = CreateReadonlyTextBox(10, y);
            rightPanel.Controls.Add(txtDetention);
            y += txtDetention.Height + gap;

            // ===== 벌금 (보석금 포함) =====
            Label lblBailFine = CreateLabel("벌금 (보석금 포함)", 10, y);
            rightPanel.Controls.Add(lblBailFine);
            y += lblBailFine.Height + 5;

            txtBailFine = CreateReadonlyTextBox(10, y);
            rightPanel.Controls.Add(txtBailFine);
            y += txtBailFine.Height + gap;

            // ===== 구금 (보석금 포함) =====
            Label lblBailDetention = CreateLabel("구금 (보석금 포함)", 10, y);
            rightPanel.Controls.Add(lblBailDetention);
            y += lblBailDetention.Height + 5;

            txtBailDetention = CreateReadonlyTextBox(10, y);
            rightPanel.Controls.Add(txtBailDetention);

            Label lblPeaks = CreateLabel("감면 할 구금", 10, y + 25);
            rightPanel.Controls.Add(lblPeaks);
            y += lblPeaks.Height + 5;

            txtPeak = new TextBox
            {
                Location = new Point(10, y + 25),
                Size = new Size(280, 25),
                BackColor = Color.FromArgb(30, 30, 30),
                ForeColor = Color.White,
                Text = ""
            };
            rightPanel.Controls.Add(txtPeak);

            txtPeak.KeyPress += (s, e) =>
            {
                if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
                    e.Handled = true;
            };
            txtPeak.TextChanged += (s, e) =>
            {
                UpdateFineAndDetention();  // 수 바뀌면 자동 계산
            };

            cbPeak = new CheckBox
            {
                Text = "피크",
                Location = new Point(10, y + 50),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 10F, FontStyle.Regular)
            };
            rightPanel.Controls.Add(cbPeak);
            cbPeak.CheckedChanged += (s, e) =>
            {
                UpdateFineAndDetention();  // 체크 시 자동 계산
            };
        }

        private TextBox CreateReadonlyTextBox(int x, int y)
        {
            return new TextBox
            {
                Location = new Point(x, y),
                Size = new Size(280, 25),
                ReadOnly = true,
                BackColor = Color.FromArgb(30, 30, 30),
                ForeColor = Color.White
            };
        }

        private Label CreateLabel(string text, int x, int y)
        {
            return new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 10F, FontStyle.Regular),
                ForeColor = Color.White,
                Location = new Point(x, y),
                AutoSize = true
            };
        }

        private Button CreateSelectableButton(string text, List<string> relatedCrimes)
        {
            var btn = new Button
            {
                Text = text,
                Size = new Size(120, 40),
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                Margin = new Padding(4),
                Cursor = Cursors.Hand
            };

            btn.FlatAppearance.BorderColor = Color.FromArgb(100, 100, 100);
            btn.FlatAppearance.BorderSize = 1;

            btn.Click += (s, e) =>
            {
                foreach (var b in robberyButtons) b.BackColor = Color.FromArgb(60, 60, 60);
                foreach (var b in storyButtons) b.BackColor = Color.FromArgb(60, 60, 60);

                btn.BackColor = Color.FromArgb(100, 140, 240);
                selectedRp = btn.Text;
                selectedCrimes = new List<string>(relatedCrimes);
                UpdateFineAndDetention();
            };

            return btn;
        }

        private readonly Dictionary<(string story, string mutder), Penalty> penaltyTable
                = new()
            {
                // ===== ATM =====
            { ("ATM", "묻더 승"),     new Penalty(200_000_000, 20) },
            { ("ATM", "묻더 패"),     new Penalty(100_000_000, 0) },
            { ("ATM", "묻묻더 승"),   new Penalty(300_000_000, 30) },
            { ("ATM", "묻묻더 패"),   new Penalty(200_000_000, 0) },
            { ("ATM", "묻묻묻더 승"), new Penalty(400_000_000, 40) },
            { ("ATM", "묻묻묻더 패"), new Penalty(300_000_000, 0) },

            // ===== 편의점 =====
            { ("편의점", "묻더 승"),     new Penalty(200_000_000, 30) },
            { ("편의점", "묻더 패"),     new Penalty(100_000_000, 0) },
            { ("편의점", "묻묻더 승"),   new Penalty(300_000_000, 45) },
            { ("편의점", "묻묻더 패"),   new Penalty(200_000_000, 0) },
            { ("편의점", "묻묻묻더 승"), new Penalty(400_000_000, 60) },
            { ("편의점", "묻묻묻더 패"), new Penalty(300_000_000, 0) },

            // ===== 빈집 =====
            { ("남부빈집", "묻더 승"),     new Penalty(200_000_000, 30) },
            { ("남부빈집", "묻더 패"),     new Penalty(100_000_000, 0) },
            { ("남부빈집", "묻묻더 승"),   new Penalty(300_000_000, 45) },
            { ("남부빈집", "묻묻더 패"),   new Penalty(200_000_000, 0) },
            { ("남부빈집", "묻묻묻더 승"), new Penalty(400_000_000, 60) },
            { ("남부빈집", "묻묻묻더 패"), new Penalty(300_000_000, 0) },

            // ===== 보석상 =====
            { ("보석상", "묻더 승"),     new Penalty(300_000_000, 40) },
            { ("보석상", "묻더 패"),     new Penalty(150_000_000, 0) },
            { ("보석상", "묻묻더 승"),   new Penalty(450_000_000, 60) },
            { ("보석상", "묻묻더 패"),   new Penalty(300_000_000, 0) },
            { ("보석상", "묻묻묻더 승"), new Penalty(600_000_000, 80) },
            { ("보석상", "묻묻묻더 패"), new Penalty(450_000_000, 0) },

            // ===== 남부은행 =====
            { ("남부은행", "묻더 승"),     new Penalty(300_000_000, 60) },
            { ("남부은행", "묻더 패"),     new Penalty(150_000_000, 0) },
            { ("남부은행", "묻묻더 승"),   new Penalty(450_000_000, 90) },
            { ("남부은행", "묻묻더 패"),   new Penalty(300_000_000, 0) },
            { ("남부은행", "묻묻묻더 승"), new Penalty(600_000_000, 120) },
            { ("남부은행", "묻묻묻더 패"), new Penalty(450_000_000, 0) },

            // ===== 수배 =====
            { ("수배", "묻더 승"),     new Penalty(600_000_000, 60) },
            { ("수배", "묻더 패"),     new Penalty(300_000_000, 0) },
            { ("수배", "묻묻더 승"),   new Penalty(90_000_000, 90) },
            { ("수배", "묻묻더 패"),   new Penalty(600_000_000, 0) },
            { ("수배", "묻묻묻더 승"), new Penalty(120_000_000, 120) },
            { ("수배", "묻묻묻더 패"), new Penalty(900_000_000, 0) },

            // ===== 즉흥 ===== 
            { ("즉흥", "묻더 승"),     new Penalty(400_000_000, 60) },
            { ("즉흥", "묻더 패"),     new Penalty(200_000_000, 0) },
            { ("즉흥", "묻묻더 승"),   new Penalty(600_000_000, 90) },
            { ("즉흥", "묻묻더 패"),   new Penalty(400_000_000, 0) },
            { ("즉흥", "묻묻묻더 승"), new Penalty(800_000_000, 120) },
            { ("즉흥", "묻묻묻더 패"), new Penalty(600_000_000, 0) },

            // ===== 영장 =====
            { ("영장", "묻더 승"),     new Penalty(400_000_000, 60) },
            { ("영장", "묻더 패"),     new Penalty(200_000_000, 0) },
            { ("영장", "묻묻더 승"),   new Penalty(600_000_000, 90) },
            { ("영장", "묻묻더 패"),   new Penalty(400_000_000, 0) },
            { ("영장", "묻묻묻더 승"), new Penalty(800_000_000, 120) },
            { ("영장", "묻묻묻더 패"), new Penalty(600_000_000, 0) },

            // ===== 경털1차 =====
            { ("경털1차", "묻더 승"),     new Penalty(400_000_000, 60) },
            { ("경털1차", "묻더 패"),     new Penalty(200_000_000, 0) },
            { ("경털1차", "묻묻더 승"),   new Penalty(600_000_000, 90) },
            { ("경털1차", "묻묻더 패"),   new Penalty(400_000_000, 0) },
            { ("경털1차", "묻묻묻더 승"), new Penalty(800_000_000, 120) },
            { ("경털1차", "묻묻묻더 패"), new Penalty(600_000_000, 0) },
        };
            

        public class Penalty
        {
            public long Fine { get; set; }
            public int Detention { get; set; }

            public Penalty(long fine, int detention)
            {
                Fine = fine;
                Detention = detention;
            }
        }

        private void UpdateRightPanelLocation()
        {
            if (rightPanel == null || mainPanel == null) return;
            int x = this.Width - rightPanel.Width - 30;
            int y = (this.Height - rightPanel.Height) / 2;
            rightPanel.Location = new Point(x, y);
            mainPanel.Width = this.Width - rightPanel.Width - 40;
        }
        private void UpdateFineAndDetention()
        {
            long totalFine = 0;
            int totalDetention = 0;
            long totalBailFine = 0;
            int totalBailDetention = 0;
            long Bail = 3_000_000L;

            bool isPeak = cbPeak.Checked;
            int bailDetentionBase = isPeak ? 5 : 10;
            int BailPerMinute = 500_000;

            int Peak = 0;
            int.TryParse(txtPeak.Text.Trim(), out Peak);
            if (Peak < 0)
                Peak = 0;

            int participantCount = 1;
            if (!int.TryParse(txtPerson.Text.Trim(), out participantCount) || participantCount < 1)
                participantCount = 1;

            foreach (var crime in selectedCrimes)
            {
                if (!string.IsNullOrEmpty(selectedMutder) &&
                    penaltyTable.TryGetValue((crime, selectedMutder), out var penalty))
                {
                    totalFine = penalty.Fine * participantCount;
                    totalDetention = penalty.Detention;

                    totalBailFine = (penalty.Fine + (Peak * Bail)) * participantCount;
                    totalBailDetention = totalDetention - Peak;
                }

                else
                {
                    switch (crime)
                    {
                        case "ATM":
                            totalFine = 100_000_000L * participantCount;
                            totalDetention = 10;
                            totalBailFine = (100_000_000L + (Peak * Bail)) * participantCount;
                            totalBailDetention = totalDetention - Peak;
                            break;

                        case "편의점":
                            totalFine = 100_000_000L * participantCount;
                            totalDetention = 15;
                            totalBailFine = (100_000_000L + (Peak * Bail)) * participantCount;
                            totalBailDetention = totalDetention - Peak;
                            break;

                        case "남부빈집":
                            totalFine = 100_000_000L * participantCount;
                            totalDetention = 15;
                            totalBailFine = (100_000_000L + (Peak * Bail)) * participantCount;
                            totalBailDetention = totalDetention - Peak;
                            break;

                        case "보석상":
                            totalFine = 150_000_000L * participantCount;
                            totalDetention = 20;
                            totalBailFine = (150_000_000L + (Peak * Bail)) * participantCount;
                            totalBailDetention = totalDetention - Peak;
                            break;

                        case "남부은행":
                            totalFine = 150_000_000L * participantCount;
                            totalDetention = 30;
                            totalBailFine = (150_000_000L + (Peak * Bail)) * participantCount;
                            totalBailDetention = totalDetention - Peak;
                            break;

                        case "수배 (4인미만)":
                            totalFine = 800_000_000L * participantCount;
                            totalDetention = 50;
                            totalBailFine = (800_000_000L + (Peak * Bail)) * participantCount;
                            totalBailDetention = totalDetention - Peak;
                            break;

                        case "수배 (4인이상)":
                            totalFine = 400_000_000L * participantCount;
                            totalDetention = 50;
                            totalBailFine = (400_000_000L + (Peak * Bail)) * participantCount;
                            totalBailDetention = totalDetention - Peak;
                            break;

                        case "즉흥":
                            totalFine = 200_000_000L * participantCount;
                            totalDetention = 30;
                            totalBailFine = (200_000_000L + (Peak * Bail)) * participantCount;
                            totalBailDetention = totalDetention - Peak;
                            break;

                        case "영장":
                            totalFine = 400_000_000L * participantCount;
                            totalDetention = 50;
                            totalBailFine = (400_000_000L + (Peak * Bail)) * participantCount;
                            totalBailDetention = totalDetention - Peak;
                            break;

                        case "경털1차":
                            totalFine = 200_000_000L * participantCount;
                            totalDetention = 30;
                            totalBailFine = (200_000_000L + (Peak * Bail)) * participantCount;
                            totalBailDetention = totalDetention - Peak;
                            break;
                    }
                }
            }

            if (isPeak)
            {
                int originalDetention = totalDetention;

                // 피크 체크 시 보석 구금 5분 고정
                totalBailDetention = 5;

                int reducedMinutes = originalDetention - 5;
                if (reducedMinutes < 0)
                    reducedMinutes = 0;

                // 줄어든 분 × 50만원 × 인원수
                totalBailFine += reducedMinutes * 500_000L * participantCount;
            }

            txtFine.Text = $"{totalFine:N0}원";
            txtDetention.Text = $"{totalDetention}분";
            txtBailFine.Text = $"{totalBailFine:N0}원";
            txtBailDetention.Text = $"{totalBailDetention}분";
        }

        private async Task LoadUsersAsync()
        {
            try
            {
                // isapprove = true 필터 적용
                var response = await SupabaseClient.Instance
                    .From<User>()
                    .Get();

                var users = response.Models ?? new List<User>();

                if (lbUsers.InvokeRequired)
                    lbUsers.Invoke(() => UpdateLbUsers(users));
                else
                    UpdateLbUsers(users);
            }
            catch (Exception ex)
            {
                MessageBox.Show("유저 목록 로딩 실패: " + ex.Message);
            }
        }

        private void UpdateLbUsers(List<User> users)
        {
            lbUsers.Items.Clear();

            foreach (var user in users.OrderBy(u => u.UserId))
            {
                lbUsers.Items.Add(user);
            }
        }

        private async void btnSubmit_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(selectedRp))
                {
                    MessageBox.Show("RP를 선택해주세요.");
                    return;
                }

                // ✅ 참여 경관 문자열 (로그용)
                string participantPolice = string.Join(", ", lbUsers.SelectedItems.Cast<User>().Select(u => $"{u.UserId} {u.Username}"));
                if (string.IsNullOrWhiteSpace(participantPolice))
                    participantPolice = "없음";

                // ✅ 상대측 참여 인원 수
                string opponentCount = txtPerson.Text.Trim();
                if (string.IsNullOrWhiteSpace(opponentCount))
                    opponentCount = "0";

                if (reportWebhook == null)
                {
                    MessageBox.Show("디스코드 웹훅이 설정되지 않았습니다.");
                    return;
                }

                // 🔥 1. 디스코드 로그 전송
                await reportWebhook.SendReportLogAsync(
                    writer: loggedInUser,
                    RP: selectedRp,
                    ParticipantPolice: participantPolice,
                    participants: opponentCount
                );

                // 🔥 2. RP 참여자 RP 횟수 증가
                var participantId = lbUsers.SelectedItems
                    .Cast<User>()
                    .Select(u => u.UserId)
                    .ToList();

                foreach (var userId in participantId)
                {
                    var user = await SupabaseClient.Instance
                        .From<User>()
                        .Filter("user_id", Supabase.Postgrest.Constants.Operator.Equals, userId)
                        .Single();

                    if (user != null)
                    {
                        user.RpCount += 1;

                        await SupabaseClient.Instance
                            .From<User>()
                            .Update(user);
                    }
                }

                if (mypageControl != null)
                {
                    await mypageControl.LoadUserRanksAsync();
                }

                lbUsers.ClearSelected();
                selectedRp = "";
                selectedCrimes.Clear();

                foreach (var b in robberyButtons)
                    b.BackColor = Color.FromArgb(60, 60, 60);

                foreach (var b in storyButtons)
                    b.BackColor = Color.FromArgb(60, 60, 60);

                MessageBox.Show("보고서가 작성되었습니다.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("보고서 작성 실패\n" + ex.Message);
            }
        }
        private async Task LoadWorkingUsersAsync()
        {
            try
            {
                // 1️⃣ work에서 근무중인 user_id 가져오기
                var workRes = await SupabaseClient.Instance
                    .From<Work>()
                    .Filter("is_working", Supabase.Postgrest.Constants.Operator.Equals, true)
                    .Get();

                var userIds = workRes.Models
                    .Select(w => w.UserId)
                    .Distinct()
                    .ToList();

                if (userIds.Count == 0)
                {
                    lbUsers.Items.Clear();
                    return;
                }

                // 2️⃣ users에서 해당 유저들만 조회
                var users = new List<User>();

                foreach (var id in userIds)
                {
                    var res = await SupabaseClient.Instance
                        .From<User>()
                        .Filter("user_id", Supabase.Postgrest.Constants.Operator.Equals, id)
                        .Get();

                    if (res.Models != null)
                        users.AddRange(res.Models);
                }


                if (lbUsers.InvokeRequired)
                    lbUsers.Invoke(() => UpdateLbUsers(users));
                else
                    UpdateLbUsers(users);
            }
            catch
            {
                // 타이머용 → 조용히 무시
            }
        }

        public async void RefreshWorkingUsers()
        {
            await LoadWorkingUsersAsync();
        }

        public event Action OnRpUpdated;

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

    }
}