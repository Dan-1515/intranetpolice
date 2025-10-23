using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Police_Intranet
{
    public partial class Main : Form
    {
        private UserControl currentControl;
        private List<User> workingUsers = new List<User>();
        public List<string> RidingUsers { get; private set; } = new List<string>();

        // 실제 DB/컨트롤 관련 제거
        // private DB db;
        private User currentUser;

        // private ReportControl reportControl;
        // public MypageControl Mypage { get; private set; }

        private Label lblVersion;

        // ================== 생성자 ==================
        public Main(User loggedInUser)
        {
            InitializeComponent();
            InitializeFormProperties();
            currentUser = loggedInUser;

            InitializeVersionLabel();
            //LoadControl(new CalculatorControl(null));
        }

        public Main(int userId, string userName, bool isAdmin)
        {
            InitializeComponent();
            InitializeFormProperties();
            currentUser = new User
            {
                UserId = userId,
                Name = userName,
            };

            InitializeVersionLabel();
            //LoadControl(new MypageControl(db, currentUser));
        }

        public Main()
        {
            InitializeComponent();
            InitializeFormProperties();

            if (!DesignMode)
                InitializeVersionLabel();
        }

        // ================== 폼 기본 속성 초기화 ==================
        private void InitializeFormProperties()
        {
            this.StartPosition = FormStartPosition.CenterScreen;
            this.ClientSize = new Size(1450, 830);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            string iconPath = System.IO.Path.Combine(Application.StartupPath, "logo.ico");
            if (System.IO.File.Exists(iconPath))
                this.Icon = new Icon(iconPath);
        }

        // ================== 컨트롤 로드 (틀만 유지) ==================
        private void LoadControl(UserControl control)
        {
            if (currentControl != null)
                currentControl.Visible = false;

            currentControl = control;

            if (!mainPanel.Controls.Contains(control))
            {
                control.Dock = DockStyle.Fill;
                mainPanel.Controls.Add(control);
            }

            control.Visible = true;
        }

        // ================== 버튼 이벤트 핸들러 ==================
        private void btnMypage_Click(object sender, EventArgs e)
        {
            // 페이지 로드 로직 제거
        }

        private void btnCalculator_Click(object sender, EventArgs e)
        {
            //LoadControl(new CalculatorControl(null));
        }

        private void btnSideNotice_Click(object sender, EventArgs e)
        {
            //LoadControl(new SideNoticeControl(null));
        }

        private void btnReport_Click(object sender, EventArgs e)
        {
            //LoadControl(new ReportControl(currentUser, db, workingUsers, this));
            //
        }

        private void btnAdmin_Click(object sender, EventArgs e)
        {
            // 권한 체크 및 관리자 페이지 로드 제거
        }

        private void BtnLogout_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("로그아웃 하시겠습니까?", "로그아웃",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                //this.Hide();
                //Login loginForm = new Login();
                //loginForm.Show();
                //this.Close();
            }
        }

        // ================== 버전 라벨 초기화 (틀만 유지) ==================
        private void InitializeVersionLabel()
        {
            lblVersion = new Label();
            lblVersion.AutoSize = true;
            lblVersion.ForeColor = Color.LightGray;
            lblVersion.Font = new Font("Segoe UI", 8F, FontStyle.Regular);
            lblVersion.Text = "dadev";

            if (leftSidebarPanel != null && btnLogout != null)
            {
                lblVersion.Location = new Point(btnLogout.Location.X + 50, btnLogout.Location.Y + btnLogout.Height + 25);
                lblVersion.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
                leftSidebarPanel.Controls.Add(lblVersion);

                leftSidebarPanel.Resize += (s, e) =>
                {
                    lblVersion.Location = new Point(btnLogout.Location.X + 50, btnLogout.Location.Y + btnLogout.Height + 25);
                };
            }
        }
    }

    // ================== 더미 User 클래스 (디자이너 안전용) ==================
    public class User
    {
        public int UserId { get; set; }
        public string Name { get; set; } = "테스트 유저";
        public bool IsAdmin { get; set; } = false;
        public bool IsWorking { get; set; } = false;
    }
}
