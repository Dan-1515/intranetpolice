using System;
using System.Linq;
using System.Windows.Forms;
using Police_Intranet.Models;
using Supabase;
using BCrypt.Net;

namespace Police_Intranet
{
    public partial class Signup : Form
    {
        public Signup()
        {
            InitializeComponent();

            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MinimizeBox = true;
            this.MaximizeBox = false;

            this.AcceptButton = btnRegister;
            this.Load += Signup_Load;

            btnRegister.Click += BtnRegister_Click;
            btnSignin.Click += BtnSignin_Click;
        }

        private void Signup_Load(object sender, EventArgs e)
        {
            if (pnlContainer != null)
            {
                pnlContainer.Left = (this.ClientSize.Width - pnlContainer.Width) / 2;
                pnlContainer.Top = (this.ClientSize.Height - pnlContainer.Height) / 2;
            }
        }

        // ===================== [회원가입] =====================
        private async void BtnRegister_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text;
            string userIdText = txtUserid.Text.Trim();

            if (string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(userIdText))
            {
                MessageBox.Show("모든 항목을 입력하세요.", "입력 오류");
                return;
            }

            if (!int.TryParse(userIdText, out int userId))
            {
                MessageBox.Show("고유번호는 숫자만 입력하세요.", "입력 오류");
                return;
            }

            var client = SupabaseClient.Instance;
            if (client == null)
            {
                MessageBox.Show("Supabase 클라이언트가 초기화되지 않았습니다.", "오류");
                return;
            }

            try
            {
                // 🔹 닉네임 중복 체크
                var usernameExists = await client
                    .From<User>()
                    .Where(u => u.Username == username)
                    .Get();

                if (usernameExists.Models.Any())
                {
                    MessageBox.Show("이미 사용 중인 닉네임입니다.", "가입 오류");
                    return;
                }

                // 🔹 user_id 중복 체크
                var userIdExists = await client
                    .From<User>()
                    .Where(u => u.UserId == userId)
                    .Get();

                if (userIdExists.Models.Any())
                {
                    MessageBox.Show("이미 등록된 고유번호입니다.", "가입 오류");
                    return;
                }

                // 🔹 비밀번호 BCrypt 해시
                string passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

                // 🔹 사용자 생성
                var newUser = new User
                {
                    UserId = userId,
                    Username = username,
                    PasswordHash = passwordHash,
                    Rank = "일반",
                    CreatedAt = DateTime.UtcNow,
                    IsWorking = false,
                    IsApproved = false,
                    IsAdmin = false
                };

                await client.From<User>().Insert(newUser);

                MessageBox.Show(
                    "회원가입이 완료되었습니다.\n관리자 승인 후 로그인 가능합니다.",
                    "가입 완료"
                );

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("회원가입 오류:\n" + ex.Message, "오류");
            }
        }

        // ===================== [로그인으로 돌아가기] =====================
        private void BtnSignin_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
