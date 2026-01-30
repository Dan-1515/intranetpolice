using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Police_Intranet.Models;
using BCrypt.Net;
using Police_Intranet.Properties;
using Supabase;
using System.Drawing;

namespace Police_Intranet
{
    public partial class Login : Form
    {
        public User LoggedInUser { get; private set; }
        private Client client;

        public Login()
        {
            InitializeComponent();

            if (TempWindowPosition.LastLocation.HasValue)
            {
                this.StartPosition = FormStartPosition.Manual;
                this.Location = TempWindowPosition.LastLocation.Value;
            }
            else
            {
                this.StartPosition = FormStartPosition.CenterScreen;
            }

            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MinimizeBox = true;
            this.MaximizeBox = false;

            this.AcceptButton = btnLogin;
            this.Load += Login_Load;
            this.FormClosing += Login_FormClosing;
        }

        public static class TempWindowPosition
        {
            public static Point? LastLocation;
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

                if (SupabaseClient.Instance == null)
                    await SupabaseClient.Initialize();

                client = SupabaseClient.Instance;

                // ✅ 자동 로그인 (user_id 기준)
                if (Settings.Default.AutoLoginEnabled &&
                    Settings.Default.SavedUserid > 0)
                {
                    await AttemptAutoLoginAsync(Settings.Default.SavedUserid);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"폼 로드 중 오류: {ex.Message}");
            }
        }

        private async void BtnLogin_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(txtUserid.Text.Trim(), out int userId) || userId <= 0)
            {
                MessageBox.Show("올바른 고유번호를 입력해주세요.",
                    "로그인 오류",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            string password = txtPassword.Text;

            if (string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("비밀번호를 입력해주세요.",
                    "로그인 오류",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            await AttemptLoginAsync(userId, password);
        }

        // =========================
        // 수동 로그인 (user_id + password)
        // =========================
        private async Task AttemptLoginAsync(int userId, string password)
        {
            try
            {
                if (client == null)
                {
                    if (SupabaseClient.Instance == null)
                        await SupabaseClient.Initialize();
                    client = SupabaseClient.Instance;
                }

                var result = await client
                    .From<User>()
                    .Where(u => u.UserId == userId)
                    .Get();

                var user = result.Models.FirstOrDefault();
                if (user == null)
                {
                    MessageBox.Show("고유번호를 확인해주세요.");
                    return;
                }

                if (user.IsApproved != true)
                {
                    MessageBox.Show("관리자의 승인이 필요합니다.");
                    return;
                }

                if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                {
                    MessageBox.Show("비밀번호를 확인해주세요.");
                    return;
                }

                LoggedInUser = user;

                bool shouldAutoLogin = chkAutoLogin?.Checked ?? false;
                Settings.Default.AutoLoginEnabled = shouldAutoLogin;
                Settings.Default.SavedUserid = shouldAutoLogin ? (user.UserId ?? 0) : 0;
                Settings.Default.Save();

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("로그인 오류: " + ex.Message);
            }
        }

        // =========================
        // 자동 로그인 (user_id)
        // =========================
        private async Task AttemptAutoLoginAsync(int userId)
        {
            try
            {
                if (client == null)
                {
                    if (SupabaseClient.Instance == null)
                        await SupabaseClient.Initialize();
                    client = SupabaseClient.Instance;
                }

                var result = await client
                    .From<User>()
                    .Where(u => u.UserId == userId)
                    .Get();

                var user = result.Models.FirstOrDefault();
                if (user == null) return;
                if (user.IsApproved != true) return;

                LoggedInUser = user;
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch
            {
                // 자동 로그인 실패 → 무시
            }
        }

        // =========================
        // 회원가입 버튼
        // =========================
        private void BtnRegister_Click(object sender, EventArgs e)
        {
            this.Hide();

            using (var signupForm = new Signup())
            {
                var result = signupForm.ShowDialog();

                if (result == DialogResult.OK)
                {
                    // 회원가입 후 다시 로그인
                    this.Show();
                }
                else
                {
                    // 그냥 닫으면 앱 종료
                    Application.Exit();
                }
            }
        }

        public void ClearSavedLogin()
        {
            Settings.Default.AutoLoginEnabled = false;
            Settings.Default.SavedUserid = 0;
            Settings.Default.Save();
        }

        private void Login_FormClosing(object sender, FormClosingEventArgs e)
        {
            TempWindowPosition.LastLocation = this.Location;
        }
    }
}
