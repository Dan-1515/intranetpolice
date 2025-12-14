using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Police_Intranet.Models;
using BCrypt.Net;
using Police_Intranet.Properties;
using Supabase;

namespace Police_Intranet
{
    public partial class Login : Form
    {
        // 로그인 성공 후 반환할 User 객체
        public User LoggedInUser { get; private set; }

        // Supabase 클라이언트
        private Client client;

        public Login()
        {
            InitializeComponent();

            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MinimizeBox = true;
            this.MaximizeBox = false;

            this.AcceptButton = btnLogin;
            this.Load += Login_Load;

            btnLogin.Click += BtnLogin_Click;
            btnRegister.Click += BtnRegister_Click;
        }

        private async void Login_Load(object sender, EventArgs e)
        {
            try
            {
                // 중앙 배치
                if (pnlContainer != null)
                {
                    pnlContainer.Left = (this.ClientSize.Width - pnlContainer.Width) / 2;
                    pnlContainer.Top = (this.ClientSize.Height - pnlContainer.Height) / 2;
                }

                // Supabase 클라이언트 초기화
                if (SupabaseClient.Instance == null)
                    await SupabaseClient.Initialize();

                client = SupabaseClient.Instance;

                // 자동 로그인 시도
                if (Settings.Default.AutoLogin && !string.IsNullOrWhiteSpace(Settings.Default.SavedUsername))
                {
                    await AttemptLoginAsync(Settings.Default.SavedUsername, autoLogin: true);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"폼 로드 중 오류: {ex.Message}");
            }
        }

        private async void BtnLogin_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("닉네임과 비밀번호를 모두 입력해주세요.", "로그인 오류", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            await AttemptLoginAsync(username, password);
        }

        private async Task AttemptLoginAsync(string username, string password = null, bool autoLogin = false)
        {
            try
            {
                if (client == null)
                {
                    if (SupabaseClient.Instance == null)
                        await SupabaseClient.Initialize();
                    client = SupabaseClient.Instance;
                }

                var result = await client.From<User>()
                                         .Where(u => u.Username == username)
                                         .Get();

                var user = result.Models.FirstOrDefault();
                if (user == null)
                {
                    if (!autoLogin) MessageBox.Show("존재하지 않는 닉네임입니다.");
                    return;
                }

                if (user.IsApproved != true)
                {
                    if (!autoLogin) MessageBox.Show("관리자의 승인이 필요합니다.");
                    return;
                }

                if (!autoLogin && !BCrypt.Net.BCrypt.Verify(password ?? "", user.PasswordHash))
                {
                    MessageBox.Show("비밀번호를 확인해주세요.");
                    return;
                }

                // ✅ 자동 로그인 설정 및 저장
                bool shouldAutoLogin = chkAutoLogin != null ? chkAutoLogin.Checked || autoLogin : autoLogin;
                Settings.Default.AutoLogin = shouldAutoLogin;
                Settings.Default.SavedUsername = shouldAutoLogin ? username : "";
                Settings.Default.Save();

                LoggedInUser = user;

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                if (!autoLogin)
                    MessageBox.Show("로그인 오류: " + ex.Message);
            }
        }

        private void BtnRegister_Click(object sender, EventArgs e)
        {
            this.Hide();
            using (var signupForm = new Signup())
            {
                if (signupForm.ShowDialog() == DialogResult.OK)
                {
                    this.Show();
                    this.BringToFront();
                }
                else
                {
                    Application.Exit();
                }
            }
        }

        // 로그아웃 시 Main에서 Login 폼을 다시 보여주기 위해 호출
        public void ShowLogin()
        {
            this.Show();
            this.BringToFront();
        }
    }
}
