using Police_Intranet.Models;
using Police_Intranet.Properties;
using Police_Intranet.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Supabase;
using System.Threading.Tasks;

namespace Police_Intranet
{
    public partial class Main : Form
    {
        private UserControl currentControl;
        private List<User> workingUsers = new List<User>();
        public List<string> RidingUsers { get; private set; } = new List<string>();

        private User _currentUser;
        public MypageControl Mypage { get; private set; }
        public AdminControl Admin { get; private set; }

        private Label lblVersion;
        private DiscordWebhook discordWebhook;
        private Client _client;

        public bool IsLogoutRequested { get; private set; } = false;

        public Main(User loggedInUser, Client client)
        {
            InitializeComponent();
            InitializeFormProperties();

            _currentUser = loggedInUser ?? throw new ArgumentNullException(nameof(loggedInUser));
            _client = client ?? throw new ArgumentNullException(nameof(client));
            this.Icon = Properties.Resource1.police;

            try
            {
                discordWebhook = new DiscordWebhook(
                    "https://discord.com/api/webhooks/1433432108264722545/4wp-I4rpcJR0FhFPnvVsUMkPAps9qi7KZa1O-n6lT3S7YzQhl5NojmznuJEUwNLfP7WY"
                );
            }
            catch
            {
                discordWebhook = null;
            }

            // ⭐ Mypage는 딱 한 번만 생성
            Mypage = new MypageControl(_currentUser, _client, discordWebhook);
            Admin = new AdminControl(_client, this, Mypage); // CurrentUser 전달

            if (!DesignMode)
                InitializeVersionLabel();

            this.Load += Main_Load; // Load 이벤트 연결
            this.FormClosing += Main_FormClosing;
        }

        // ⭐ 앱 시작 시 한 번만 초기 데이터 로드
        private async void Main_Load(object sender, EventArgs e)
        {
            if (Mypage != null)
            {
                await Mypage.InitializeAsync();
            }
        }

        private void InitializeFormProperties()
        {
            this.StartPosition = FormStartPosition.CenterScreen;
            this.ClientSize = new Size(1450, 830);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            try
            {
                string iconPath = System.IO.Path.Combine(Application.StartupPath, "resources", "logos.ico");
                if (System.IO.File.Exists(iconPath))
                    this.Icon = new Icon(iconPath);
            }
            catch { }
        }

        private void LoadControl(UserControl control)
        {
            if (control == null) return;

            if (currentControl != null)
                currentControl.Visible = false;

            currentControl = control;

            if (mainPanel != null && !mainPanel.Controls.Contains(control))
            {
                control.Dock = DockStyle.Fill;
                mainPanel.Controls.Add(control);
            }

            control.Visible = true;
        }

        // ✅ 마이페이지: 기존 인스턴스 그대로 보여주기 (초기화 ❌)
        private void btnMypage_Click(object sender, EventArgs e)
        {
            try
            {
                LoadControl(Mypage);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"마이페이지 로드 오류: {ex.Message}");
            }
        }
        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                // 1️⃣ 먼저 자동 퇴근
                if (Mypage != null)
                {
                    Mypage.ForceCheckoutIfNeededAsync();
                }
            }
            catch
            {
                // 종료 중 오류 무시
            }
            finally
            {
                // 2️⃣ 그 다음 정리
                if (Mypage != null)
                {
                    Mypage.Dispose();
                    Mypage = null;
                }
            }
        }

        private void btnCalculator_Click(object sender, EventArgs e)
        {
            LoadControl(new CalculatorControl());
        }

        private void btnSideNotice_Click(object sender, EventArgs e)
        {
            LoadControl(new SideNoticeControl());
        }

        private void btnReport_Click(object sender, EventArgs e)
        {
            if (_currentUser != null)
                LoadControl(new ReportControl(this, _currentUser));
        }

        private async void btnAdmin_Click(object sender, EventArgs e)
        {
            if (_currentUser != null && _currentUser.Rank == "관리자")
            {
                await Admin.InitializeAsync();
                LoadControl(Admin);
            }
            else
            {
                MessageBox.Show("권한이 없습니다.", "권한 오류",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BtnLogout_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("로그아웃 하시겠습니까?", "확인", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                this.Hide();

                Settings.Default.AutoLogin = false;
                Settings.Default.SavedUsername = "";
                Settings.Default.Save();

                using (var loginForm = new Login())
                {
                    if (loginForm.ShowDialog() == DialogResult.OK)
                    {
                        _currentUser = loginForm.LoggedInUser;

                        // Mypage 인스턴스는 재사용, 계정만 갱신
                        Mypage.UpdateUser(_currentUser);

                        this.Show();
                    }
                    else
                    {
                        Application.Exit();
                    }
                }
            }
        }

        private void InitializeVersionLabel()
        {
            if (leftSidebarPanel == null || btnLogout == null) return;

            lblVersion = new Label
            {
                AutoSize = true,
                ForeColor = Color.LightGray,
                Font = new Font("Segoe UI", 8F, FontStyle.Regular),
                Text = "dadev"
            };

            lblVersion.Location = new Point(
                btnLogout.Location.X + 60,
                btnLogout.Location.Y + btnLogout.Height + 25
            );

            lblVersion.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
            leftSidebarPanel.Controls.Add(lblVersion);

            leftSidebarPanel.Resize += (s, e) =>
            {
                lblVersion.Location = new Point(
                    btnLogout.Location.X + 50,
                    btnLogout.Location.Y + btnLogout.Height + 25
                );
            };
        }

        
    }
}
