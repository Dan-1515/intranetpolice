using Police_Intranet.Models;
using Police_Intranet.Properties;
using Police_Intranet.Services;
using Supabase;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Reflection;

namespace Police_Intranet
{
    public partial class Main : Form
    {
        private UserControl currentControl;
        private List<User> workingUsers = new List<User>();
        public List<string> RidingUsers { get; private set; } = new List<string>();

        private User _currentUser;
        public MypageControl Mypage { get; private set; }

        public ReportControl Report { get; private set; }

        public SideNoticeControl SideNotice { get; private set; }

        public CalculatorControl Cal { get; private set; }

        public AdminControl Admin { get; private set; }

        private Label lblVersion;
        private DiscordWebhook discordWebhook;
        private Client _client;

        public bool IsLogoutRequested { get; private set; } = false;

        public Main(User loggedInUser, Client client)
        {
            InitializeComponent();
            InitializeFormProperties();
            RestoreWindowLocation();

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

            // ⭐ 각 탭에 대하여 딱 한 번만 생성
            Mypage = new MypageControl(_currentUser, _client, discordWebhook);
            Report = new ReportControl(this, _currentUser);
            Cal = new CalculatorControl();
            SideNotice = new SideNoticeControl();
            Admin = new AdminControl(_client, this, Mypage);

            if (!DesignMode)
                InitializeVersionLabel();

            this.Load += Main_Load;
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

        private bool _isClosingHandled = false;

        private async void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_isClosingHandled)
                return;

            try
            {
                _isClosingHandled = true;
                e.Cancel = true;

                Properties.Settings.Default.WindowX = this.Location.X;
                Properties.Settings.Default.WindowY = this.Location.Y;
                Properties.Settings.Default.Save();

                if (Mypage != null)
                {
                    await Mypage.ForceCheckoutIfNeededAsync();
                    await Task.Delay(300);
                }

                this.FormClosing -= Main_FormClosing;
                this.Close();
            }
            catch
            {
                e.Cancel = false;
            }
        }

        private void btnCalculator_Click(object sender, EventArgs e)
        {
            if (_currentUser != null)
                LoadControl(Cal);
        }

        private void btnSideNotice_Click(object sender, EventArgs e)
        {
            if (_currentUser != null)
                LoadControl(SideNotice);
        }

        private void btnReport_Click(object sender, EventArgs e)
        {
            if (_currentUser != null)
                LoadControl(Report);
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
                        Mypage.UpdateUserAsync(_currentUser);
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

            string rawVersion =
                Assembly.GetExecutingAssembly()
                        .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
                        .InformationalVersion
                ?? "dadev";

            // 🔥 + 뒤 커밋 해시 제거
            string displayVersion = rawVersion.Split('+')[0];

            lblVersion = new Label
            {
                AutoSize = true,
                ForeColor = Color.LightGray,
                Font = new Font("Segoe UI", 8F, FontStyle.Regular),
                Text = displayVersion
            };

            lblVersion.Location = new Point(
                btnLogout.Location.X + 60,
                btnLogout.Location.Y + btnLogout.Height + 25
            );

            lblVersion.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
            leftSidebarPanel.Controls.Add(lblVersion);
        }

        private void RestoreWindowLocation()
        {
            int x = Properties.Settings.Default.WindowX;
            int y = Properties.Settings.Default.WindowY;

            if (x >= 0 && y >= 0)
            {
                this.StartPosition = FormStartPosition.Manual;
                this.Location = new Point(x, y);

                if (!IsLocationOnScreen(this.Location))
                    CenterToScreen();
            }
            else
            {
                CenterToScreen();
            }
        }

        private bool IsLocationOnScreen(Point location)
        {
            foreach (Screen screen in Screen.AllScreens)
            {
                if (screen.WorkingArea.Contains(location))
                    return true;
            }
            return false;
        }

        // 🔥 아이콘(로고) 더블클릭 시 앱 종료 방지
        protected override void WndProc(ref Message m)
        {
            const int WM_NCLBUTTONDBLCLK = 0x00A3;
            const int WM_SYSCOMMAND = 0x0112;
            const int HTSYSMENU = 3;
            const int SC_CLOSE = 0xF060;

            // 1️⃣ 아이콘 더블클릭 (구형 / 일부 환경)
            if (m.Msg == WM_NCLBUTTONDBLCLK && m.WParam.ToInt32() == HTSYSMENU)
            {
                return;
            }

            // 2️⃣ 아이콘 더블클릭 → 바로 SC_CLOSE 날아오는 경우 (Win11 / .NET 8)
            if (m.Msg == WM_SYSCOMMAND && (m.WParam.ToInt32() & 0xFFF0) == SC_CLOSE)
            {
                // 🔥 마우스가 아이콘 영역에 있을 때만 차단
                Point mousePos = PointToClient(Cursor.Position);

                Rectangle iconRect = new Rectangle(0, 0, SystemInformation.SmallIconSize.Width + 10, SystemInformation.CaptionHeight);

                if (iconRect.Contains(mousePos))
                {
                    return; // 아이콘 더블클릭 종료 방지
                }
            }

            base.WndProc(ref m);
        }

    }
}
