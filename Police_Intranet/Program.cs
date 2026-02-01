using System;
using System.Windows.Forms;
using Police_Intranet.Models;
using Supabase;

namespace Police_Intranet
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            // ===============================
            // Supabase 초기화 (단 한 번)
            // ===============================
            Client client;
            try
            {
                SupabaseClient.Initialize();
                client = SupabaseClient.Instance;

                if (client == null)
                    throw new Exception("Supabase Client 인스턴스가 null입니다.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Supabase 초기화 실패\n\n" + ex,
                    "치명적 오류",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                return;
            }

            // ===============================
            // 로그인 / 회원가입 루프
            // ===============================
            while (true)
            {
                using (var loginForm = new Login())
                {
                    var result = loginForm.ShowDialog();

                    // 🔴 X 버튼 → 프로그램 종료
                    if (result == DialogResult.Cancel)
                        return;

                    // 🟡 회원가입으로 이동
                    if (result == DialogResult.Yes)
                    {
                        using (var signupForm = new Signup())
                        {
                            signupForm.ShowDialog();
                            // 회원가입 끝나면 다시 로그인으로
                            continue;
                        }
                    }

                    // 🟢 로그인 성공
                    if (result == DialogResult.OK)
                    {
                        if (loginForm.LoggedInUser == null)
                        {
                            MessageBox.Show(
                                "로그인 정보가 전달되지 않았습니다.",
                                "오류",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error
                            );
                            return;
                        }

                        // 메인폼 실행
                            Application.Run(new Main(loginForm.LoggedInUser, client));
                        return;
                    }
                }
            }
        }
    }
}
