using System;
using System.Linq;
using System.Windows.Forms;
using Police_Intranet.Models;
using Supabase;
using BCrypt.Net; // BCrypt.Net-Next 필요

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

            // 버튼 이벤트 연결
            btnRegister.Click += BtnRegister_Click;
            btnSignin.Click += BtnSignin_Click;

            // Enter 키로 회원가입
            txtUsername.KeyDown += SignupInputs_KeyDown;
            txtPassword.KeyDown += SignupInputs_KeyDown;

            this.Load += Signup_Load;
        }

        private void Signup_Load(object sender, EventArgs e)
        {
            if (pnlContainer != null)
            {
                pnlContainer.Left = (this.ClientSize.Width - pnlContainer.Width) / 2;
                pnlContainer.Top = (this.ClientSize.Height - pnlContainer.Height) / 2;
            }
        }

        // ===================== [회원가입 버튼 클릭] =====================
        private async void BtnRegister_Click(object sender, EventArgs e)
        {
            string name = txtUsername.Text.Trim();
            string password = txtPassword.Text;

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("모든 항목을 입력하세요.", "입력 오류");
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
                // 🔹 DB에서 닉네임 중복 확인
                var existingUsers = await client.From<User>()
                    .Where(u => u.Username == name)
                    .Get();

                if (existingUsers.Models.Any())
                {
                    MessageBox.Show("이미 존재하는 닉네임입니다.", "가입 오류");
                    return;
                }

                // 🔹 새 사용자 생성
                var newUser = new User
                {
                    Username = name,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                    Rank = "일반직",
                    IsWorking = false,
                    IsApproved = false // 관리자 승인 필요
                };

                await client.From<User>().Insert(newUser);

                MessageBox.Show("회원가입 완료\n관리자의 승인이 필요합니다.", "가입 대기");

                // 🔥 회원가입 성공 → Signup 닫고 Login으로 복귀
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("회원가입 오류: " + ex.Message, "오류");
            }
        }

        // ===================== [Enter 키로 회원가입] =====================
        private void SignupInputs_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                BtnRegister_Click(sender, e);
                e.SuppressKeyPress = true;
            }
        }

        private void BtnSignin_Click(object sender, EventArgs e)
        {
            this.Hide();
            var loginForm = new Login();
            loginForm.ShowDialog();
            this.Show();
        }

    }
}
