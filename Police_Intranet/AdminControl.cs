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

        private Button btnApprove;
        private Button btnUpdate;
        private Button btnDelete;

        private TextBox txtUserId;
        private TextBox txtName;
        private TextBox txtRank;

        private int selectedPk = -1;   // 선택된 유저의 PK 저장

        public AdminControl(Supabase.Client supabaseClient)
        {
            this.client = supabaseClient ?? throw new ArgumentNullException(nameof(supabaseClient));

            InitializeComponent();
            InitializeUI();

            _ = LoadAllDataAsync();
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
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
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

            btnApprove = new Button()
            {
                Text = "가입 승인",
                Location = new Point(120, 360),
                Size = new Size(100, 35),
                BackColor = Color.FromArgb(70, 70, 70),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnApprove.Click += BtnApprove_Click;
            panelSignupWaiting.Controls.Add(btnApprove);

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
            });

            lbUsers = new ListBox()
            {
                Location = new Point(20, 50),
                Size = new Size(230, 300),
                BackColor = Color.FromArgb(50, 50, 50),
                ForeColor = Color.White
            };
            lbUsers.SelectedIndexChanged += LbUsers_SelectedIndexChanged;
            panelUserlist.Controls.Add(lbUsers);

            txtUserId = new TextBox() { Location = new Point(260, 50), Size = new Size(120, 25) };
            txtName = new TextBox() { Location = new Point(260, 90), Size = new Size(120, 25) };
            txtRank = new TextBox() { Location = new Point(260, 130), Size = new Size(120, 25) };

            // panelUserlist.Controls.Add(txtUserId);
            panelUserlist.Controls.Add(txtName);
            panelUserlist.Controls.Add(txtRank);

            btnUpdate = new Button()
            {
                Text = "저장",
                Location = new Point(260, 170),
                Size = new Size(120, 35),
                BackColor = Color.FromArgb(70, 70, 70),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnUpdate.Click += async (s, e) => await BtnUpdate_ClickAsync();
            panelUserlist.Controls.Add(btnUpdate);

            btnDelete = new Button()
            {
                Text = "삭제",
                Location = new Point(260, 220),
                Size = new Size(120, 35),
                BackColor = Color.FromArgb(150, 50, 50),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnDelete.Click += async (s, e) => await BtnDelete_ClickAsync();
            panelUserlist.Controls.Add(btnDelete);

            // ─ 주간 근무시간 조회 ─
            panelWeekTime = new Panel()
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(30, 30, 30),
                Padding = new Padding(10)
            };
            panelWeekTime.Controls.Add(new Label()
            {
                Text = "주간 출근시간 조회",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Location = new Point(100, 10),
                AutoSize = true
            });

            lbTimes = new ListBox()
            {
                Location = new Point(50, 50),
                Size = new Size(250, 300),
                BackColor = Color.FromArgb(50, 50, 50),
                ForeColor = Color.White
            };
            panelWeekTime.Controls.Add(lbTimes);

            Button btnResetWeek = new Button()
            {
                Text = "초기화",
                Location = new Point(120, 360),
                Size = new Size(100, 35),
                BackColor = Color.FromArgb(70, 70, 70),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
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
        }

        // ─ 회원가입 대기 목록 로드 ─
        private async Task LoadWaitingUsersAsync()
        {
            lbWaiting.Items.Clear();
            var resp = await client.From<User>().Get();

            foreach (var u in resp.Models.Where(u => u.IsApproved == false))
                lbWaiting.Items.Add($"{u.Username} | {u.Rank}");
        }

        // ─ 회원가입 승인 ─
        private async void BtnApprove_Click(object sender, EventArgs e)
        {
            if (lbWaiting.SelectedItem == null)
            {
                MessageBox.Show("승인할 유저를 선택하세요.");
                return;
            }

            // 선택된 항목에서 Username 추출
            string selectedUsername = lbWaiting.SelectedItem.ToString().Split('|')[0].Trim();

            try
            {
                // 사용자 조회
                var users = await client.From<User>()
                                               .Where(u => u.Username == selectedUsername)
                                               .Get();
                var existingUser = users.Models.FirstOrDefault();

                if (existingUser == null)
                {
                    MessageBox.Show("선택된 사용자를 찾을 수 없습니다.");
                    return;
                }

                // 승인 처리
                existingUser.IsApproved = true;

                await client.From<User>()
                            .Where(u => u.Username == selectedUsername)
                            .Update(existingUser);

                MessageBox.Show("승인이 완료되었습니다.");

                // UI 갱신
                await LoadWaitingUsersAsync();
                await LoadAllUsersAsync();
                await LoadWeekTimesAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show("승인 처리 실패: " + ex.Message);
            }
        }


        // ─ 전체 유저 목록 로드 ─
        private async Task LoadAllUsersAsync()
        {
            lbUsers.Items.Clear();
            var resp = await client.From<User>().Get();

            foreach (var u in resp.Models.Where(u => u.IsApproved == true))
            {
                // UI에는 PK 표시 안 함
                lbUsers.Items.Add($"{u.Username} | {u.Rank}");
            }
        }

        private async void LbUsers_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lbUsers.SelectedItem == null) return;

            var p = lbUsers.SelectedItem.ToString().Split('|');
            string username = p[0].Trim();

            txtUserId.Text = ""; // UserId 컬럼 없음, 비워두거나 username 표시 가능
            txtName.Text = username;
            txtRank.Text = p.Length > 1 ? p[1].Trim() : "";

            await SetSelectedPkAsync(username);
        }

        private async Task SetSelectedPkAsync(string username)
        {
            try
            {
                var response = await client.From<User>()
                                           .Where(u => u.Username == username)
                                           .Get();

                var user = response.Models.FirstOrDefault();
                if (user != null)
                    selectedPk = user.Id;
                else
                    selectedPk = -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show("유저 선택 중 오류: " + ex.Message);
                selectedPk = -1;
            }
        }


        // ─ 유저 정보 수정 ─
        private async Task BtnUpdate_ClickAsync()
        {
            if (selectedPk <= 0)
            {
                MessageBox.Show("유저를 선택해주세요.");
                return;
            }

            try
            {
                // PK 기준으로 사용자 가져오기
                var response = await client.From<User>()
                                           .Where(u => u.Id == selectedPk)
                                           .Get();

                var existingUser = response.Models.FirstOrDefault();
                if (existingUser == null)
                {
                    MessageBox.Show("선택된 사용자를 찾을 수 없습니다.");
                    return;
                }

                // 정보 수정
                existingUser.Username = txtName.Text.Trim();
                existingUser.Rank = txtRank.Text.Trim();

                // 업데이트
                await client.From<User>()
                            .Where(u => u.Id == selectedPk)
                            .Update(existingUser);

                await LoadAllUsersAsync();
                await LoadWeekTimesAsync();
                MessageBox.Show("유저 정보가 업데이트되었습니다.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("유저 정보 업데이트 중 오류: " + ex.Message);
            }
        }

        // ─ 유저 삭제 ─
        private async Task BtnDelete_ClickAsync()
        {
            if (selectedPk <= 0) return;

            if (MessageBox.Show("선택된 유저를 삭제하시겠습니까?", "삭제 확인",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                return;

            try
            {
                // PK 기준 삭제
                await client.From<User>()
                            .Where(u => u.Id == selectedPk)
                            .Delete();

                // UI 초기화
                selectedPk = -1;
                txtName.Clear();
                txtRank.Clear();

                await LoadAllUsersAsync();
                await LoadWaitingUsersAsync();
                await LoadWeekTimesAsync();

                MessageBox.Show("유저가 삭제되었습니다.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("유저 삭제 중 오류: " + ex.Message);
            }
        }


        // ─ 주간 근무시간 조회 ─
        private async Task LoadWeekTimesAsync()
        {
            lbTimes.Items.Clear();

            var resp = await client.From<User>()
                                   .Where(u => u.IsApproved == true)
                                   .Get();

            foreach (var u in resp.Models)
            {
                long week = u.WeekTotalSeconds ?? 0;
                TimeSpan t = TimeSpan.FromSeconds(week);

                long totalHours = (long)t.TotalHours;

                lbTimes.Items.Add($"{u.Username} | {totalHours}시간 {t.Minutes}분");
            }
        }

        private async Task ResetWeekTimeAsync()
        {
            var resp = await client.From<User>().Get();
            foreach (var u in resp.Models)
            {
                await client.From<User>()
                    .Where(x => x.Id == u.Id)
                    .Update(u.GetWeekResetCopy());
            }

            await LoadWeekTimesAsync();
        }
    }
}
