using System;
using System.Linq;
using System.Windows.Forms;
using Police_Intranet.Models;
using Police_Intranet.Properties;
using Supabase;

namespace Police_Intranet
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            // 🔹 Supabase 동기 초기화
            Client client = null;
            try
            {
                SupabaseClient.Initialize();
                client = SupabaseClient.Instance;

                if (client == null)
                {
                    MessageBox.Show("Supabase 클라이언트 초기화 실패",
                        "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Supabase 초기화 중 오류: " + ex.Message,
                    "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            while (true)
            {
                User loggedInUser = null;

                // 🔹 자동 로그인 시도
                if (Settings.Default.AutoLogin && !string.IsNullOrWhiteSpace(Settings.Default.SavedUsername))
                {
                    try
                    {
                        var result = client.From<User>()
                                           .Where(u => u.Username == Settings.Default.SavedUsername)
                                           .Get()
                                           .GetAwaiter()
                                           .GetResult(); // 동기 방식 호출

                        loggedInUser = result?.Models?.FirstOrDefault();
                    }
                    catch
                    {
                        loggedInUser = null;
                    }
                }

                // 🔹 자동 로그인 실패 → 로그인 폼 표시
                if (loggedInUser == null)
                {
                    using (var loginForm = new Login())
                    {
                        var loginResult = loginForm.ShowDialog();

                        // ⛔ 로그인창에서 X → 프로그램 종료
                        if (loginResult != DialogResult.OK || loginForm.LoggedInUser == null)
                            return;

                        loggedInUser = loginForm.LoggedInUser;
                    }
                }

                // 🔹 로그인 성공 → 메인 폼 실행
                Application.Run(new Main(loggedInUser, client));

                // Main 폼 종료 후 반복 여부 판단
                // 로그아웃 여부는 Main 내부에서 처리하도록 구현
                break; // 단순 종료 시 반복 중단
            }
        }
    }
}
