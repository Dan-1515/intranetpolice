using Police_Intranet.Models;
using Police_Intranet.Properties;
using Police_Intranet.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Supabase;

namespace Police_Intranet
{
    public partial class Main : Form
    {
        private UserControl currentControl;
        private List<User> workingUsers = new List<User>();
        public List<string> RidingUsers { get; private set; } = new List<string>();

        private User _currentUser;
        public MypageControl Mypage { get; private set; }

        private Label lblVersion;
        private DiscordWebhook discordWebhook;

        private Client _client;

        // 로그아웃 시 Program에서 판단하기 위한 플래그
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
                discordWebhook = new DiscordWebhook("https://discord.com/api/webhooks/1433432108264722545/4wp-I4rpcJR0FhFPnvVsUMkPAps9qi7KZa1O-n6lT3S7YzQhl5NojmznuJEUwNLfP7WY");
            }
            catch
            {
                discordWebhook = null;
            }

            try
            {
                // MypageControl 생성자 : 두 개 인자를 받음
                Mypage = new MypageControl(_currentUser, discordWebhook);
            }
            catch (Exception ex)
            {
                MessageBox.Show("MypageControl 생성 오류: " + ex.Message, "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            if (!DesignMode)
                InitializeVersionLabel();
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

        private void btnMypage_Click(object sender, EventArgs e)
        {
            if (Mypage != null)
                LoadControl(Mypage);
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

        private void btnAdmin_Click(object sender, EventArgs e)
        {
            if (_currentUser != null && _currentUser.Rank == "관리자")
            {
                LoadControl(new AdminControl(_client));
            }
            else
            {
                MessageBox.Show("권한이 없습니다.", "권한 오류", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // MainForm
        private void BtnLogout_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("로그아웃 하시겠습니까?", "확인", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                this.Hide(); // MainForm 닫지 않고 숨김

                Settings.Default.AutoLogin = false;
                Settings.Default.SavedUsername = "";
                Settings.Default.Save();

                using (var loginForm = new Login())
                {
                    if (loginForm.ShowDialog() == DialogResult.OK)
                    {
                        // 로그인 성공 → MainForm 다시 보여줌
                        this.Show();
                    }
                    else
                    {
                        // 로그인 취소 → 프로그램 종료
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

            lblVersion.Location = new Point(btnLogout.Location.X + 60, btnLogout.Location.Y + btnLogout.Height + 25);
            lblVersion.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;

            leftSidebarPanel.Controls.Add(lblVersion);

            leftSidebarPanel.Resize += (s, e) =>
            {
                lblVersion.Location = new Point(btnLogout.Location.X + 50, btnLogout.Location.Y + btnLogout.Height + 25);
            };
        }
    }
}
