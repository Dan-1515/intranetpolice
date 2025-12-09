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

            btnLogin.Click += BtnLogin_Click;
            btnRegister.Click += BtnRegister_Click;

            txtUsername.KeyDown += LoginInputs_KeyDown;
            txtPassword.KeyDown += LoginInputs_KeyDown;

            this.Load += Login_Load;
        }

        private async void Login_Load(object sender, EventArgs e)
        {
            try
            {
                if (pnlContainer != null)
                {
                    pnlContainer.Left = (this.ClientSize.Width - pnlContainer.Width) / 2;
                    pnlContainer.Top = (this.ClientSize.Height - pnlContainer.Height) / 2;
                }

                // Supabase 클라이언트 초기화
                if (SupabaseClient.Instance == null)
                    await SupabaseClient.Initialize();

                client = SupabaseClient.Instance;

                // 자동 로그인
                if (Settings.Default.AutoLogin && !string.IsNullOrWhiteSpace(Settings.Default.SavedUsername))
                {
                    string savedUsername = Settings.Default.SavedUsername;
                    await AttemptLoginAsync(savedUsername, autoLogin: true);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"폼 로드 중 오류: {ex.Message}");
            }
        }

        private async void BtnLogin_Click(object sender, EventArgs e)
        {
            string usernameInput = txtUsername.Text.Trim();
            string passwordInput = txtPassword.Text;

            if (string.IsNullOrWhiteSpace(usernameInput) || string.IsNullOrWhiteSpace(passwordInput))
            {
                MessageBox.Show("닉네임과 비밀번호를 모두 입력해주세요.", "로그인 오류", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // Supabase 클라이언트 초기화
                if (client == null)
                {
                    if (SupabaseClient.Instance == null)
                        await SupabaseClient.Initialize();

                    client = SupabaseClient.Instance;
                }

                await AttemptLoginAsync(usernameInput, passwordInput);
            }
            catch (Exception ex)
            {
                MessageBox.Show("로그인 중 오류가 발생했습니다: " + ex.Message);
            }
        }

        private async Task AttemptLoginAsync(string usernameInput, string passwordInput = null, bool autoLogin = false)
        {
            try
            {
                if (client == null)
                {
                    MessageBox.Show("Supabase 클라이언트가 초기화되지 않았습니다.");
                    return;
                }

                var result = await client.From<User>()
                                         .Where(u => u.Username == usernameInput)
                                         .Get();

                var user = result?.Models?.FirstOrDefault();

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

                if (!autoLogin && !BCrypt.Net.BCrypt.Verify(passwordInput ?? "", user.PasswordHash))
                {
                    MessageBox.Show("비밀번호를 확인해주세요.");
                    return;
                }

                // 자동 로그인 설정
                if (chkAutoLogin != null)
                {
                    Settings.Default.AutoLogin = chkAutoLogin.Checked || autoLogin;
                    Settings.Default.SavedUsername = Settings.Default.AutoLogin ? usernameInput : "";
                    Settings.Default.Save();
                }

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

        private void LoginInputs_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                BtnLogin_Click(sender, e);
                e.SuppressKeyPress = true;
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
