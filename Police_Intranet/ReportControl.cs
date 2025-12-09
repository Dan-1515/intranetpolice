using Police_Intranet;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Police_Intranet.Models;

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

        public ReportControl(Main main, User currentUser, MypageControl mypage = null)
        {
            InitializeComponent();
            InitializeRpUi();

            mainForm = main;
            loggedInUser = currentUser; // 여기서 초기화
            this.RidingUsers = main.RidingUsers;
            mypageControl = mypage;

            RefreshWorkingUsers();
            RefreshLbUser(); // 탭 전환 후에도 유지 가능하도록 수정
        }

        private void InitializeRpUi()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.FromArgb(30, 30, 30);

            mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = false,
                BackColor = Color.FromArgb(30, 30, 30),
            };
            this.Controls.Add(mainPanel);

            // 강도RP 버튼
            Label lblRobbery = new Label
            {
                Text = "강도RP",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                Location = new Point(10, 60),
                AutoSize = true
            };
            mainPanel.Controls.Add(lblRobbery);

            // FlowLayoutPanel 설정
            FlowLayoutPanel flpRobbery = new FlowLayoutPanel
            {
                Location = new Point(10, 85),
                Size = new Size(650, 120),   // 줄바꿈 때문에 Height 증가
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
                    { "경찰서털이", new List<string> { "경찰서털이" } }
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
                Location = new Point(10, 215),
                AutoSize = true
            };
            mainPanel.Controls.Add(lblStory);

            FlowLayoutPanel flpStory = new FlowLayoutPanel
            {
                // 🔥 위치 내려줌 (기존 190 → 265)
                Location = new Point(10, 245),

                Size = new Size(650, 65),
                AutoSize = false,
                BackColor = Color.Transparent,

                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true
            };

            // 스토리RP 리스트
            var storyData = new Dictionary<string, List<string>>
                {
                    { "수배", new List<string> { "수배" } },
                    { "즉흥", new List<string> { "즉흥" } },
                    { "영장", new List<string> { "영장" } }
                };

            string[] storyNames = storyData.Keys.ToArray();

            storyButtons = storyNames.Select(name =>
            {
                var b = CreateSelectableButton(name, storyData[name]);
                b.Margin = new Padding(5);   // spacing 통일
                return b;
            }).ToArray();

            // 버튼 추가
            foreach (var btn in storyButtons)
                flpStory.Controls.Add(btn);

            mainPanel.Controls.Add(flpStory);


            // 마쯔다 운행표 & 보고서
            int panelsTop = flpStory.Bottom + 40;
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
                Text = "🚔 마쯔다 운행표",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                Location = new Point(75, 5),
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

            panelRight = new Panel
            {
                Location = new Point(panelLeft.Right + 10, panelsTop),
                Size = new Size(panelsWidth, panelsHeight),
                BackColor = Color.FromArgb(30, 30, 30)
            };
            mainPanel.Controls.Add(panelRight);

            Label lblPanelRightTitle = new Label
            {
                Text = "🚔 마쯔다 운행 보고서",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                Location = new Point(55, 5),
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
            cbLevel.Items.AddRange(new string[] { "고위/간부직", "일반직", "특공대", "타격대", "항공팀" });
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
            cbRP.Items.AddRange(new string[] { "출석/수배", "즉흥", "순찰" });
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

            submit = new Button
            {
                Text = "보고서 작성",
                Size = new Size(95, 30),
                BackColor = Color.FromArgb(70, 70, 70),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Location = new Point(rightPanel.Width - 110, 10)
            };
            submit.FlatAppearance.BorderSize = 0;
            submit.MouseEnter += (s, e) => submit.BackColor = Color.FromArgb(100, 100, 100);
            submit.MouseLeave += (s, e) => submit.BackColor = Color.FromArgb(70, 70, 70);
            // submit.Click += btnSubmit_Click;
            // rightPanel.Controls.Add(submit);

            Label lblSelectedTitle = new Label
            {
                Text = "참여자 선택",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(5, 14),
                AutoSize = true
            };
            // rightPanel.Controls.Add(lblSelectedTitle);

            lbUsers = new ListBox
            {
                Location = new Point(10, submit.Bottom + 10),
                Size = new Size(rightPanel.Width - 20, 200),
                BackColor = Color.FromArgb(30, 30, 30),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10F, FontStyle.Regular),
                BorderStyle = BorderStyle.FixedSingle,
                SelectionMode = SelectionMode.MultiExtended,
                DrawMode = DrawMode.OwnerDrawFixed,
                ItemHeight = 24,
            };
            lbUsers.DrawItem += (s, e) =>
            {
                if (e.Index < 0) return;
                e.DrawBackground();
                string text = lbUsers.Items[e.Index].ToString();
                using (Brush brush = new SolidBrush(e.ForeColor))
                    e.Graphics.DrawString(text, e.Font, brush, e.Bounds);
                e.DrawFocusRectangle();
            };
            // rightPanel.Controls.Add(lbUsers);

            // if (loggedInUser != null)
            //    lbUsers.Items.Add($"{loggedInUser.UserId} | {loggedInUser.Name}");

            CreateFineDetentionControls();

            btnClear = new Button
            {
                Text = "초기화",
                Size = new Size(280, 30),
                Location = new Point(10, 305),
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
            rightPanel.Controls.Add(btnClear);

            UpdateRightPanelLocation();
            this.Resize += (s, e) => UpdateRightPanelLocation();
        }

        // BtnStartRide 클릭 이벤트
        // 탑승 시작
        private async void BtnStartRide_Click(object sender, EventArgs e)
        {
            if (loggedInUser == null)
            {
                MessageBox.Show("로그인 정보가 없습니다. 다시 로그인해주세요.");
                return;
            }

            if (cbLevel.SelectedItem == null)
            {
                MessageBox.Show("탑승자의 직급을 선택해주세요.");
                return;
            }

            if (cbRP.SelectedItem == null)
            {
                MessageBox.Show("참여하는 RP를 선택해주세요.");
                return;
            }

            string name = loggedInUser.Username;
            string level = cbLevel.SelectedItem.ToString();
            string rp = cbRP.SelectedItem.ToString();

            // 이미 탑승 중인지 확인 (Main.RidingUsers 기준)
            if (mainForm.RidingUsers.Any(u => u.StartsWith(name + " |")))
            {
                MessageBox.Show("이미 탑승 중입니다.");
                return;
            }

            string rideInfo = $"{name} | {level} | {rp}";

            // UI 업데이트
            lbUser.Items.Add(rideInfo);

            // 외부 리스트 업데이트
            mainForm.RidingUsers.Add(rideInfo);

            // 버튼 상태 업데이트
            isRiding = true;
            btnStartRide.Visible = false;
            btnEndRide.Visible = true;

            // Supabase DB 업데이트
            if (SupabaseClient.Instance == null)
            {
                MessageBox.Show("DB 클라이언트가 초기화되지 않았습니다.\n프로그램을 다시 시작해주세요.");
                return;
            }

            try
            {
                await SupabaseClient.Instance
                    .From<User>()
                    .Where(u => u.Username == loggedInUser.Username)
                    .Update(new User
                    {
                        IsRiding = true,
                        Level = level,
                        RP = rp
                    });
            }
            catch (Exception ex)
            {
                MessageBox.Show("탑승 정보 업데이트 중 오류: " + ex.Message);
            }
        }

        // 탑승 종료
        private async void BtnEndRide_Click(object sender, EventArgs e)
        {
            if (loggedInUser == null)
            {
                MessageBox.Show("로그인 정보가 없습니다. 다시 로그인해주세요.");
                return;
            }

            string name = loggedInUser.Username;
            int indexToRemove = mainForm.RidingUsers.FindIndex(u => u.StartsWith(name + " |"));

            if (indexToRemove != -1)
            {
                // UI 업데이트
                lbUser.Items.RemoveAt(indexToRemove);

                // 외부 리스트 업데이트
                mainForm.RidingUsers.RemoveAt(indexToRemove);
            }
            else
            {
                MessageBox.Show("현재 탑승 정보가 없습니다.");
            }

            // 버튼 상태 업데이트
            isRiding = false;
            btnStartRide.Visible = true;
            btnEndRide.Visible = false;

            // Supabase DB 업데이트
            if (SupabaseClient.Instance == null)
            {
                MessageBox.Show("DB 클라이언트가 초기화되지 않았습니다.\n프로그램을 다시 시작해주세요.");
                return;
            }

            try
            {
                await SupabaseClient.Instance
                    .From<User>()
                    .Where(u => u.Username == loggedInUser.Username)
                    .Update(new User
                    {
                        IsRiding = false,
                        Level = null,
                        RP = null
                    });
            }
            catch (Exception ex)
            {
                MessageBox.Show("탑승 종료 업데이트 중 오류: " + ex.Message);
            }
        }

        // ReportControl 표시 시 호출
        public void RefreshLbUser()
        {
            lbUser.Items.Clear();
            foreach (var rideInfo in mainForm.RidingUsers)
            {
                lbUser.Items.Add(rideInfo);
            }

            // 현재 유저 상태 체크 후 버튼 상태 업데이트
            string name = loggedInUser?.Username;
            if (name != null && mainForm.RidingUsers.Any(u => u.StartsWith(name + " |")))
            {
                isRiding = true;
                btnStartRide.Visible = false;
                btnEndRide.Visible = true;
            }
            else
            {
                isRiding = false;
                btnStartRide.Visible = true;
                btnEndRide.Visible = false;
            }
        }

        private void CreateFineDetentionControls()
        {
            Label lblCheck = new Label
            {
                Text = "벌금/구금 확인",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(5, 14),
                AutoSize = true
            };
            rightPanel.Controls.Add(lblCheck);

            Label lblPerson = new Label
            {
                Text = "상대측 참여 인원 수",
                Font = new Font("Segoe UI", 10F, FontStyle.Regular),
                ForeColor = Color.White,
                Location = new Point(5, 45),
                AutoSize = true
            };
            rightPanel.Controls.Add(lblPerson);

            txtPerson = new TextBox
            {
                Location = new Point(10, 65),
                Size = new Size(280, 25),
                BackColor = Color.FromArgb(30, 30, 30),
                ForeColor = Color.White
            };
            txtPerson.KeyPress += (sender, e) =>
            {
                if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar)) e.Handled = true;
            };
            txtPerson.TextChanged += (s, e) => UpdateFineAndDetention();
            rightPanel.Controls.Add(txtPerson);

            txtFine = CreateReadonlyTextBox(10, 115);
            rightPanel.Controls.Add(CreateLabel("벌금", 5, 95));
            rightPanel.Controls.Add(txtFine);

            txtDetention = CreateReadonlyTextBox(10, 165);
            rightPanel.Controls.Add(CreateLabel("구금", 5, 145));
            rightPanel.Controls.Add(txtDetention);

            txtBailFine = CreateReadonlyTextBox(10, 215);
            rightPanel.Controls.Add(CreateLabel("벌금 (보석금 포함)", 5, 195));
            rightPanel.Controls.Add(txtBailFine);

            txtBailDetention = CreateReadonlyTextBox(10, 265);
            rightPanel.Controls.Add(CreateLabel("구금 (보석금 포함)", 5, 245));
            rightPanel.Controls.Add(txtBailDetention);
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
            int totalBailDetention = 10;

            int participantCount = 1;
            if (!int.TryParse(txtPerson.Text.Trim(), out participantCount) || participantCount < 1)
                participantCount = 1;

            foreach (var crime in selectedCrimes)
            {
                switch (crime)
                {
                    case "ATM":
                        totalFine = 100_000_000L;
                        totalDetention = 10;
                        totalBailFine = 100_000_000L + (participantCount * 100_000_000L);
                        break;

                    case "편의점":
                        totalFine = 200_000_000L;
                        totalDetention = 10;
                        totalBailFine = 200_000_000L + (participantCount * 100_000_000L);
                        break;

                    case "남부빈집":
                        totalFine = 100_000_000L;
                        totalDetention = 10;
                        totalBailFine = 100_000_000L + (participantCount * 100_000_000L);
                        break;

                    case "보석상":
                        totalFine = 1_200_000_000L;
                        totalDetention = 30;
                        totalBailFine = 1_200_000_000L + (participantCount * 200_000_000L);
                        break;

                    case "남부은행":
                        totalFine = 1_200_000_000L;
                        totalDetention = 30;
                        totalBailFine = 1_200_000_000L + (participantCount * 200_000_000L);
                        break;

                    case "경찰서털이":
                        totalFine = 500_000_000L * participantCount;
                        totalDetention = 30;
                        totalBailFine = (500_000_000L + 200_000_000L) * participantCount;
                        break;

                    case "수배":
                        totalFine = 300_000_000L * participantCount;
                        totalDetention = 20;
                        totalBailFine = (300_000_000L + 100_000_000L) * participantCount;
                        break;

                    case "즉흥":
                        totalFine = 400_000_000L * participantCount;
                        totalDetention = 25;
                        totalBailFine = (400_000_000L + 150_000_000L) * participantCount;
                        break;

                    case "영장":
                        totalFine = 500_000_000L * participantCount;
                        totalDetention = 30;
                        totalBailFine = (500_000_000L + 200_000_000L) * participantCount;
                        break;
                }
            }

            txtFine.Text = $"{totalFine:N0}원";
            txtDetention.Text = $"{totalDetention}분";
            txtBailFine.Text = $"{totalBailFine:N0}원";
            txtBailDetention.Text = $"{totalBailDetention}분";
        }


        public void RefreshWorkingUsers()
        {
            lbUsers.Items.Clear();

        }
        public event Action OnRpUpdated;

    }
}
